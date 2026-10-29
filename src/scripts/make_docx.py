import os
import re
import io
import zipfile
import xml.etree.ElementTree as ET

DOCX_TEMPLATE = "Rapport_Projet_P_FUN.docx"
MD_PATH = "Rapport_Projet_P_FUN.md"
OUTPUT_DOCX = "Rapport_Projet_P_FUN.docx"

def escape_xml(text):
    return (text.replace("&", "&amp;")
                .replace("<", "&lt;")
                .replace(">", "&gt;")
                .replace('"', "&quot;"))

def parse_runs(text):
    """Split text by formatting markers: links [label](url), bold **b**, italic *i*, and code `c`."""
    pattern = r"(\[.*?\]\(.*?\)|\*\*.*?\*\*|\*.*?\*|`.*?`)"
    parts = re.split(pattern, text)
    runs_xml = []
    
    for part in parts:
        if not part:
            continue
            
        # Markdown Link: [label](url)
        link_m = re.match(r"^\[(.*?)\]\((.*?)\)$", part)
        if link_m:
            label = link_m.group(1).strip()
            url = link_m.group(2).strip()
            # If anchor like #1-introduction
            if url.startswith("#"):
                clean_lbl = escape_xml(label)
                runs_xml.append(f'<w:r><w:rPr><w:color w:val="0F172A"/></w:rPr><w:t xml:space="preserve">{clean_lbl}</w:t></w:r>')
            elif label == url:
                clean_url = escape_xml(url)
                runs_xml.append(f'<w:r><w:rPr><w:rFonts w:ascii="Consolas" w:hAnsi="Consolas"/><w:color w:val="2563EB"/><w:u w:val="single"/></w:rPr><w:t xml:space="preserve">{clean_url}</w:t></w:r>')
            else:
                clean_lbl = escape_xml(label)
                clean_url = escape_xml(url)
                runs_xml.append(f'<w:r><w:rPr><w:b/><w:color w:val="2563EB"/><w:u w:val="single"/></w:rPr><w:t xml:space="preserve">{clean_lbl}</w:t></w:r>')
                runs_xml.append(f'<w:r><w:rPr><w:color w:val="64748B"/></w:rPr><w:t xml:space="preserve"> ({clean_url})</w:t></w:r>')
            continue
            
        # Bold: **bold**
        if part.startswith("**") and part.endswith("**") and len(part) >= 4:
            clean = escape_xml(part[2:-2])
            runs_xml.append(f'<w:r><w:rPr><w:b/></w:rPr><w:t xml:space="preserve">{clean}</w:t></w:r>')
            continue
            
        # Italic: *italic*
        if part.startswith("*") and part.endswith("*") and len(part) >= 2 and not part.startswith("**"):
            clean = escape_xml(part[1:-1])
            runs_xml.append(f'<w:r><w:rPr><w:i/></w:rPr><w:t xml:space="preserve">{clean}</w:t></w:r>')
            continue
            
        # Code: `code`
        if part.startswith("`") and part.endswith("`") and len(part) >= 2:
            clean = escape_xml(part[1:-1])
            runs_xml.append(f'<w:r><w:rPr><w:rFonts w:ascii="Consolas" w:hAnsi="Consolas"/><w:color w:val="0F172A"/><w:shd w:val="clear" w:color="auto" w:fill="F1F5F9"/></w:rPr><w:t xml:space="preserve">{clean}</w:t></w:r>')
            continue
            
        # Plain text
        clean = escape_xml(part)
        runs_xml.append(f'<w:r><w:t xml:space="preserve">{clean}</w:t></w:r>')
        
    return "".join(runs_xml)

def create_paragraph(style, text, custom_pPr=None):
    runs = parse_runs(text)
    pPr_xml = ""
    if style or custom_pPr:
        pStyle = f'<w:pStyle w:val="{style}"/>' if style else ''
        extra = custom_pPr if custom_pPr else ''
        pPr_xml = f'<w:pPr>{pStyle}{extra}</w:pPr>'
    return f'<w:p>{pPr_xml}{runs}</w:p>'

