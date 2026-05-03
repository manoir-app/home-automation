# Domaine Communication - Aurore

**Agent responsable** : Aurore  
**État** : Partiellement implémenté

## Vue d'ensemble

Aurore est l'agent de communication et d'affichage de Manoir. Elle gère :
- Les notifications utilisateur (mobiles, écrans, voix)
- Les messages et chat
- L'animation visuelle sur écrans
- La synthèse vocale (text-to-speech)
- Les accueils et salutations
- L'intégration avec des écrans/enceintes connectées

## Canaux de communication

Manoir supporte **5 canaux de communication** pour les notifications et informations :

### 1. SMS / App Notifications
- **Cibles** : Mobile (app native) + Desktop (web push)
- **Types** : SMS, push notification
- **Latence** : Immédiate
- **Cas d'usage** : Alertes urgentes, confirmations temps réel

### 2. Email
- **Cible** : Email de l'utilisateur
- **Latence** : Quasi-immédiate (quelques secondes)
- **Cas d'usage** : Résumés, documents, confirmations

### 3. Voix
- **Technologie** : ✅ Azure Cognitive Services Text-to-Speech
- **Intégration** : ✅ SpeechHelper.cs
- **Latence** : Immédiate (une fois générée)
- **Cas d'usage** : Alertes sonores, annonces, confirmations vocales

### 4. Écrans "rapides"
- **Cibles** : Awtrix, Divoom
- **Latence** : Immédiate
- **Cas d'usage** : Notifications flash, infos instantanées, alertes

### 5. Écrans "lents"
- **Cibles** : Dashboards web, affichages persistants
- **Latence** : Mise à jour périodique
- **Cas d'usage** : Calendriers, statut devices, météo, agenda

#### Awtrix
- **Type** : Écrans connectés (displays matriciels)
- **Intégration** : ✅ AwtrixHelper.cs
- **Capacités** :
  - Messages texte avec icône
  - Notifications avec durée
  - Apps configurables (ChatApp, PrivacyApp, custom)
  - Envoi en broadcast à tous les Awtrix configurés
- **Gestion des apps** :
  - Configuration : quelle app sur quel device
  - Durée de vie : chaque app se "détruit" après X secondes
  - Ordre d'affichage : géré par Awtrix, Aurore envoie seulement

#### Divoom
- **Type** : Écrans/enceintes connectées (Divoom TimeBox, etc.)
- **Intégration** : ✅ DivoomHelper.cs
- **Capacités** :
  - Messages animés
  - Apps configurables (ChatApp, PrivacyApp, custom)
  - Affichage d'images
- **Gestion des apps** :
  - Configuration : quelle app sur quel device
  - Durée de vie : chaque app se "détruit" après X secondes

### Affichage écrans "lents"
- 🔄 Dashboards web
- 🔄 Affichages persistants (journal, calendrier)
- 🔄 Widgets sur TV/écrans secondaires

### Communication vocale

#### Synthèse vocale (Text-to-Speech)
- **Technologie** : ✅ Azure Cognitive Services Speech
- **Intégration** : ✅ SpeechHelper.cs
- **Fonctionnement** :
  - Convertit texte en audio WAV
  - Caching basé sur MD5 du contenu
  - Voix français (fr-FR) configurable
  - Support SSML pour contrôle de diction
- **Stockage** : Fichiers WAV cachés localement

#### Reconnaissance vocale (Speech-to-Text)
- **Technologie** : ✅ Rhasspy (open-source)
- **Intégration** : ✅ RhasspyService.cs
- **Fonctionnement** :
  - Reconnaissance locale (offline)
  - Gestion des utterances et slots
  - Cache des modèles de langage

### Notifications

#### Types de notifications implémentés

| Type | Cible | État | Description |
|------|-------|------|-------------|
| **Greeting** | Mobile | ✅ | Salutations mobiles personnalisées |
| **Chat Activity** | Écrans | ✅ | Notification d'activité chat |
| **Privacy Mode Change** | Écrans | ✅ | Notification changement mode privé |
| **Weather Change** | Écrans | ✅ | Alerte changement météo |
| **User Change** | Mobile | ✅ | Notification utilisateur créé/modifié/supprimé |
| 🔄 Custom | Variable | 🔄 | Notifications personnalisées |
| 🔄 Priority/Alert | Variable | 🔄 | Alertes prioritaires |

