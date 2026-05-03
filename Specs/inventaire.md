# Domaine Inventaire - Vesta

Agent responsable: Vesta

État: v1 spécifiée

## Pourquoi un agent dédié ?
- Portée large: garde-manger, produits ménagers, appareils électroménagers, outillage, livres, consommables divers.
- Allège Clara (déjà chargée: mails, agenda, tâches) et garde Erina focalisée alimentation/menus.
- Point central des stocks et actifs matériels du foyer, exploitable par les autres agents.

## Rôle et périmètre
- Inventaire des biens (consommables et non-consommables).
- Localisation multi-niveaux: Zone > Pièce > Contenant (placard, bac, étagère) > Emplacement libre.
- Quantités, unités, dates d’expiration (si applicables), seuils d’alerte bas stock.
- Références produit: code-barres (EAN/UPC), marque, modèle, variantes.
- Actifs: numéro de série, facture, garantie, manuel, entretien périodique.
- Média: photo(s) de l’objet, preuve d’achat.

## Interactions avec autres agents
- Erina (Alimentation):
  - v1 lecture seule: utilise les disponibilités et dates (Vesta) pour suggestions et liste de courses.
  - Post‑repas: proposition de décrémentation des ingrédients utilisés (confirmation utilisateur) puis mise à jour Vesta.
  - Alertes DLC (dates limites) → suggestions de recettes.
- Clara (Majordome):
  - Génère tâches: racheter un produit (stock < seuil), maintenance périodique (filtre VMC, détartrage), fin de garantie.
  - Ajoute rendez-vous d’entretien important (chaudière, voiture si inclus plus tard).
- Aurore (Communication):
  - Notifications: bas stock, DL(C) proche, maintenance due; affichage sur écrans rapides/lents.
- Erza (Sécurité/Présence):
  - Privacy mode: masque catégories sensibles (médicaments) sur écrans partagés.
- Sarah (Domotique):
  - Cross-référence: associer un appareil inventorié à un device domotique (consommation, état) quand pertinent.

## Modèle de données (proposé)
- Item
  - id, name, description
  - kind: consumable | asset | book | tool | other
  - categories: [string]
  - barcodes: [string]
  - brand, model, variant
  - quantity: number, unit: string (kg, L, pcs, ml…)
  - minThreshold: number (alerte bas stock)
  - expirationDate?: date (pour consommables)
  - location: { zoneId, roomId, containerId?, freeText? }
  - photos: [url]
  - purchase: { date?, price?, vendor? }
  - warranty: { endsOn?, coverage? }
  - serialNumber?: string (assets)
  - maintenance: [{ kind, frequency, nextDueOn }]
  - tags: [string]
  - privacyLevel: public | household | private
  - audit: { createdOn, updatedOn, updatedBy }

- Container
  - id, name, roomId, type (placard, bac, tiroir, étagère), notes

- Category
  - id, label, parentId?, icon?

## Workflows clés
- Entrée/sortie de stock (consommables): incrément/décrément, historique 7j détaillé.
- Bas stock: déclenche `RaisedMessage` → Clara crée tâche/ajoute à liste de courses.
- Expiration: N-7/N-3/N-1 jours → notification Aurore + proposition Erina.
- Maintenance: planification `Trigger` (Clock + offset), `RaisedMessage` → tâche Clara.
- Import: CSV initial, scan code-barres (mobile/app), lecture ticket de caisse (OCR) plus tard.

## API & événements
- REST via Home.Graph: CRUD items/containers/categories, mouvements de stock, recherche.
- NATS:
  - `vesta.item.changed`
  - `vesta.stock.low`
  - `vesta.item.expiring`
  - `vesta.maintenance.due`

## Intégrations externes
- Lookup code-barres: OpenFoodFacts / Open Product Data (optionnel).
- Stock images/icônes: CDN interne.

## Historique et rétention
- Mouvements de stock: transitions détaillées conservées 7 jours (aligné domotique).
- Événements majeurs (achat, vente, maintenance): conservation longue (configurable).

## MVP (priorités)
Portée v1: couvrent les trois familles principales — consommables (alimentaires/maison), actifs/équipements (électroménager, outils), et livres/dossiers.

1) CRUD Items/Containers/Categories + localisation (Zone > Pièce > Contenant).
2) Quantité + seuil bas + alertes (NATS) → création tâche Clara / ajout liste de courses.
3) Expiration (date unique) + notifications Aurore (J-3, J-1).
4) Import CSV simple (items + quantités + localisation).
5) UI admin de base (Home.Graph.Public) pour gérer items/containers.
6) Support minimal des actifs: numéro de série, preuve d’achat (lien/fichier), fin de garantie, maintenance périodique (rappel via Clara).

## Questions en suspens
- Unités par défaut et conversions (kg ↔ g, L ↔ ml) à gérer automatiquement ?
- Politique de privacy par catégorie (ex: médicaments = household/private par défaut) ?
- Liaison avec tickets d’achat (OCR) à court terme ou plus tard ?

## Configuration (VestaConfigurationData — v1)
- `Defaults`: `{ lowStockThresholdByCategory, defaultUnitsByCategory }`
- `PrivacyPolicies`: `{ categoryId → privacyLevel }`
- `Barcode`: `{ providersEnabled: ["OpenFoodFacts"], synonyms: { "farine T55": "flour.white" } }`