def create_table(table_rows):
    if not table_rows:
        return ""
    xml = ['<w:tbl>',
           '<w:tblPr><w:tblStyle w:val="Grilledutableau"/><w:tblW w:w="0" w:type="auto"/><w:tblLook w:val="04A0"/></w:tblPr>']
    for r_idx, row in enumerate(table_rows):
        xml.append('<w:tr>')
        for c in row:
            runs = parse_runs(c.strip())
            bg = '<w:shd w:val="clear" w:color="auto" w:fill="F1F5F9"/>' if r_idx == 0 else ''
            bld = '<w:rPr><w:b/></w:rPr>' if r_idx == 0 else ''
            xml.append(f'<w:tc><w:tcPr>{bg}<w:tcMar><w:top w:w="120" w:type="dxa"/><w:bottom w:w="120" w:type="dxa"/><w:left w:w="160" w:type="dxa"/><w:right w:w="160" w:type="dxa"/></w:tcMar></w:tcPr><w:p>{runs}</w:p></w:tc>')
        xml.append('</w:tr>')
    xml.append('</w:tbl>')
    return "".join(xml)

def strip_heading_number(title):
    """Strip leading numbers like '1 ', '1.1 ', '7.5.1 ' because Word's styles automatically add them."""
    return re.sub(r"^\d+(\.\d+)*\s*", "", title)

def build_body(md_content):
    lines = md_content.splitlines()
    body_elements = []
    
    in_table = False
    current_table = []
    in_toc = False
    
    for line in lines:
        stripped = line.strip()
        
        # Check table
        if stripped.startswith("|") and stripped.endswith("|"):
            parts = [p.strip() for p in stripped.split("|")[1:-1]]
            if all(re.match(r"^:?-+:?$", p) for p in parts if p):
                continue
            if not in_table:
                in_table = True
                current_table = []
            current_table.append(parts)
            continue
        else:
            if in_table:
                body_elements.append(create_table(current_table))
                in_table = False
                current_table = []
                
        if not stripped:
            continue
            
        # Horizontal rule -> page break or separator
        if stripped == "---":
            body_elements.append('<w:p><w:pPr><w:pBdr><w:bottom w:val="single" w:sz="6" w:space="12" w:color="CBD5E1"/></w:pBdr></w:pPr></w:p>')
            continue
            
        # Code fence
        if stripped.startswith("```"):
            continue
            
        # Skip image markdown entirely
        if stripped.startswith("!["):
            continue
            
        # Blockquote / Alert
        if stripped.startswith(">"):
            clean = re.sub(r"^>\s*(\[!.*?\])?\s*", "", stripped)
            custom_pPr = '<w:pBdr><w:left w:val="single" w:sz="18" w:space="12" w:color="0284C7"/></w:pBdr><w:shd w:val="clear" w:color="auto" w:fill="F0F9FF"/><w:ind w:left="240" w:right="240"/><w:spacing w:before="120" w:after="120"/>'
            body_elements.append(create_paragraph(None, clean, custom_pPr=custom_pPr))
            continue
            
        # Table of contents section
        if stripped == "## Table des matières":
            in_toc = True
            # Title for TOC without heading number
            custom_pPr = '<w:spacing w:before="360" w:after="180"/>'
            body_elements.append(create_paragraph("Titrecentr", "**Table des matières**", custom_pPr=custom_pPr))
            continue
            
        if in_toc:
            # If next major heading starts, exit TOC
            if stripped.startswith("## ") and stripped != "## Table des matières":
                in_toc = False
            else:
                # Format TOC item
                m = re.match(r"^(\s*)-\s*\[(.*?)\]\(.*?\)", line)
                if m:
                    leading_spaces = len(m.group(1))
                    item_text = m.group(2).strip()
                    if leading_spaces >= 4:
                        custom_pPr = '<w:ind w:left="720"/><w:spacing w:before="30" w:after="30"/>'
                        body_elements.append(create_paragraph(None, item_text, custom_pPr=custom_pPr))
                    elif leading_spaces >= 2:
                        custom_pPr = '<w:ind w:left="360"/><w:spacing w:before="40" w:after="40"/>'
                        body_elements.append(create_paragraph(None, item_text, custom_pPr=custom_pPr))
                    else:
                        custom_pPr = '<w:ind w:left="0"/><w:spacing w:before="80" w:after="40"/>'
                        body_elements.append(create_paragraph(None, f"**{item_text}**", custom_pPr=custom_pPr))
                    continue
                    
        # Headings (stripping manual numbers so Word adds its own without duplicating)
        if stripped.startswith("# "):
            title_text = stripped[2:].strip()
            body_elements.append(create_paragraph("Titre", title_text))
        elif stripped.startswith("## "):
            title_text = strip_heading_number(stripped[3:].strip())
            body_elements.append(create_paragraph("Titre1", title_text))
        elif stripped.startswith("### "):
            title_text = strip_heading_number(stripped[4:].strip())
            body_elements.append(create_paragraph("Titre2", title_text))
        elif stripped.startswith("#### "):
            title_text = strip_heading_number(stripped[5:].strip())
            body_elements.append(create_paragraph("Titre3", title_text))
        elif stripped.startswith("- ") or stripped.startswith("* "):
            body_elements.append(create_paragraph("Puce", stripped[2:].strip()))
        elif re.match(r"^\d+\.\s", stripped):
            m = re.match(r"^(\d+)\.\s*(.*)", stripped)
            num = m.group(1)
            rest = m.group(2)
            custom_pPr = '<w:ind w:left="360"/><w:spacing w:before="40" w:after="40"/>'
            body_elements.append(create_paragraph(None, f"**{num}.** {rest}", custom_pPr=custom_pPr))
        else:
            body_elements.append(create_paragraph("Corpsdetexte", stripped))
            
    if in_table:
        body_elements.append(create_table(current_table))
        
    return "".join(body_elements)