## Affichages et Interfaces

### Accueils (Greetings)
**État** : ✅ Implémenté

Affichages personnalisés selon contexte :

**Destinations** :
- **Screen** - Écran commun (accueil général)
- **Mobile** - Notification mobile personnalisée par utilisateur

**Contenu généré** :
- ✅ Statut du mesh (date, météo)
- ✅ Infos utilisateur personnalisées
- ✅ Actualités
- ✅ Agenda proche
- ✅ Tâches TODO
- 🔄 Domotique (statut devices)
- 🔄 Sécurité (alarmes, présence)

**Types d'accueils** :
- `CommonScreenGreetings` - Accueil écran commun
- `SingleUserGreetings` - Accueil individuel (mobile)
- `MultipleUserGreetings` - Accueil multi-utilisateur

### Journal (Journal Helper)
**État** : ✅ Implémenté partiellement

Gestion de pages/sections pour affichage persistant :
- Upload de propriétés de sections
- Gestion de pages thématiques
- 🔄 Édition et mise à jour d'affichages

### Recherche (TypeSense)
**État** : ✅ Implémenté

Integration avec TypeSense pour :
- Indexation complète du contenu (chat, documents, etc.)
- Recherche full-text
- Gestion des index

## Priorités, Throttling & Règles d’envoi

**État** : 🔄 À implémenter (modèle spécifié)

### Modèle de priorités
- Niveaux: `CRITICAL`, `HIGH`, `NORMAL`, `LOW`.
- Catégories: `security`, `safety`, `system`, `agenda`, `inventory`, `downloads`, `weather`, `custom`.
- Contexte: `meshPrivacyMode`, `userPresence`, `userDnd`, `quietHours`.

### Mapping canaux par défaut (pré‑règles)
| Priorité | Canaux par défaut | Note mode privé |
|----------|-------------------|-----------------|
| CRITICAL | App/SMS + Écran rapide + Voix | Ignorer quiet hours (sauf override) |
| HIGH | App + Écran rapide | Respecte quiet hours, escalade optionnelle |
| NORMAL | Écran rapide + Écran lent + Log | Filtrable par préférences |
| LOW | Écran lent + Log | Souvent agrégé/bundlé |

### Règles & évaluation (ordre)
1) Déduplication: éviter doublons (même `type/source/correlationId`) sur fenêtre T (ex: 2 min).
2) Suppression conditionnelle: si `userDnd` actif et priorité < HIGH, supprimer ou retarder.
3) Quiet hours: plage horaire définie au niveau mesh (baseline), avec overrides par utilisateur; CRITICAL passe, HIGH configurable.
4) Throttling: limites par `type` et par `canal` (ex: max 1/min/device pour offline).
5) Bundling/Agrégation: regrouper items similaires (ex: « 3 devices offline »).
6) Choix canal effectif: appliquer policy (priorité × catégorie × contexte × préférences utilisateur/mesh).
7) Escalade: si non accusé réception (`ack`) sous délai (ex: 60s) pour CRITICAL/HIGH → passer au canal suivant. Ordre par défaut CRITICAL: App → Voix → SMS.

### Préférences & overrides
- Par utilisateur: canaux autorisés par priorité/catégorie, overrides de quiet hours personnelles, DND.
- Par mesh: règles globales (quiet hours baseline, écrans en salon, etc.).
- Mode privé: masque contenu sensible et bascule canaux vers non visibles (écrans publics), sauf CRITICAL sécurité.

### Accusés & lifecycle
- `notification.sent` → `notification.delivered?` → `notification.acknowledged` → `notification.escalated?` → `notification.resolved`.
- Acknowledgement: par app/mobile/voix; lien avec évènement déclencheur (ex: alarme effacée).

### NATS (proposés)
- `aurore.notify.request` → { `id`,`type`,`priority`,`category`,`targets[]`,`channels?`,`title`,`body`,`data`,`correlationId` }
- `aurore.notify.sent`/`delivered`/`ack`/`escalated`/`error` → { `id`,`channel`,`timestamp`,`details` }

### Persistance & audit
- Journal des envois (min: 7 jours) avec horodatage, canal, statut.
- Option d’historique utilisateur (consultation dans l’UI).

### Throttling détaillé
- Fenêtres glissantes par `type` (ex: `device.offline`), par `deviceId`, et par canal.
- Backoff exponentiel pour sources « bruyantes » (ex: connectivité instable).

