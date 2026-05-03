# Use Cases Manoir

## Structure

Ce document catalogue les use cases par domaine et les scénarios transversaux impliquant plusieurs agents.

---

## Use Cases par Domaine

### Sarah (Domotique)

#### UC-Sarah-1: Scène de réveil matinal
- **Acteurs**: Utilisateur, Sarah (scènes), Clara (wakeup time), Aurore (greetings)
- **Déclencheur**: Trigger Clock à `FromEarliestWakeup` calculé par Clara (ex: train à 06:00 → wakeup 05:30; sinon heure par défaut, weekend/jours fériés ajustés)
- **Flux**:
  1. Clara calcule `wakeupTime` (agenda, trajets, congés/vacances) et publie l’événement; Sarah arme un lock `correlationId` pour éviter double déclenchement.
  2. Sarah exécute scène "Réveil" avec conditions par étape:
     - Ouvre volets chambres.
     - Si `now > sunrise` (Erza), ouvre volets RDC.
     - Si au moins un user présent (Erza), met thermostat à 20°C.
     - Rampe lumière: brightness 10→100 en ~5 min.
     - Ignore devices indispo (log warning, pas de blocage).
  3. Aurore (action de scène) envoie greeting mobile personnalisé (météo, agenda jour, menus si dispo) + affiche écran rapide heure/météo/tâches.
- **Intégrations**: Clara (wakeup time), Erza (soleil + présence exposés dans Home.Graph: `nextSunrise`, `hasSunRisen`), Erina (menus jour si config), Aurore (greetings).
- **Dépendances**: Délais/étapes scènes (✅ spécifiée), conditions de scène (sunrise/presence via propriétés Home.Graph) applicables.
- **Priorité**: MVP.

#### UC-Sarah-2: Ajustement température basé sur présence
- **Acteurs**: Utilisateur (présence), Sarah (devices thermiques), Erza (détection présence).
- **Déclencheur**: Topic Erza `erza.presence.changed` ou trigger `NetworkDeviceConnectionChanged`.
- **Flux**:
  1. Erza publie `erza.presence.changed` { userId, status: present|away, room?, confidence, timestamp } et expose dans Home.Graph les propriétés maison (`presence.homeStatus`, `presence.presentUsers`).
  2. Sarah consomme la propriété ou l’événement, arme un lock anti-doublon et applique logique chauffage par zone:
     - Si `presence.homeStatus = away` (tous absents, confiance suffisante), passe en éco (ex: 16°C) pour toutes les zones chauffées.
     - Si au moins un user présent, remonte en confort (ex: 20°C) dans les zones occupées ou par défaut la maison.
     - Hystérésis/tempo: ne repasse pas à confort/éco plus d’une fois toutes les X minutes pour éviter le flapping; ignore signaux de faible confiance.
     - Devices offline: ignore étape si thermostat/zone indispo, log warning, pas de blocage.
  3. Aurore (option) notifie en NORMAL: "Chauffage réduit, maison vide" ou "Confort activé, quelqu’un est là".
- **Intégrations**: Erza (présence + propriétés Home.Graph), Aurore (notif optionnelle).
- **Dépendances**: Détection présence Erza (✅ spécifiée), triggers Sarah (✅ spécifiés), propriétés présence dans Home.Graph disponibles.
- **Priorité**: MVP.

#### UC-Sarah-3: Détection anomalie device (offline critique)
- **Acteurs**: Sarah (monitoring), Aurore (alerts).
- **Déclencheur**: Monitoring périodique Sarah, seuils par protocole/rôle avec hysteresis (ex: 2 sondes consécutives avant OFFLINE).
- **Flux**:
  1. Sarah publie `sarah.device.status.changed` { deviceId, availability: online|warning|offline, battery?, lqi?, role, protocol, timestamp } et met à jour l’état dans Home.Graph.
  2. Règles d’alerte (important only):
     - Bridge/hub/power device offline → priorité CRITICAL.
     - Sensor/actionneur essentiel offline → priorité HIGH.
     - Batterie faible ou LQI faible → pas d’alerte Aurore; seulement `availability=warning` dans Home.Graph (consultable en UI), log.
  3. Aurore notifie selon priorité (CRITICAL/HIGH) avec escalade si pas ack (App → Voix → SMS). Rien pour WARNING batterie/LQI.
  4. Anti-flapping: backoff entre changements d’état; ne repasse pas online/offline trop fréquemment (fenêtre temporelle).