def generate_docx():
    print(f"Reading {MD_PATH}...")
    with open(MD_PATH, "r", encoding="utf-8") as f:
        md_text = f.read()

    body_xml = build_body(md_text)

    with open(DOCX_TEMPLATE, "rb") as f:
        template_bytes = f.read()

    with zipfile.ZipFile(io.BytesIO(template_bytes), "r") as zin:
        orig_doc_xml = zin.read("word/document.xml").decode("utf-8")
        header_end = orig_doc_xml.find("<w:body>") + len("<w:body>")
        sect_start = orig_doc_xml.rfind("<w:sectPr")
        doc_header = orig_doc_xml[:header_end]
        doc_footer = orig_doc_xml[sect_start:]
        
        new_document_xml = doc_header + body_xml + doc_footer

        out_buffer = io.BytesIO()
        with zipfile.ZipFile(out_buffer, "w", compression=zipfile.ZIP_DEFLATED) as zout:
            for item in zin.infolist():
                if item.filename == "word/document.xml":
                    zout.writestr(item, new_document_xml.encode("utf-8"))
                elif item.filename == "word/footer1.xml":
                    footer_xml = zin.read(item.filename).decode("utf-8")
                    footer_xml = footer_xml.replace("Aaron Vichery", "Elias Veya")
                    zout.writestr(item, footer_xml.encode("utf-8"))
                else:
                    zout.writestr(item, zin.read(item.filename))

    print(f"Writing {OUTPUT_DOCX}...")
    with open(OUTPUT_DOCX, "wb") as f:
        f.write(out_buffer.getvalue())

    print(f"Success! Generated {OUTPUT_DOCX} ({os.path.getsize(OUTPUT_DOCX)} bytes)")

if __name__ == "__main__":
    generate_docx()
