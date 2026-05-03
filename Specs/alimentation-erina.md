# Domaine Alimentation - Erina

Agent responsable: Erina

## Rôle

Erina planifie les menus, gère une bibliothèque de recettes, propose des suggestions en fonction des disponibilités réelles (Vesta) et génère la liste de courses. Elle s’intègre avec Clara (agenda/tâches) et Aurore (notifications), sans décrémentation automatique des stocks: on commence par « voir ce qu’on a » et décider.

## Menus & Planification

- Questionnaires de planification (v1):
  - Périmètre: horizon (ex: 7 jours), repas par jour (déj/dîner), convives, invités ponctuels.
  - Contraintes: régimes/allergènes, appareils disponibles (four, robot, BBQ…), temps max par repas, budget approximatif, préférences (cuisines, familles de plats), option « restes ».
  - Rotation (anti-répétition): fenêtres minimales avant répétition d’une recette/tag/cuisine (ex: 14 jours recette, 7 jours tag « pâte », 5 jours cuisine « asiatique »).
  - Disponibilités: booster les recettes utilisant des ingrédients en stock, prioriser ceux proches de péremption (Vesta), possibilité d’exclure catégories/ingrédients.
- Moteur de suggestion (score):
  - + Disponibilité Vesta (complet/partiel), + Péremption proche, + Saison/tags souhaités, + Note utilisateur; – Récence (pénalité rotation), – Allergènes/interdits.
  - Fallback: si indispo → alternatives proches (substituts, variantes), sinon proposition « neutre ».
- Sortie d’un plan: menu structuré par jour/repas avec recettes (plat principal, accompagnement facultatif, dessert facultatif), variantes enfants/ invités si besoin.
  - Export Clara: création d’évènements `TodoItem` (type `EventItem`) pour créneaux cuisine/repas; ajouts de tâches « préparation » si utile.

## Recettes

- Modèle recette:
  - Métadonnées: `id`, `title`, `description`, `durationPrep`, `durationCook`, `difficulty`, `servings`, `tags` (cuisine, saison, catégorie), `appliances`, `sourceUrl`.
  - Ingrédients: `ingredientRef` (lien vers Vesta — catégorie/produit/synonyme), `quantity`+`unit`, `substitutes` (facultatifs), `notes`.
  - Étapes: liste ordonnée, minuteur(s) possibles, conseils.
  - Nutrition (optionnel v2): calories, macros, allergènes.
- Import: manuel (librairie locale), plus tard importeurs (URL blog, OCR, etc.).

## Liste de courses

- Génération: différence entre besoins du plan et disponibilités Vesta; conversion d’unités; arrondi (ex: 185g → 200g).
- Regroupement: par catégorie/rayon (plus tard par enseigne). Export en tâches (Clara, liste courses mesh) et/ou fichier.
- Politique stock: ne pas décrémenter automatiquement. Après le repas, proposer « marquer comme utilisé » pour mettre à jour Vesta.

## Intégration Vesta (Inventaire)

- Lecture seule v1: interrogation de Vesta pour disponibilités, dates de péremption, seuils.
- Suggestions « à consommer vite »: prioriser les items proches péremption dans les menus; remonter une alerte « use-it-now ».
- Post‑cuisine: workflow de confirmation pour décrémentation (liste d’ingrédients utilisés → choix utilisateur → écriture Vesta).

## Intégrations

- Clara: création d’évènements (repas/cuisine) et de tâches (courses). Rappels de préparation si temps long.
- Aurore: notifications pour lancer la planification (questionnaire), récap hebdo de menus, alertes « à consommer vite ».
- Sarah (optionnel): scénarios cuisine (éclairage chaud, hotte) déclenchés pendant le créneau.

## Confidentialité

- Menus par défaut « Partagé avec utilisateurs »; détails recette/ingrédients masqués en mode privé du mesh dans les surfaces publiques.

## Configuration (ErinaConfigurationData — v1)

- `PlanningDefaults`: `{ horizonDays, mealsPerDay, rotationWindows: {recipeDays, tagDays, cuisineDays}, preferLeftovers }`
- `Preferences`: `{ allergensExcluded, dislikedIngredients, favoredCuisines, budgetLevel, maxPerMealMinutes }`
- `RecipeSources`: `{ localLibraryPath(s), allowedDomainsForImport }`
- `VestaMapping`: `{ ingredientSynonyms → vestaCategory/product }`

## MVP

- Assistant de planification (questionnaire) + suggestions basées sur Vesta (disponibilités + péremption) + export en évènements Clara + liste de courses (tâches mesh).
- Pas de décrémentation auto; confirmation post‑repas pour MAJ Vesta.
- Librairie de recettes locale minimaliste (JSON) avec tagging basique.

## NATS (proposés)

- `erina.menu.plan.request` → { `horizonDays`, `mealsPerDay`, `constraints`, `preferences` }
- `erina.menu.plan.result` → { `plan`, `shoppingList`, `scoreBreakdown` }
- `erina.notifications.useitnow` → { `vestaItemId`, `label`, `bestBefore`, `suggestedRecipes[]` }
- `erina.vesta.refresh.request` / `erina.vesta.refresh.done`

## Ouvertures v2

- Intégration Drive (courses en ligne), import recettes web, scoring nutritif, budgets multi‑enseignes, planification multi‑ménages.