- **Intégrations**: Aurore (notifications + escalade), Home.Graph (états devices, incluant battery/LQI en warning sans alerte).
- **Dépendances**: Monitoring Sarah (✅ spécifiée), priorités Aurore (✅ spécifiées).
- **Priorité**: MVP.

#### UC-Sarah-4: Appairage/Intégration nouveau device
- **Acteurs**: Admin, Sarah (découverte), Hub/Protocol (ex: Hue Bridge, Zigbee2MQTT, Shelly).
- **Déclencheur**: Mode appairage activé sur hub (ex: bouton Hue Bridge, mode permit-join Z2M) ou découverte automatique périodique.
- **Flux**:
  1. Hub/protocol détecte nouveau device (ex: Hue lamp appairée, Shelly détecte MQTT announce) → Sarah le découvre via API hub/MQTT et publie `sarah.device.discovered` { deviceId, protocol, capabilities, suggestedName, ... }.
  2. **Auto-configuration technique** (si device supporte `IAutoSetupDevice`, ex: Shelly):
     - Sarah détecte device en statut "NewDevice" (pas d'authentification ni MQTT configurés).
     - Configure automatiquement:
       - Authentication: username `sarah` + password sécurisé.
       - MQTT: active + pointage vers broker local.
       - Reboot device.
     - Log du succès ou erreur.
  3. Admin ouvre UI discovery dans Home.Graph.Public:
     - Voit liste devices découverts (auto-configurés pour Shelly, en attente pour autres protocols).
     - Valide device, renomme si besoin (ex: "Lampe Bureau" au lieu de "Hue Lamp 3").
     - Associe à location (Zone > Room), confirme type/rôle (light, sensor, switch, etc.).
     - Configure options spécifiques si applicable (ex: transition time, polling interval, groupes).
  4. Sarah enregistre dans Home.Graph, active immédiatement dans monitoring, rend disponible pour scènes/triggers.
  5. Publie `sarah.device.added` { deviceId, name, location, type, capabilities, timestamp }.
  6. Aurore (optionnel) notifie en LOW: "Nouveau device ajouté: Lampe Bureau (Chambre)".
- **Intégrations**: Home.Graph (UI admin + storage), protocols (Hue, Zigbee, Shelly, WLED, SmartThings, Divoom), Aurore (notif optionnelle).
- **Dépendances**: CRUD devices via Home.Graph API (à spécifier), UI discovery (à créer), auto-config devices (✅ implémentée pour Shelly).
- **Priorité**: MVP.

#### UC-Sarah-5: Scène multi-pièce avec parallélisme
- **Acteurs**: Admin, Sarah (scènes), devices.
- **Déclencheur**: Commande manuelle "Mode Ciné" ou trigger.
- **Flux**:
  1. Scène "Mode Ciné": groupes parallèles (parallelGroupId).
     - Groupe 1: baisse stores, noir → attendre 1s.
     - Groupe 2: atténue lampes (100→5% brightness sur 2s), lampe TV en couleur ambiante.
     - Groupe 3: active home theater si Samsung TV présente.
  2. Exécution concurrente des groupes, séquentielle dans chaque groupe.
  3. Log des étapes, durée totale ~3s.
- **Intégrations**: Home.Graph (UI scènes), logs (7 jours).
- **Dépendances**: Moteur d'exécution scènes avec delays/parallélisme (✅ spécifiée).
- **Priorité**: MVP.

---

### Aurore (Communication)

#### UC-Aurore-1: Notification prioritaire avec escalade
- **Acteurs**: Aurore, utilisateur, canaux (App/Voix/SMS).
- **Déclencheur**: Alerte sécurité (Erza, Sarah) ou système.
- **Flux**:
  1. Source publie `aurore.notify.request` { type: "security.alarm", priority: CRITICAL, category: "security", ... }.
  2. Aurore évalue:
     - Dédup: même `correlationId` en 2 min? Non.
     - Quiet hours mesh: 23:00–06:00? Oui, mais CRITICAL passe.
     - User DND? Ignore si CRITICAL.
     - Choix canal: App + Écran rapide + Voix (ordre CRITICAL).
  3. Envoie App push immédiatement.
  4. Si pas ack en 60s → escalade Voix (TTS "Alerte sécurité, pressez ok").
  5. Si pas ack en 120s → SMS (si config).
  6. Log: { id, type, priority, channels_sent: [app, voice, sms], acks: [...], timestamp }.
- **Intégrations**: Sources (Erza, Sarah, Clara), canaux (App, Voix, SMS), Home.Graph (log).
- **Dépendances**: Priorités/escalade Aurore (✅ spécifiées), TTS (✅ implémentée).
- **Priorité**: MVP.

#### UC-Aurore-2: Notification agrégée bas stock
- **Acteurs**: Vesta, Aurore, utilisateur, écran rapide.
- **Déclencheur**: Vesta publie `vesta.stock.low` { itemId, quantity, threshold, ... }.
- **Flux**:
  1. Multiples items bas stock en 1 min → Aurore bundlise: "3 produits sous seuil".
  2. Priorité NORMAL, canal par défaut: Écran rapide + Écran lent.
  3. Utilisateur clique → UI détails courses.
- **Intégrations**: Vesta (alertes), Aurore (bundling), UI (action).
- **Dépendances**: Vesta stock (✅ spécifiée), throttling/bundling Aurore (✅ spécifiée).
- **Priorité**: MVP.

#### UC-Aurore-3: Greeting personnalisé matin
- **Acteurs**: Aurore, Clara, Erina, Erza, utilisateur.
- **Déclencheur**: 07:00 chaque jour ou manuel.
- **Flux**:
  1. Aurore récupère (parallèle):
     - Clara: agenda jour (rendez-vous, tâches).
     - Erina: menu jour si planifié.
     - Erza: météo, présence maison.
  2. Génère greeting texte: "Bon matin, Tu as RDV à 10h, météo 15°C, menu ce soir: pasta".
  3. Envoie mobile (SingleUserGreetings) + écran commun (CommonScreenGreetings).
- **Intégrations**: Clara, Erina, Erza, canaux (app, écran).
- **Dépendances**: Greetings (✅ implémentés), intégrations agents.
- **Priorité**: MVP.

#### UC-Aurore-4: Configuration priorités admin
- **Acteurs**: Admin, Aurore admin UI.
- **Flux**:
  1. Admin ouvre "Notification Rules" dans Home.Graph.Public.
  2. Configure:
     - CRITICAL → App+Écran rapide+Voix, ignore quiet hours, escalade 60s.
     - HIGH → App+Écran rapide, respecte quiet hours user, max 5/min.
     - NORMAL → Écran rapide+lent, throttle 1/min par type.
  3. Définit quiet hours mesh: 22:00–07:00 (override perso par user).
  4. Teste avec "Simulate": entre un `request` et voit le routage.
  5. Déploie config → versionnée dans Home.Graph.
- **Intégrations**: Home.Graph (config), UI admin, NATS (pub/sub).
- **Dépendances**: Admin UI (à créer), persistance config Home.Graph.
- **Priorité**: MVP (v2 pour fine-tuning).

---

### Clara (Majordome)

#### UC-Clara-1: Synchronisation bidirectionnelle Microsoft To Do
- **Acteurs**: Clara, Microsoft Graph, utilisateur.
- **Flux**:
  1. Clara crée tâche locale `TodoItem` (ex: "Appeler dentiste").
  2. Publie vers Microsoft Graph API → création dans To Do distant.
  3. Utilisateur modifie titre sur Microsoft → delta reçu par Clara → mise à jour locale.
  4. Utilisateur marque "Done" sur Microsoft → Clara marque status Done localement.
  5. Clara supprime une tâche locale → suppression Microsoft.
  6. Conflits: "last write wins" avec log.
- **Intégrations**: Microsoft Graph, Home.Graph (stockage), audit.
- **Dépendances**: Graph API integration (✅ architecture OK), delta sync (à implémenter).
- **Priorité**: MVP.

#### UC-Clara-2: Récupération automatique vacances scolaires + prestataires
- **Acteurs**: Clara (intégrations externes), calendriers (Gov, Azaé), Home.Graph.
- **Flux**:
  1. Clara télécharge hebdo: vacances scolaires FR (data.gouv ICS par zone) et calendrier prestataires (ex: Azaé via scraping).
  2. Crée/met à jour TodoItem événements (type EventItem) avec AutoActivate si applicable.
  3. Événement "Vacances scolaires 1-7 avril" → pas de scénario automatique, info affichée.
  4. Événement "Nettoyage Azaé mardi 18h-20h" → AutoActivate, ScenarioOnStart/End configuré (ex: "Passer à pied").
  5. Notifications Aurore de confirmation.
- **Intégrations**: External APIs (gov, Azaé), Home.Graph (CRUD), Aurore (notif).
- **Dépendances**: External calendar integrations (✅ implémentées), Vesta (pour tâches dérivées).
- **Priorité**: MVP.

#### UC-Clara-3: Parsing emails réservations → évènements
- **Acteurs**: Clara (email parser), Microsoft Graph, utilisateur.
- **Flux**:
  1. Clara reçoit email contenant JSON-LD FlightReservation (airbnb, kayak, etc.).
  2. Parse: date départ, destination, numéro confirmation, passagers.
  3. Crée TodoItem événement avec metadonnées: titre "Vol AF123 Paris→NY", dueDate, location, lien confirmation.
  4. Optionnel: crée tâches pré-départ ("Imprimer documents", "Appeler taxi").
  5. Affiche dans agenda Clara + notification Aurore.
- **Intégrations**: Microsoft Graph (emails), JSON-LD parsing (à affiner), Home.Graph (CRUD).
- **Dépendances**: Email parsing (partiellement implémenté), JSON-LD parsing (à compléter).
- **Priorité**: MVP.

#### UC-Clara-4: Gestion tâches récurrentes (routines)
- **Acteurs**: Clara (scheduler), utilisateurs.
- **Flux**:
  1. Admin ou utilisateur crée tâche récurrente: "Réunion hebdo lundi 14h".
  2. Clara calcule prochaines occurrences (basé sur `schedule` rule: freq=WEEKLY, byDay=MO).
  3. Crée un TodoItem à J+7 avec `SourceItemId` pointant au template.
  4. À exécution, crée une nouvelle occurrence 7 jours plus tard (déduplication par OriginItemData).
  5. Historique conservé 7 jours + archivage long terme.
- **Intégrations**: Home.Graph (storage), scheduler rules (✅ spécifiée).
- **Dépendances**: ScheduleHelper (✅ implémenté), PIM scheduler (✅ implémenté).
- **Priorité**: MVP.

---

### Erina (Alimentation)

#### UC-Erina-1: Planification repas avec suggestions Vesta
- **Acteurs**: Utilisateur, Erina, Vesta.
- **Flux**:
  1. Utilisateur lance questionnaire: "7 jours, 2 repas/jour, 4 convives, budget moyen, pas trop de pâte récemment".
  2. Erina interroge Vesta: items en stock (farine, pâtes, viande, etc.), dates péremption.
  3. Moteur scoring: recettes ayant ingrédients dispo → score +, péremption proche → score +++, recette faite il y a 3j → score -.
  4. Propose plan: "Lundi: Pâtes bolognaise (88% des ingrédients), Mardi: Ratatouille (100%)...".
  5. Utilisateur valide → retour à Clara pour création évènements/tâches + génération liste courses.
- **Intégrations**: Vesta (read), scoring local.
- **Dépendances**: Vesta disponibilités (✅ spécifiée), questionnaire UI (à créer).
- **Priorité**: MVP.

#### UC-Erina-2: Génération liste courses basée sur plan
- **Acteurs**: Erina, Vesta, Clara.
- **Flux**:
  1. Plan de menu finalisé (recettes + portions saisies).
  2. Erina calcule besoins: ex "farine 500g, tomates 1kg, huile 200ml".
  3. Compare avec stocks Vesta: "farine 100g disponible, reste 400g à acheter".
  4. Génère liste courses, regroupée par catégorie (épicerie, boucherie, produits frais).
  5. Exporte vers Clara comme TaskItem (liste mesh "Courses semaine 1") ou fichier à importer.
  6. Optionnel: suggestions fournisseurs (Budget/Carrefour) si géolocalisation + partenariats.
- **Intégrations**: Vesta (stock), Clara (création tâche/liste), conversion unités.
- **Dépendances**: Vesta (lecture), Clara integration.
- **Priorité**: MVP.

#### UC-Erina-3: Alerte « à consommer vite »
- **Acteurs**: Vesta (watch expiration), Erina, Aurore.
- **Flux**:
  1. Vesta détecte item alimentaire péremption < 3 jours → publie `vesta.item.expiring` { itemId, label, bestBefore }.
  2. Erina souscrit, propose recettes utilisant cet item (ex: "Poulet dans 2 jours? Coq au vin!").
  3. Aurore crée notification: priorité NORMAL, canal écran rapide "⚠️ Utiliser poulet avant demain" + suggestion Erina.
  4. Utilisateur peut cliquer pour voir recette détaillée ou modifier menu.
- **Intégrations**: Vesta (alerts), Erina (scoring recettes), Aurore (notif).
- **Dépendances**: Vesta expiration alerts (✅ spécifiée).
- **Priorité**: MVP.

---

### Vesta (Inventaire)

#### UC-Vesta-1: Entrée stock initial (import CSV)
- **Acteurs**: Admin, Vesta, Home.Graph.
- **Flux**:
  1. Admin prépare CSV: `itemName, quantity, unit, location(Zone/Room/Container), expirationDate, category`.
  2. Upload via UI Vesta → parsing, validation.
  3. Création des items, conteneurs si absents.
  4. Localisation (hiérarchie Zone → Room → Container).
  5. Confirmation import, items visibles dans inventaire.
- **Intégrations**: Home.Graph (UI), CSV parser.
- **Dépendances**: CRUD items/containers (✅ spécifiée).
- **Priorité**: MVP.

#### UC-Vesta-2: Suivi stock avec alertes bas stock
- **Acteurs**: Utilisateur, Vesta, Clara, Aurore.
- **Flux**:
  1. Item configuré avec seuil bas: "Farine, seuil 100g".
  2. Utilisateur décrémente (après cooking) ou scan code-barres: "Farine: 250g → 180g".
  3. Vesta vérifie: 180g > 100g → OK, log mouvement.
  4. Nouveau décrement: 180g → 80g < 100g → publie `vesta.stock.low` { itemId, quantity, threshold }.
  5. Aurore/Clara souscrivent:
     - Aurore: agrège et notifie utilisateur (écran rapide).
     - Clara: ajoute tâche "Acheter farine" à la liste courses.
- **Intégrations**: Vesta (tracking), Clara (tâche), Aurore (notif), Home.Graph (log 7j).
- **Dépendances**: Stock movements (✅ spécifiée), alerts (✅ spécifiée).
- **Priorité**: MVP.

#### UC-Vesta-3: Scan code-barres → lookup produit
- **Acteurs**: Utilisateur (app mobile), Vesta, OpenFoodFacts API.
- **Flux**:
  1. Utilisateur scan EAN "3017760235498" (chocolat Lindt).
  2. Vesta interroge local cache → absent → requête OpenFoodFacts.
  3. Récupère: label "Lindor Truffles", brand, image.
  4. Propose création: "Ajouter 200g Lindor Truffles à Cuisine placard?" ou met à jour item existant.
  5. Utilisateur valide → item ajouté/incrémenté, code-barres enregistré.
- **Intégrations**: OpenFoodFacts (API), Home.Graph (UI mobile).
- **Dépendances**: Barcode lookup (à implémenter).
- **Priorité**: v2.

#### UC-Vesta-4: Maintenance périodique (ex: filtres VMC)
- **Acteurs**: Vesta, Clara, utilisateur.
- **Flux**:
  1. Admin enregistre actif: "Filtre VMC cuisine, s/n ABC123, garantie 2026-01, maintenance tous 3 mois".
  2. Vesta calcule: prochain entretien = now + 3 mois = 24 avril.
  3. Le 20 avril, Vesta publie `vesta.maintenance.due` → Clara crée tâche "Nettoyer filtre VMC".
  4. Utilisateur valide exécution → Clara clôt tâche, Vesta met à jour `nextDueOn`.
- **Intégrations**: Vesta (maintenance tracking), Clara (tâche).
- **Dépendances**: Maintenance workflows (✅ spécifiée).
- **Priorité**: MVP.

---

### Erza (Sécurité & Présence)

#### UC-Erza-1: Détection présence multi-source
- **Acteurs**: Erza, Sarah (devices réseau), utilisateurs.
- **Flux**:
  1. Erza agrège signaux:
     - Détection smartphone sur WiFi (Sarah/Freeia).
     - Présence capteur PIR (Sarah devices).
     - Calendrier (Clara: "busy" ≠ présent, mais corrélation).
  2. Score de présence par utilisateur: somme pondérée des signaux.
  3. Publie `erza.presence.changed` { userId, status: present|away, confidence, room?, timestamp }.
  4. Autres agents souscrivent: Sarah (température), Aurore (greeting), etc.
- **Intégrations**: Sarah (device status), Clara (calendar), Freeia (WiFi).
- **Dépendances**: Presence scoring (✅ spécifiée).
- **Priorité**: MVP.

#### UC-Erza-2: Mode privé mesh (activation/désactivation)
- **Acteurs**: Utilisateur/admin, Erza, Aurore, Sarah (displays).
- **Flux**:
  1. Utilisateur bascule "Mode privé" ON via bouton/commande.
  2. Erza publie `system.mesh.privacymode.changed` { meshId, enabled: true, timestamp }.
  3. Aurore souscrit:
     - Suspend notifications NORMAL/LOW (garde CRITICAL).
     - Masque détails sensibles (personnels, santé) sur écrans publics.
  4. Sarah souscrit:
     - Cache infoI personnelles sur écrans publics (Awtrix/Divoom en salon).
  5. Affichage global: "Mode Privé ACTIVÉ" sur écrans.
- **Intégrations**: Aurore (notification rules), Sarah (display rules), Home.Graph (statut).
- **Dépendances**: Mode privé (✅ spécifiée).
- **Priorité**: MVP.

#### UC-Erza-3: Alerte météo extrême
- **Acteurs**: Erza (weather providers), Aurore, utilisateur.
- **Flux**:
  1. Erza sonde met.no, Météo-France hazards toutes les 30 min.
  2. Détecte: "Alerte tempête pour demain 18h-00h, vitesse vent 80 km/h".
  3. Calcule impact: évaluation risque (HIGH).
  4. Publie `WeatherChangeMessage` (déjà implémenté) ou `erza.weather.alert` { type, severity, description, affectedAreas, timeline }.
  5. Aurore crée notification priorité HIGH, canal App + Écran rapide.
  6. Optional: crée tâche Clara "Rentrer meubles de jardin" si config.
- **Intégrations**: Weather APIs, Aurore (notif), Clara (tâche).
- **Dépendances**: Weather providers (✅ implémentés).
- **Priorité**: MVP.

---

### Freeia (Réseau & Infrastructure)

#### UC-Freeia-1: Reboot Freebox via demande
- **Acteurs**: Utilisateur (demande voix/UI), Freeia, Freebox.
- **Flux**:
  1. Utilisateur dit "Redémarre la box" ou clique "Reboot" admin UI.
  2. Freeia valide auhorisation (admin only), publie commande Freebox API.
  3. Freebox redémarre → Freeia sonde jusqu'à statut "online".
  4. Notification Aurore: "Box redémarrée, internet actif à nouveau".
- **Intégrations**: Freebox API (✅ implémenté), Aurore (notif).
- **Dépendances**: Freebox commands (✅ spécifiée).
- **Priorité**: MVP.

#### UC-Freeia-2: Enumeration Shelly discovery
- **Acteurs**: Freeia (scan réseau), Sarah (integration).
- **Flux**:
  1. Admin clique "Découvrir devices réseau" dans Sarah admin.
  2. Freeia lance scan Shelly (mDNS/DHCP/UPnP).
  3. Trouve devices: "Shelly25-1 (cuisine), Shelly25-2 (chambre)".
  4. Transmet info à Sarah avec propositions: "Intégrer comme roller/switch?".
  5. Admin valide, devices ajoutés dans Sarah config.
- **Intégrations**: Sarah (device manager), network tools.
- **Dépendances**: Shelly discovery (✅ implémenté).
- **Priorité**: MVP.

---

### Gaia (Cluster & Déploiement)

#### UC-Gaia-1: Restart d'un agent suite erreur
- **Acteurs**: Admin/monitoring, Gaia, agents (Sarah, Clara, etc.).
- **Flux**:
  1. Monitoring détecte agent Clara crash → alerte.
  2. Admin clique "Restart Clara" dans Gaia admin UI.
  3. Gaia publie K8s command: kubectl restart deployment clara.
  4. K8s redémarre pod, Gaia sonde jusqu'à prêt.
  5. Notification Aurore: "Agent Clara relancé, services disponibles".
  6. Log: timestamp, raison, durée indisponibilité.
- **Intégrations**: K8s API, Aurore (notif), monitoring.
- **Dépendances**: Kubernetes ops (✅ spécifiée).
- **Priorité**: MVP.

#### UC-Gaia-2: Gestion certificats TLS + FRP
- **Acteurs**: Admin, Gaia, certificats (Let's Encrypt/auto-renew).
- **Flux**:
  1. Cert expiration détectée 30 jours avant → Gaia publie alert.
  2. Admin triggerant renouvellement: kubectl apply new secret.
  3. Gaia met à jour FRP config avec nouveau cert.
  4. Redéploiement FRP (reverse proxy).
  5. Test de connectivité HTTPS, confirmation logs.
- **Intégrations**: Cert management (✅ spécifiée), FRP ops (✅ spécifiée).
- **Dépendances**: K8s secrets.
- **Priorité**: MVP.

---

## Use Cases Transversaux

### UC-Trans-1: Flux complet repas (Erina + Vesta + Clara + Aurore + Sarah)
- **Déclencheur**: "Je veux planifier les repas de la semaine"
- **Flux**:
  1. Utilisateur lance questionnaire Erina (7 jours, contraintes).
  2. Erina interroge Vesta: stocks dispo + dates.
  3. Erina génère plan + liste courses → Clara.
  4. Clara crée:
     - Évènements "Dîner Monday 19h" (Sarah peut déclencher scène "cuisine").
     - Tâches "Courses semaine" (statut sync To Do).
  5. Aurore notifie: "Plan de semaine confirmé, 5 courses à faire".
  6. Jour repas:
     - Sarah: scène "Cuisine" active (lumière, hotte).
     - Après repas: utilisateur marque "utilisé" les ingrédients.
     - Vesta decremente stocks, publie alertes si bas stock.
- **Priorité**: MVP (montre intégration complète).
- **Dépendances**: Erina (planificateur), Vesta (read/write), Clara (évènements/tâches), Aurore (notifs), Sarah (scènes).

### UC-Trans-2: Flux alerte sécurité (Erza + Sarah + Aurore + Clara)
- **Déclencheur**: "Capteur fumée détecte alarme"
- **Flux**:
  1. Sarah reçoit signal capteur fumée → publie `sarah.security.alert` { type: "smoke", severity: CRITICAL, room: "kitchen" }.
  2. Erza souscrit, évalue contexte:
     - Mode privé? Non, alerte passe.
     - Utilisateurs présents? Oui (présence détection).
     - Historique faux positifs? Premier incident.
  3. Érza publie `erza.security.incident` { type: "fire_risk", confidence: HIGH, ... }.
  4. Aurore reçoit → CRITICAL notif avec escalade:
     - App push: "Fumée détectée cuisine, évacuer!".
     - Voix: "Alerte incendie, évacuez le bâtiment".
     - SMS: "Alerte incendie détectée".
  5. Sarah déclenche scène d'urgence: portes ouvertes, éclairage maximum, sirène (si device).
  6. Clara crée tâche urgente: "Appeler pompiers" + log incident.
  7. Utilisateur ack alarme → arrêt sirène, confirmation de sécurité.
- **Priorité**: MVP (critique).
- **Dépendances**: Sarah (sensors), Erza (incident evaluation), Aurore (escalade), Clara (tasks), Sarah (scènes).

### UC-Trans-3: Absence prolongée (Erza + Sarah + Erina + Aurore)
- **Déclencheur**: "Tous les utilisateurs away > 24h"
- **Flux**:
  1. Erza détecte: presence.status = away pour tous les utilisateurs, duration > 24h.
  2. Publie `erza.presence.extended_absence` { duration, users: [...], estimatedReturn: null }.
  3. Sarah souscrit:
     - Mode "absence": réduit chauffage (14°C), éclairage minimal.
     - Logs accès/présence pour monitoring.
  4. Erina souscrit:
     - Suspend planification repas automatique.
     - Alerte péremption non affichée (menu ignoré).
  5. Aurore souscrit:
     - Suspend notifications NORMAL.
     - Greeting mobile: "Mode absence, retour prévu dans X jours".
  6. Utilisateur confirme retour: Erza publie `erza.presence.absence_end` → tous les services reviennent à la normale.
- **Priorité**: MVP.
- **Dépendances**: Erza presence, Sarah automation, Erina planning, Aurore rules.

### UC-Trans-4: Synchronisation agenda multi-sources (Clara + Erina + Sarah + Aurore)
- **Déclencheur**: Nouveau mois ou refresh périodique
- **Flux**:
  1. Clara récupère (parallèle):
     - Microsoft To Do (via Graph delta).
     - Vacances scolaires (data.gouv).
     - Prestataires (Azaé).
     - Utilisateur: saisies manuelles.
  2. Fusionne en calendrier unifié dans Home.Graph.
  3. Erina souscrit:
     - Récupère jours "vacances scolaires" pour ajuster planification.
     - Jours "RDV important" → propose menus légers si prise de temps.
  4. Sarah souscrit:
     - "Réveil 07:30" → scène réveil.
     - "Absence vacances 1-7 avril" → mode absence Sarah activé.
  5. Aurore souscrit:
     - Greeting personnalisé par jour (agenda affiché).
     - Rappel avant rendez-vous (15 min avant si app active).
- **Priorité**: MVP.
- **Dépendances**: Clara multi-sources, Erina planning, Sarah scenes, Aurore rules.

### UC-Trans-5: Mode privé guest (Erza + Aurore + Sarah + Vesta)
- **Déclencheur**: Admin crée compte guest / toggle mode privé
- **Flux**:
  1. Admin crée user "guest" (1 semaine expiration, accès limité).
  2. Erza configure mesh privacy pour guest:
     - Read-only access: some devices, no financial data.
     - Mode privé activé par défaut pour guest.
  3. Aurore applique:
     - Masque notifications sensibles (messages personnels, contacts).
     - Greeting: infos publiques seulement.
  4. Sarah applique:
     - Écrans publics (salon): cache valeurs thermostat/consommation énergétique.
     - Guest voit: température, météo, calendrier public.
  5. Vesta applique:
     - Guest ne voit pas items sensibles (medicaments, coûts).
  6. Guest accède via app/web, scope limité visible.
- **Priorité**: v1.5 (post-MVP).
- **Dépendances**: User roles/permissions, Erza privacy, Aurore rules, Sarah displays, Vesta access.

---

## Dépendances & Ordre d'Implémentation Proposé

### Phase 1 (MVP core)
1. **Sarah**: UC-Sarah-1 (réveil), UC-Sarah-2 (présence), UC-Sarah-4 (appairage).
2. **Clara**: UC-Clara-1 (sync To Do), UC-Clara-2 (vacances + prestataires).
3. **Aurore**: UC-Aurore-1 (priorités/escalade), UC-Aurore-3 (greeting).
4. **Erza**: UC-Erza-1 (présence), UC-Erza-2 (mode privé).
5. **Vesta**: UC-Vesta-1 (import), UC-Vesta-2 (stock alerts).
6. **Transverse**: UC-Trans-1 (repas), UC-Trans-2 (alerte incendie).

### Phase 2 (v1 stabilisation)
- Erina full (planification + listes).
- Clara email parsing.
- Aurore admin UI + config règles.
- Sarah monitoring (UC-Sarah-3).
- Vesta + Erina intégration "use-it-now" alerts.

### Phase 3 (v1.5+ polish)
- Gaia ops.
- Freeia WiFi/downloads.
- Clara agenda multi-sync.
- Vesta barcode lookup.
- Transverse guest mode.

---

## Remarques

- Les dépendances inter-agents sont nombreuses → tests d'intégration critiques.
- MVP doit couvrir un flux utilisateur complet (ex: repas + notifications), pas juste isolated features.
- Chaque UC liste un "Dépendances" → à tracker lors de l'implémentation.
- Topics NATS doivent être réservés et versionnés au fur et à mesure.
