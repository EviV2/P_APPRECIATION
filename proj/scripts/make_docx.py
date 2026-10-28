import os
import re
import zipfile
import xml.etree.ElementTree as ET

DOCX_TEMPLATE = r"C:\Users\pl01psa\.gemini\antigravity\brain\fbd2feb7-6b97-4949-8de4-88e1955a6271\.user_uploaded\media_1793194019289_8dc1e97b.docx"
MD_PATH = r"c:\Users\pl01psa\Documents\GitHub\P_FUN\proj\Rapport_Projet_P_FUN.md"
OUTPUT_DOCX = r"c:\Users\pl01psa\Documents\GitHub\P_FUN\proj\Rapport_Projet_P_FUN.docx"

W_NS = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"

def escape_xml(text):
    return (text.replace("&", "&amp;")
                .replace("<", "&lt;")
                .replace(">", "&gt;")
                .replace('"', "&quot;"))

def parse_runs(text):
    """Split text by **bold** and `code` markers to create runs."""
    parts = re.split(r"(\*\*.*?\*\*|`.*?`)", text)
    runs_xml = []
    for part in parts:
        if not part:
            continue
        if part.startswith("**") and part.endswith("**") and len(part) >= 4:
            clean = escape_xml(part[2:-2])
            runs_xml.append(f'<w:r><w:rPr><w:b/></w:rPr><w:t xml:space="preserve">{clean}</w:t></w:r>')
        elif part.startswith("`") and part.endswith("`") and len(part) >= 2:
            clean = escape_xml(part[1:-1])
            runs_xml.append(f'<w:r><w:rPr><w:rFonts w:ascii="Consolas" w:hAnsi="Consolas"/><w:color w:val="0F172A"/></w:rPr><w:t xml:space="preserve">{clean}</w:t></w:r>')
        else:
            clean = escape_xml(part)
            runs_xml.append(f'<w:r><w:t xml:space="preserve">{clean}</w:t></w:r>')
    return "".join(runs_xml)

def create_paragraph(style, text):
    runs = parse_runs(text)
    if style:
        return f'<w:p><w:pPr><w:pStyle w:val="{style}"/></w:pPr>{runs}</w:p>'
    else:
        return f'<w:p>{runs}</w:p>'

def create_table(table_rows):
    """table_rows is a list of lists of cell strings."""
    if not table_rows:
        return ""
    xml = ['<w:tbl>',
           '<w:tblPr><w:tblStyle w:val="Grilledutableau"/><w:tblW w:w="0" w:type="auto"/><w:tblLook w:val="04A0"/></w:tblPr>']
    for r_idx, row in enumerate(table_rows):
        xml.append('<w:tr>')
        for c in row:
            runs = parse_runs(c.strip())
            bg = '<w:shd w:val="clear" w:color="auto" w:fill="F1F5F9"/>' if r_idx == 0 else ''
            xml.append(f'<w:tc><w:tcPr>{bg}<w:tcMar><w:top w:w="120" w:type="dxa"/><w:bottom w:w="120" w:type="dxa"/><w:left w:w="160" w:type="dxa"/><w:right w:w="160" w:type="dxa"/></w:tcMar></w:tcPr><w:p>{runs}</w:p></w:tc>')
        xml.append('</w:tr>')
    xml.append('</w:tbl>')
    return "".join(xml)

def build_body(md_content):
    lines = md_content.splitlines()
    body_elements = []
    
    in_table = False
    current_table = []
    
    for line in lines:
        stripped = line.strip()
        
        # Check if table row
        if stripped.startswith("|") and stripped.endswith("|"):
            parts = [p.strip() for p in stripped.split("|")[1:-1]]
            # check if separator row like |:---|:---|
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
        
        if not stripped or stripped == "---":
            continue
            
        # Code block fences
        if stripped.startswith("```"):
            continue
            
        # Images or blockquotes
        if stripped.startswith("!["):
            m = re.match(r"!\[(.*?)\]\((.*?)\)", stripped)
            desc = m.group(1) if m else "Illustration"
            body_elements.append(create_paragraph("Italique", f"[Image : {desc}]"))
            continue
            
        if stripped.startswith(">"):
            clean = re.sub(r"^>\s*(\[!.*?\])?\s*", "", stripped)
            body_elements.append(create_paragraph("Italique", clean))
            continue
            
        # Headings
        if stripped.startswith("# "):
            body_elements.append(create_paragraph("Titre", stripped[2:]))
        elif stripped.startswith("## "):
            body_elements.append(create_paragraph("Titre1", stripped[3:]))
        elif stripped.startswith("### "):
            body_elements.append(create_paragraph("Titre2", stripped[4:]))
        elif stripped.startswith("#### "):
            body_elements.append(create_paragraph("Titre3", stripped[5:]))
        elif stripped.startswith("- "):
            body_elements.append(create_paragraph("Puce", stripped[2:]))
        elif re.match(r"^\d+\.\s", stripped):
            clean = re.sub(r"^\d+\.\s*", "", stripped)
            body_elements.append(create_paragraph("Puce", clean))
        else:
            body_elements.append(create_paragraph("Corpsdetexte", stripped))
            
    if in_table:
        body_elements.append(create_table(current_table))
        
    return "".join(body_elements)

def generate_docx():
    print("Reading markdown...")
    with open(MD_PATH, "r", encoding="utf-8") as f:
        md_text = f.read()

    body_xml = build_body(md_text)

    # Read base document.xml from template to get the exact header & sectPr
    with zipfile.ZipFile(DOCX_TEMPLATE, "r") as zin:
        orig_doc_xml = zin.read("word/document.xml").decode("utf-8")
        header_end = orig_doc_xml.find("<w:body>") + len("<w:body>")
        sect_start = orig_doc_xml.rfind("<w:sectPr")
        doc_header = orig_doc_xml[:header_end]
        doc_footer = orig_doc_xml[sect_start:]
        
        new_document_xml = doc_header + body_xml + doc_footer

        # Write new docx
        print(f"Creating {OUTPUT_DOCX}...")
        with zipfile.ZipFile(OUTPUT_DOCX, "w") as zout:
            for item in zin.infolist():
                if item.filename == "word/document.xml":
                    zout.writestr(item, new_document_xml.encode("utf-8"))
                elif item.filename == "word/footer1.xml":
                    # Clean footer text
                    footer_xml = zin.read(item.filename).decode("utf-8")
                    footer_xml = footer_xml.replace("Aaron Vichery", "Elias Veya")
                    zout.writestr(item, footer_xml.encode("utf-8"))
                else:
                    zout.writestr(item, zin.read(item.filename))
                    
    print(f"Done! Created {OUTPUT_DOCX} (size: {os.path.getsize(OUTPUT_DOCX)} bytes)")

if __name__ == "__main__":
    generate_docx()