## Administration (Aurore Admin)

**État** : 🔄 À spécifier/implémenter

### Écran de configuration
- Règles par priorité/catégorie: mapping canaux, escalade (ordre CRITICAL par défaut App → Voix → SMS), délais, quiet hours.
- Préférences par utilisateur: canaux autorisés, overrides de quiet hours perso, DND.
- Mode privé: politique d’affichage (masquage champs sensibles) par catégorie et canal.
- Limites/Throttling: quotas par type/canal, fenêtres d’agrégation.

### Outils
- Simulation: saisir un `request` et voir le routage prévu (canaux/règles appliquées).
- Test envoi: envoi « dry-run » ou réel vers un utilisateur de test.
- Journal/Audit: liste filtrable des envois, stats (par canal, par catégorie), échecs.

### Stockage & API
- Stockage des règles dans Home.Graph (config editable) avec versions.
- Endpoints d’admin (REST) pour CRUD de politiques, préférences et silences.

## Mode Privé

**État** : ✅ Concept existant, 🔄 À affiner

- Gestion assurée par Erza (agent sécurité)
- Impact sur Aurore : limitation/mute des notifications
- Topic NATS : `system.mesh.privacymode.changed`
- 🔄 Règles précises à définir avec Erza

Aurore intègre des outils pour interaction avec LLM :

| Outil | Fonction |
|-------|----------|
| `GetMainUsersTool` | Récupère les utilisateurs principaux |
| `GetWeatherTool` | Récupère météo locale |
| `GetUserTodosTool` | Récupère TODOs utilisateur |
| `GetPresentUsersTool` | Récupère utilisateurs présents |
| `SendNotificationTool` | Envoie notification |
| `LlmToolsManager` | Gère disponibilité des outils |

## Architecture technique

### Communication NATS
Aurore écoute les topics :
- `aurore.>` - Messages dirigés à Aurore
- `pim.chat.>` - Activité chat (calendrier/todo)
- `system.mesh.privacymode.changed` - Changements mode privé
- `greetings.>` - Demandes d'accueil
- `users.accounts.>` - Changements utilisateurs
- `communication.>` - Messages de communication générales
- `security.alarm` - Alertes de sécurité
- `WeatherChangeMessage.TopicName` - Changements météo

### Composants principaux

| Composant | Fichier | Responsabilité |
|-----------|---------|----------------|
| Handler | AuroreMessageHandler.cs | Traitement des messages NATS |
| Speech | SpeechHelper.cs | Synthèse vocale |
| Greetings | GreetingsHandler.cs | Génération accueils |
| Journal | JournalHelper.cs | Gestion pages/sections |
| Awtrix | AwtrixHelper.cs | Intégration écrans Awtrix |
| Divoom | DivoomHelper.cs | Intégration Divoom |
| Rhasspy | RhasspyService.cs | Reconnaissance vocale |
| TypeSense | TypeSenseSearchProvider.cs | Moteur recherche |

## Questions en suspens

### 1. Configuration des priorités
- Fichier config statique ou interface d'admin ?
- À quel niveau : par type de notification ou par canal ?
- Qui définit les règles : admin system ou chaque utilisateur ?

### 2. Throttling/Limitation
- Doit-on limiter la fréquence des notifications du même type ?
- Exemple : max 1 notification "device offline" par 5 minutes ?
- Agrégation possible ? (ex: "3 devices offline" au lieu de 3 notifs)

### 3. Persistance des notifications
- Historique des notifications envoyées ?
- Durée de conservation ?
- Base de données ou logs seulement ?

### 4. SMS/Email
- Quel service SMTP pour l'email ? Configuration ?
- Provider SMS (Twilio, Nexmo) ?
- Numéro de téléphone des utilisateurs ? Où stocké ?

### 5. Voix
- Langue automatique selon utilisateur ou statique (fr-FR) ?
- Voix masculine/féminine préférence ?
- Lire les notifications automatiquement ou sur demande ?

### 6. Affichage écrans "lents"
- Est-ce que c'est une responsabilité d'Aurore ou d'un autre agent ?
- Mise à jour en temps réel ou polling périodique ?

### 7. Interaction avec Erza (Sécurité/Présence)
- Quelles sont les règles exactes en mode privé ?
- Qui est responsable de switcher le mode privé (Erza uniquement) ?
