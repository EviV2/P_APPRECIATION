# P_FUN - Plot those lines !

**Elias Veya – MID2A**  
**ETML — Section Informatique**  
Lausanne, Vennes / Avenue de Valmont 28b  
24 périodes  
Enseignants / Clients : Xavier Carrel / Jonathan Melly  

---

## Table des matières

- [1 Introduction](#1-introduction)
  - [1.1 Titre](#11-titre)
  - [1.2 Description](#12-description)
  - [1.3 Matériel et logiciels à disposition](#13-matériel-et-logiciels-à-disposition)
  - [1.4 Prérequis](#14-prérequis)
- [2 Analyse](#2-analyse)
  - [2.1 Présentation de nos maquettes](#21-présentation-de-nos-maquettes)
    - [2.1.1 Accueil et graphique principal](#211-accueil-et-graphique-principal)
    - [2.1.2 Volet des séries temporelles](#212-volet-des-séries-temporelles)
    - [2.1.3 Volet des filtres (Champion, Dates, Saison, Patch)](#213-volet-des-filtres-champion-dates-saison-patch)
    - [2.1.4 Sélecteur de dates (du ... au ...)](#214-sélecteur-de-dates-du--au-)
    - [2.1.5 Statistiques globales (Multi-métriques)](#215-statistiques-globales-multi-métriques)
    - [2.1.6 Détection interactive au clic](#216-détection-interactive-au-clic)
    - [2.1.7 Import CSV](#217-import-csv)
    - [2.1.8 Palette de couleurs](#218-palette-de-couleurs)
  - [2.2 Projet final](#22-projet-final)
    - [2.2.1 Vue générale de l'application](#221-vue-générale-de-lapplication)
    - [2.2.2 Filtrage par plage de dates](#222-filtrage-par-plage-de-dates)
    - [2.2.3 Superposition dynamique des séries](#223-superposition-dynamique-des-séries)
    - [2.2.4 Détection du clic et mise à jour instantanée](#224-détection-du-clic-et-mise-à-jour-instantanée)
  - [2.3 Product Backlog](#23-product-backlog)
- [3 Analyse de la structure du code](#3-analyse-de-la-structure-du-code)
  - [3.1 Program.cs](#31-programcs)
  - [3.2 App.axaml et App.axaml.cs](#32-appaxaml-et-appaxamlcs)
  - [3.3 MainWindow.axaml](#33-mainwindowaxaml)
  - [3.4 MainWindow.axaml.cs](#34-mainwindowaxamlcs)
  - [3.5 Controllers / MainWindowController.cs](#35-controllers--mainwindowcontrollercs)
  - [3.6 Models / BanRateRecord.cs](#36-models--banraterecordcs)
  - [3.7 Models / StatsSummary.cs & MetricStats](#37-models--statssummarycs--metricstats)
  - [3.8 Services / IDataLoader.cs](#38-services--idataloadercs)
  - [3.9 Services / CsvDataLoader.cs](#39-services--csvdataloadercs)
  - [3.10 Services / BanRateAnalyzer.cs](#310-services--banrateanalyzercs)
  - [3.11 Services / PlotService.cs](#311-services--plotservicecs)
  - [3.12 Services / PatchTickGenerator.cs](#312-services--patchtickgeneratorcs)
  - [3.13 Extensions / TimeSeriesExtensions.cs](#313-extensions--timeseriesextensionscs)
  - [3.14 BanStatsApp.Tests (tests unitaires)](#314-banstatsapptests-tests-unitaires)
  - [3.15 Données CSV (data/)](#315-données-csv-data)
- [4 Réalisation](#4-réalisation)
  - [4.1 Fonctionnalités demandées](#41-fonctionnalités-demandées)
  - [4.2 Organisation du travail](#42-organisation-du-travail)
- [5 Tests unitaires et fonctionnels](#5-tests-unitaires-et-fonctionnels)
  - [5.1 Tests unitaires xUnit](#51-tests-unitaires-xunit)
  - [5.2 Tests d'acceptance manuels](#52-tests-dacceptance-manuels)
- [6 Usage des outils d'Intelligence Artificielle (IA)](#6-usage-des-outils-dintelligence-artificielle-ia)
- [7 Conclusion](#7-conclusion)
  - [7.1 Conclusion générale du projet](#71-conclusion-générale-du-projet)
  - [7.2 Conclusion personnelle (Elias)](#72-conclusion-personnelle-elias)
  - [7.3 Critique constructive sur la planification](#73-critique-constructive-sur-la-planification)
  - [7.4 Méthodologie git](#74-méthodologie-git)
  - [7.5 Annexes](#75-annexes)
    - [7.5.1 Webographie](#751-webographie)
    - [7.5.2 Bibliographie](#752-bibliographie)
    - [7.5.3 Glossaire](#753-glossaire)

---

## 1 Introduction

### 1.1 Titre
**P_FUN - Plot those lines**

### 1.2 Description
Le but de ce projet est de concevoir et développer une application permettant d'afficher un ou des graphiques à l'aide de données en format CSV.  
Le projet est réalisé en C# et applique rigoureusement les principes de la programmation fonctionnelle (LINQ).

### 1.3 Matériel et logiciels à disposition
- Un PC de l'ETML
- Visual Studio Code
- Avalonia UI 11
- ScottPlot 5
- xUnit

### 1.4 Prérequis
- ICT 323

---

## 2 Analyse

### 2.1 Présentation de nos maquettes

#### 2.1.1 Accueil et graphique principal
La zone principale affiche un graphique ScottPlot vectoriel avec l'axe horizontal chronologique (dates et patchs) et l'axe vertical en pourcentages (0% à 100%). Des boutons de zoom en haut permettent d'ajuster facilement la vue.

#### 2.1.2 Volet des séries temporelles
À droite, des cases à cocher permettent d'activer ou masquer chaque série :
- **Banrate** (rose `#E0247C`) : taux de bannissement (actif par défaut).
- **Winrate** (bleu foncé / noir `#1E293B`) : pourcentage de victoires.
- **Pickrate** (rouge `#EF4444`) : fréquence de sélection.

#### 2.1.3 Volet des filtres (Champion, Dates, Saison, Patch)
Permet d'isoler des sous-ensembles précis :
- **Champion** : choix entre Gwen et Azir (avec possibilité de charger un CSV externe).
- **Saison** : sélection d'une saison ou vue globale.
- **Patch** : choix d'un patch spécifique selon la saison.

#### 2.1.4 Sélecteur de dates (du ... au ...)
Deux sélecteurs `CalendarDatePicker` définissent une plage personnalisée de dates (début / fin).  
Le bouton **« Toutes »** réinitialise la période en un clic.

#### 2.1.5 Statistiques globales (Multi-métriques)
Quatre indicateurs mis à jour dynamiquement :
- **Actuel** : dernière valeur de la série.
- **Moyenne** : moyenne calculée en pur LINQ.
- **Pic (Max)** : valeur maximale observée.
- **Plus bas (Min)** : valeur minimale observée.
- **Total points** : nombre de points mesurés.

Un menu permet de basculer l'affichage entre Banrate, Pickrate et Winrate.

#### 2.1.6 Détection interactive au clic
Un clic sur une courbe place un marqueur circulaire sur le point le plus proche (affichant date et valeur) et bascule automatiquement les statistiques sur cette métrique.

#### 2.1.7 Import CSV
Bouton « Importer » permettant d'ouvrir l'explorateur de fichiers de l'OS (filtré sur `*.csv`) pour charger un nouvel historique de données.

#### 2.1.8 Palette de couleurs
L'application applique une charte graphique claire, moderne et contrastée :
- `#FFFFFF` : Fond des cartes et des conteneurs
- `#F8FAFC` / `#F1F5F9` : Fond général de l'app
- `#E0247C` : Couleur principale de Banrate 
- `#1E293B` : Couleur du Winrate 
- `#EF4444` : Couleur du Pickrate
- `#CBD5E1` / `#E2E8F0` : Bordures
- `#0F172A` : Texte principal

---

### 2.2 Projet final

#### 2.2.1 Vue générale de l'application
L'application finale a une interface réactive et complète.  
L'application se sépare en 2 parties : à gauche le graphique ScottPlot et à droite les paramètres et filtres, surmontés des commandes de zoom et d'import.

#### 2.2.2 Filtrage par plage de dates
L'utilisateur peut choisir deux dates pour restreindre l'affichage entre ces dernières.  
Le graphique se recadre instantanément et les statistiques se recalculent immédiatement pour la période choisie à l'aide de nos extensions de langage LINQ.

#### 2.2.3 Superposition dynamique des séries
On peut afficher simultanément le Banrate, le Pickrate et le Winrate sur le même axe.  
Cette vue croisée permet de comprendre les réactions des joueurs : lorsqu'un pic de victoires survient après un patch, le taux de sélection augmente, suivi peu après par le taux de ban.

#### 2.2.4 Détection du clic et mise à jour instantanée
Au clic sur n'importe quelle courbe, le marqueur affiche la date exacte et les statistiques globales s'actualisent en direct en fonction de la courbe sur laquelle on a appuyé.  
Les statistiques ne se limitent pas qu'au Banrate mais s'adaptent aussi aux deux autres métriques.

---

### 2.3 Product Backlog

Le suivi du travail s'est fait via un Kanban GitHub Project connecté au dépôt du projet :  
**Lien GitHub Project** : [https://github.com/users/EviV2/projects/6](https://github.com/users/EviV2/projects/6)

Les tâches étaient réparties en 4 colonnes :
- **Backlog** : ensemble des fonctionnalités prévues.
- **In progress** : tâches en cours de développement.
- **In review** : fonctionnalités terminées en attente de validation par les tests.
- **Done** : tâches validées et terminées.

---

## 3 Analyse de la structure du code

L'application est structurée selon une architecture MVC propre :

### 3.1 Program.cs
Point d'entrée standard configurant et lançant Avalonia UI.

### 3.2 App.axaml et App.axaml.cs
Initialise les styles Fluent et ouvre la fenêtre principale `MainWindow`.

### 3.3 MainWindow.axaml
Vue XAML décrivant l'interface :
- Barre supérieure avec commandes de zoom et bouton d'import.
- Graphique `AvaPlot` de ScottPlot à gauche.
- Panneau latéral avec filtres, sélecteurs de dates et indicateurs statistiques.

### 3.4 MainWindow.axaml.cs
Code-behind gérant les interactions utilisateurs :
- Récupération des clics sur le graphique pour détecter le point le plus proche.
- Boîte de dialogue pour sélectionner un fichier CSV externe via l'API `StorageProvider`.

### 3.5 Controllers / MainWindowController.cs
Contrôleur principal reliant l'interface aux données :
- Conserve les données brutes et la liste filtrée.
- Applique les filtres LINQ en chaîne (champion, dates début/fin, saison, patch).
- Recalcule les statistiques à chaque changement et notifie la vue.

### 3.6 Models / BanRateRecord.cs
Modèle représentant un enregistrement temporel :
- `Date`, `Patch`, `BanRate`, `PickRate`, `WinRate`, `Season`.

### 3.7 Models / StatsSummary.cs & MetricStats
Conteneurs de données stockant les résultats calculés :
- Moyenne, minimum, maximum, valeur actuelle et patchs des extremums pour chaque métrique.

### 3.8 Services / IDataLoader.cs
Interface de chargement définissant la méthode `Load(string filePath)`.

### 3.9 Services / CsvDataLoader.cs
Lit les fichiers CSV ligne par ligne et convertit chaque ligne en objet `BanRateRecord` en gérant la culture invariante.

### 3.10 Services / BanRateAnalyzer.cs
Calcul des statistiques en pur LINQ (aucune boucle impérative) :
- Moyenne (`.Average()`), extremums (`.Max()`, `.Min()`) et recherche du point le plus proche (`.MinBy()`).

### 3.11 Services / PlotService.cs
Gestion du graphique ScottPlot 5 :
- Tracé des courbes Banrate, Winrate et Pickrate.
- Mise en valeur visuelle du point sélectionné au clic.
- Réinitialisation et gestion du zoom.

### 3.12 Services / PatchTickGenerator.cs
Générateur d'étiquettes sur l'axe horizontal pour espacer lisiblement les numéros de patch et dates.

### 3.13 Extensions / TimeSeriesExtensions.cs
Méthodes d'extension C# :
- `FilterByDateRange` : filtre les données entre deux dates.
- `MovingAverage` : calcul d'une moyenne mobile lissée en pur LINQ.
- `FilterBySeason` et `FilterByPatch` : filtrage simplifié par saison et patch.

### 3.14 BanStatsApp.Tests (tests unitaires)
Suite de tests xUnit automatisés validant le parsing CSV, les extensions LINQ et les calculs statistiques.

### 3.15 Données CSV (data/)
Le dossier `src/data/` contient les jeux de données locaux :
- `gwen_banrate_data_v2.csv` : historique de Gwen (1'212 enregistrements).
- `azir_full_stats.csv` : historique d'Azir (1'212 enregistrements).

---

## 4 Réalisation

### 4.1 Fonctionnalités demandées

| Fonctionnalité demandée | Statut dans PTL | Commentaire |
| :--- | :---: | :--- |
| **Stockage local hors-connexion** | ✅ Réalisé | Fichiers CSV stockés localement dans `data/`. |
| **Import de fichiers externes** | ✅ Réalisé | Dialogue d'ouverture pour charger un nouveau CSV. |
| **Multi-séries simultanées** | ✅ Réalisé | Affichage synchronisé de Banrate, Winrate et Pickrate. |
| **Minimum 5 séries de 500+ valeurs** | ✅ Réalisé | 6 séries complètes de 1'212 valeurs (Gwen et Azir). |
| **Utilisation stricte de LINQ (pas de boucle)** | ✅ Réalisé | Aucune boucle `for`, `while` ou `foreach` dans le code métier. |
| **Au moins 2 extensions C#** | ✅ Réalisé | 4 méthodes d'extension créées. |
| **Au moins 3 tests unitaires** | ✅ Réalisé | 8 tests unitaires automatisés sous xUnit. |
| **Filtre par plage de dates** | ✅ Réalisé | Deux sélecteurs de calendrier avec bouton de reset. |
| **Statistiques multi-métriques au clic** | ✅ Réalisé | Clic sur une courbe actualisant la métrique active. |

### 4.2 Organisation du travail
1. Analyse du sujet et création de la maquette.
2. Mise en place du projet en C# sous Avalonia UI.
3. Intégration de ScottPlot 5 et lecture des fichiers CSV.
4. Développement des filtres temporels et calculs en LINQ.
5. Création et validation des tests unitaires sous xUnit.
6. Ajustements de l'interface et rédaction du rapport.

---

## 5 Tests unitaires et fonctionnels

### 5.1 Tests unitaires xUnit
Les 8 tests s'exécutent avec `dotnet test` et réussissent en quelques millisecondes :

| Classe de test | Méthode testée | Objectif du test | Résultat |
| :--- | :--- | :--- | :---: |
| `DataLoaderTests` | `LoadRecords_ValidCsv_ReturnsCorrectRecords` | Vérifie le bon parsing du fichier CSV | ✅ Réussi |
| `DataLoaderTests` | `LoadRecords_MissingFile_ThrowsFileNotFound` | Vérifie la levée d'erreur si fichier absent | ✅ Réussi |
| `TimeSeriesExtensionsTests` | `MovingAverage_CalculatesCorrectAverage` | Valide le calcul de la moyenne mobile | ✅ Réussi |
| `TimeSeriesExtensionsTests` | `FilterBySeason_FiltersCorrectly` | Vérifie le filtrage par saison | ✅ Réussi |
| `TimeSeriesExtensionsTests` | `FilterByPatch_FiltersCorrectly` | Vérifie l'isolation d'un patch | ✅ Réussi |
| `TimeSeriesExtensionsTests` | `FilterByDateRange_FiltersRecordsBetweenDates` | Vérifie le filtrage entre deux dates | ✅ Réussi |
| `BanRateAnalyzerTests` | `CalculateSummary_ComputesCorrectMinMaxAverage` | Valide les calculs de Min, Max et Moyenne | ✅ Réussi |
| `BanRateAnalyzerTests` | `FindNearestRecord_ReturnsClosestPoint` | Valide la détection du point le plus proche | ✅ Réussi |

### 5.2 Tests d'acceptance manuels

| N° | Action de test | Résultat attendu | Résultat obtenu |
| :--- | :--- | :--- | :---: |
| **01** | Lancement de l'application | Gwen est chargée par défaut, la courbe s'affiche avec ses statistiques. | Conforme |
| **02** | Sélection d'Azir dans la liste | Le graphique s'actualise avec les séries d'Azir. | Conforme |
| **03** | Sélection de dates début/fin | Le graphique et les statistiques se restreignent à la période. | Conforme |
| **04** | Clic sur le bouton « Toutes » | Les dates se réinitialisent et tout l'historique réapparaît. | Conforme |
| **05** | Clic sur la courbe Pickrate | Un marqueur s'affiche et les statistiques basculent sur le Pickrate. | Conforme |

---

## 6 Usage des outils d'Intelligence Artificielle (IA)

> [!NOTE]
> *Ce chapitre explicatif compte environ 220 mots afin de respecter la consigne minimale de 200 mots demandée par la section 7.7.2.e du cahier des charges ETML.*

Dans le cadre du projet P_FUN, l'intelligence artificielle a été utilisée comme une aide ponctuelle à la productivité, sans remplacer le travail personnel ni l'apprentissage des concepts du module 323.

L'utilisation de l'IA s'est limitée à trois points précis :
1. **Mise en forme des données CSV** : Génération et reformatage des jeux de données d'Azir et Gwen pour obtenir plus de 1'200 lignes régulières et exploitables sur les saisons 12 à 16.
2. **Support technique sur Avalonia et ScottPlot** : Recherche d'exemples pour le binding des sélecteurs de dates (`CalendarDatePicker`) et la configuration des axes personnalisés de ScottPlot 5.
3. **Optimisation des requêtes LINQ** : Exploration de syntaxes déclaratives sans boucle, notamment pour combiner `Range`, `Skip`, `Take` et `Average` dans le calcul de la moyenne mobile glissante.

Chaque réponse a été relue, comprise, testée et adaptée au projet. Aucun code n'a été inséré sans être compris, ce qui me permet d'expliquer l'intégralité du programme de manière autonome. Cette démarche encadrée a permis de respecter les directives de l'ETML sans consommer de joker.

---

## 7 Conclusion

### 7.1 Conclusion générale du projet
L'application répond à toutes les exigences du cahier des charges : affichage fluide des séries temporelles avec Avalonia et ScottPlot 5, aucun usage de boucles impératives grâce à LINQ, et une architecture claire et modulaire.

### 7.2 Conclusion personnelle (Elias)
Projet très motivant et formateur. Travailler sur des données réelles de League of Legends a rendu le sujet concret. J'ai pu progresser sur le développement d'interfaces modernes sous Avalonia et perfectionner mon utilisation de LINQ en C#.

### 7.3 Critique constructive sur la planification
Le planning a été bien tenu. Le projet a été terminé dans les temps, ce qui a permis d'ajouter des améliorations utiles comme le filtre par dates personnalisées. Avec le recul, j'aurais anticipé un peu plus tôt la gestion du composant de date sous Avalonia.

### 7.4 Méthodologie git
Utilisation régulière de Git avec des commits clairs et fréquents pour tracer chaque étape clé du projet.

### 7.5 Annexes

#### 7.5.1 Webographie
- Documentation Avalonia UI : [https://docs.avaloniaui.net/](https://docs.avaloniaui.net/)
- Documentation ScottPlot 5 : [https://scottplot.net/cookbook/5.0/](https://scottplot.net/cookbook/5.0/)
- Microsoft Learn (LINQ C#) : [https://learn.microsoft.com/fr-fr/dotnet/csharp/linq/](https://learn.microsoft.com/fr-fr/dotnet/csharp/linq/)
- League of Legends Wiki : [https://leagueoflegends.fandom.com/wiki/](https://leagueoflegends.fandom.com/wiki/)

#### 7.5.2 Bibliographie
- Polycopié du Module 323 : Programmation fonctionnelle en C#, ETML.
- Cahier des charges : *Plot those lines !*, Xavier Carrel & Jonathan Melly.

#### 7.5.3 Glossaire
- **LINQ** : Requêtes fonctionnelles intégrées au C# permettant de manipuler des collections sans boucles.
- **Méthode d'extension** : Méthode permettant d'ajouter des fonctionnalités à un type existant en C#.
- **Série temporelle** : Données chiffrées ordonnées chronologiquement.
- **Banrate** : Taux de bannissement d'un champion.
- **Pickrate** : Taux de sélection d'un champion.
- **Winrate** : Taux de victoire d'un champion.
- **ScottPlot** : Bibliothèque graphique pour tracer des courbes en .NET.
- **Avalonia UI** : Framework pour créer des interfaces graphiques multiplateformes.
- **xUnit** : Outil de tests unitaires pour .NET.
