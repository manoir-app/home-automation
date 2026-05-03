# Domaine Domotique - Sarah

**Agent responsable** : Sarah  
**État** : Partiellement implémenté

## Vue d'ensemble

Sarah est l'agent domotique de Manoir, responsable de la gestion des devices (lampes, capteurs, switches, etc.), des scènes et des automatisations.

## Protocoles & Intégrations supportés

Sarah gère actuellement les protocoles et systèmes suivants :

| Protocole/Système | Type | État | Description |
|-------------------|------|------|-------------|
| **Philips Hue** | Hub | ✅ Implémenté | Lampes et switches Hue |
| **Shelly** | Direct Wi-Fi | ✅ Implémenté | Switches, relais, capteurs température |
| **Zigbee2MQTT** | Hub MQTT | ✅ Implémenté | Devices Zigbee via bridge MQTT |
| **WLED** | Direct Wi-Fi | ✅ Implémenté | Contrôleurs LED |
| **Samsung SmartThings** | Cloud/Hub | ✅ Implémenté | Devices Samsung (TV, etc.) |
| **Divoom** | Direct Wi-Fi | ✅ Implémenté | Écrans/enceintes connectées |
| **ZipatoBox** | Hub | ✅ Implémenté | Box domotique Zipato |
| **Plex** | Serveur média | ✅ Implémenté | Serveur média Plex |
| **Emby** | Serveur média | ✅ Implémenté | Serveur média Emby |
| **Storage** | Network | ✅ Implémenté | Stockage réseau |
| **Network Devices** | Network | ✅ Implémenté | Devices réseau génériques |

## Types de devices

### Classification par rôle principal

Les devices sont organisés selon leur rôle principal (`HomeAutomationMainRole`) :

- **`main:bridge`** - Bridges/Hubs (Hue Bridge, Zigbee2MQTT Bridge)
- **`main:light`** - Éclairages
- **`main:shutters`** - Volets et stores
- **`main:sensors`** - Capteurs (température, humidité, etc.)
- Autres à documenter...

### Classification par capacités

Les devices implémentent des interfaces selon leurs capacités :

| Interface | Capacité | Exemples |
|-----------|----------|----------|
| `IToggleSwitchDevice` | On/Off | Switches, prises |
| `IColorBoundDevice` | Couleur | Lampes RGB |
| `IActionButton` | Boutons programmables | Télécommandes, boutons Shelly |
| `IHubDevice` | Hub/Bridge | Hue Bridge, Z2MQTT |

### Types de données (DeviceData)

Chaque device expose des données typées :

**Données principales** :
- `on/off` - État switch
- `up/down` - État volet
- `gradient` - Intensité (brightness)
- `color` - Couleur

**Capteurs** :
- `temperature` - Température
- `humidity` - Humidité
- `pressure` - Pression

**Consommation** :
- `powerconsumption` - Consommation actuelle
- `powerconsumptiontotal` - Consommation totale

**Données secondaires** :
- `alimentation` - Type d'alimentation
- `battery_percentage` - Niveau batterie
- `device_temp` - Température interne du device
- `link_quality` - Qualité du signal

## Architecture technique

### Communication

Sarah communique via **NATS** avec des topics structurés :

**Discovery** (découverte de devices) :
- `homeautomation.discovery.{protocol}.>` (ex: `homeautomation.discovery.hue.>`)

**Commandes** (actions sur devices) :
- `homeautomation.{protocol}.>` (ex: `homeautomation.shelly.>`)

**Gestion Sarah** :
- `homeautomation.devices.sarah.>` - Actions reconnues comme passant par Sarah

### Composants principaux

| Composant | Fichier | Responsabilité |
|-----------|---------|----------------|
| `DeviceManager` | DeviceManager.cs | Gestion centrale des devices, écoute NATS |
| `DeviceManager-Config` | DeviceManager-Config.cs | Configuration des devices |
| `DeviceManager-Status` | DeviceManager-Status.cs | État et statut des devices |
| `ScenesHelper` | Scenes/ScenesHelper.cs | Gestion des scènes |
| `TriggersChecker` | TriggersChecker.cs | Vérification et déclenchement des triggers |

### Helpers spécifiques par protocole

Chaque protocole dispose de son helper dans `Devices/{Protocol}/` :
- `HueHelper` - Gestion Philips Hue
- `ShellyDeviceHelper` - Gestion Shelly
- `Z2MqttHelper` - Gestion Zigbee2MQTT
- `WledHelper` - Gestion WLED
- Etc.

## Scènes

**État** : ✅ Implémenté

Les scènes permettent de regrouper plusieurs actions sur plusieurs devices.

### Structure
- **SceneGroup** : Groupe de scènes (ex: "Salon", "Chambre")
- **Scene** : Scène individuelle (ex: "Soirée film", "Réveil")
- **SceneStep** : Étape d'une scène (action sur un device)

### Fonctionnement
- Cache rafraîchi toutes les 2 minutes depuis Home.Graph
- API endpoints : 
  - `/v1.0/homeautomation/scenes/groups`
  - `/v1.0/homeautomation/scenes/scenes`

### Moteur d’exécution des scènes (délais/étapes)
- Exécution séquentielle par défaut, avec support de groupes parallèles.
- `SceneStep` champs proposés:
  - `action` (device + opération + payload)
  - `delayBeforeMs?` (temporisation avant l’action)
  - `delayAfterMs?` (temporisation après l’action, avant étape suivante)
  - `timeoutMs?` (durée max pour considérer l’étape échouée)
  - `retry?` (nb de réessais, backoff fixe 500ms ou configurable)
  - `parallelGroupId?` (étapes portant le même id s’exécutent en parallèle)
  - `stopOnFailure?` (arrêt de la scène sur erreur de l’étape)
  - `stepConditions?` (conditions locales à l’étape, évaluées juste avant l’action)
- Variantes temporelles: sélection automatique d’une variante de `Scene` par jour/semaine (déjà acté) et/ou par tranche horaire.
- Attente d’état: une étape peut inclure `awaitState` (ex: attendre `brightness >= 50` sur une lampe) avec `timeoutMs`.
- Journalisation: chaque étape loggée (start/end/err) pour 7 jours, consolidée par `Scene`.

## Triggers (Automatisations)

**État** : ✅ Partiellement implémenté

Les triggers permettent de déclencher des actions automatiquement.

### Types de triggers identifiés
- **`TriggerKind.MqttValue`** - Déclenchement sur valeur MQTT

### Fonctionnement
- Thread dédié `TriggersChecker`
- Surveillance continue des conditions
- Exécution d'actions lorsque conditions remplies

## Gestion des Devices

### Découverte et intégration

**Avec Hub** (Hue, Zigbee2MQTT, Freeia pour Shelly) :
1. Le hub détecte automatiquement les devices
2. Le hub remonte les "devices détectés" à Sarah
3. L'utilisateur doit "intégrer" manuellement le device :
   - Associer à un emplacement (Zone > Pièce)
   - Valider/corriger le type de device
   - Saisir des credentials si nécessaire

**Sans Hub** (WiFi direct, Fully Kiosk, etc.) :
- Configuration manuelle via pages d'administration
- Saisie IP, credentials, paramètres

**Cas particulier Shelly** : 
- Freeia détecte automatiquement les Shelly sur le réseau
- Transmet les informations à Sarah pour intégration

### Structure des emplacements

**Hiérarchie** : `Location` > `LocationZone` > `LocationRoom`

**Types de pièces (`RoomKind`)** :
- Generic, Corridor, Bedroom, Bathroom
- Kitchen, LivingRoom, DiningRoom
- Office, ReadingRoom, Pool

**Propriétés des pièces** (`LocationElementProperties`) :
- Temperature, Humidity, Pressure
- Occupancy (état d'occupation)
- MoreProperties (extensible)

### Stockage
- Base de données : **MongoDB**
- Entités : `Device`, `Integration`, `IntegrationInstance`

## Intégrations

Le système d'intégrations est **transversal** à toute la plateforme :
- Sarah : intégrations domotiques (Hue, Shelly, etc.)
- Clara : intégrations SaaS (Gmail, calendriers, etc.)
- Erza : intégrations météo, sécurité
- Etc.

Chaque intégration doit être **activée et configurée** avant utilisation (credentials, endpoints, etc.).

## Scènes

**État** : ✅ Implémenté, 🔄 À améliorer

### Structure actuelle
- **SceneGroup** : Groupe de scènes (par zone, par thème)
- **Scene** : Scène individuelle
- **SceneStep** : Étape d'une scène (action sur un device)

### Portée des scènes
- ✅ Globales (toute la maison)
- ✅ Par zone
- ✅ Par pièce

### Fonctionnalités
- ✅ Cache rafraîchi toutes les 2 minutes
- ✅ Exécution via message NATS
- ✅ Variantes par jour de la semaine (ex: Matin plus tôt le mercredi)
- ✅ Délais entre étapes: `delayBeforeMs`/`delayAfterMs` par `SceneStep`, parallélisme par `parallelGroupId`, `timeoutMs`/`retry`/`stopOnFailure`.

## Triggers et Automatisations

**État** : ✅ Partiellement implémenté

### Types de triggers (`TriggerKind`)

| Type | État | Description |
|------|------|-------------|
| `Clock` | ✅ | Déclenchement temporel avec offset |
| `NetworkDeviceConnectionChanged` | ✅ | Connexion/déconnexion d'un device réseau |
| `Webhook` | ✅ | Webhook HTTP |
| `MqttValue` | ✅ | Changement de valeur MQTT |

### Offsets temporels (`TimeOffsetKind`)
- `FromMidnight` - Depuis minuit (heure fixe)
- `FromSunrise` - Depuis le lever du soleil
- `FromSunset` - Depuis le coucher du soleil
- `FromEarliestWakeup` - Depuis l'heure de lever la plus tôt
- `FromLatestWakeup` - Depuis l'heure de lever la plus tard

### Actions déclenchées

**`RaisedMessages`** - Envoyer des messages NATS :
- Exécuter une scène
- Modifier un device
- Envoyer une notification (via Aurore)
- Message à n'importe quel agent

**`ChangedProperties`** - Modifier des propriétés :
- Propriétés de pièce (température, occupancy, etc.)
- Propriétés génériques
- 🔄 Concept à affiner/améliorer

### Système de conditions

**Structure** : Conditions imbriquées avec opérateurs AND/OR

**Types de vérifications (`ConditionKind`)** :
- `Or` / `And` - Combinaisons logiques
- `DeviceCheck` - Vérifier état d'un device
- `UserCheck` - Vérifier info utilisateur
- `RoomPropertyCheck` - Vérifier propriété de pièce
- `MeshPropertyCheck` - Vérifier propriété du mesh
- `SceneCheck` - Vérifier si scène active

**Opérateurs** : `==`, `!=`, `>`, `<`, `>=`, `<=`, `in`

**Exemple** :
```
AND (
  Device(salon-lampe).on == true
  Room(salon).temperature > 25
)
```

### Configuration MQTT
- `Path` - Chemin de la propriété MQTT
- `JsonPathInValue` - Extraction d'une variable JSON
- `ThresholdForChange` - Seuil minimal de changement (éviter updates trop fréquentes)

## État et Monitoring

### Objectif
Suivre la disponibilité et la santé des devices/protocoles, alerter en cas d’anomalie, et offrir un tableau de bord synthétique.

### Métriques suivies (par device)
- `lastSeen` (horodatage dernière télémétrie/ack)
- `availability` (online | warning | offline)
- `battery_percentage?`, `link_quality?` (RSSI/LQI selon protocole)
- `firmwareVersion?`, `errors?` (si exposé)

### Seuils & règles (par protocole/role)
- Défauts v1:
  - Zigbee2MQTT: `warning` si `lastSeen > 15 min`, `offline` si `> 60 min` (sur batterie); `warning > 5 min`, `offline > 20 min` (secteur).
  - Shelly (Wi‑Fi): `warning > 3 min`, `offline > 10 min`.
  - Hue: `warning > 5 min`, `offline > 20 min` (statut via Bridge + reachability API).
  - WLED: `warning > 5 min`, `offline > 20 min` (ping HTTP + dernier état reçu).
- Criticité par rôle:
  - `main:bridge`, `main:sensors` de sécurité (fumée/eau) ⇒ critiques: alerte immédiate.
  - autres ⇒ alerte standard (regroupable).

### Sources de vérité
- Zigbee2MQTT: topics MQTT `availability` + propriété `last_seen` des devices.
- Shelly: statut HTTP MQTT/CoAP (selon modèle), dernier message télémétrie.
- Hue: `reachable` via Bridge + heartbeat périodique.
- WLED: ping HTTP/JSON + évènements si dispo.

### Alertes (Aurore)
- `device.offline` (critique si rôle critique), `device.back_online` (info).
- Agrégation: burst control sur 2 minutes (max N alertes uniques), résumé si >N.
- Silences planifiés (nuit) configurables, sauf critiques.

### Topics NATS (proposés)
- `sarah.device.status.changed` → { `deviceId`, `availability`, `lastSeen`, `battery?`, `lqi?`, `reason` }
- `sarah.health.summary` (périodique, ex 60s) → { `byProtocol`, `byRoom`, `criticalOffline[]`, `counts` }

### Tableau de bord santé (Home.Graph.Public)
- KPIs: total online/warning/offline, par protocole, par rôle, par pièce.
- Liste critique: ponts/hubs et capteurs critiques offline.
- Détails device: métriques récentes, historique 7j, dernières erreurs.
- Config: seuils par protocole/role, fenêtres de silence, activation monitoring par device.

### Historique & rétention
- États/availability: transitions conservées 7 jours (aligné plateforme).

## Questions en suspens

### 1. Historique
- Durée de conservation : **7 jours maximum**
- Granularité : Toutes les transitions d'état

### 2. Mises à jour
- Gestion des mises à jour firmware des devices ?
- Notifications de mises à jour disponibles ?

### 3. Groupes de devices
- Peut-on grouper des devices pour actions simultanées (hors scènes) ?
- Groupes par type, par pièce, personnalisés ?

### 4. Protocoles supplémentaires
- Z-Wave prévu ?
- Matter/Thread ?
- HomeKit bridge ?
- KNX ?

---

## Tableau des technologies domotiques

### Protocoles radio/Sans-fil

| Technologie | Portée | Débit | Fréquence | Avantages | Inconvénients | Intégration Manoir |
|-------------|--------|-------|-----------|-----------|---------------|-------------------|
| **Zigbee** | 10-100m | 250 kbps | 2.4 GHz | Mesh, faible conso | Débit limité | ✅ Via Zigbee2MQTT |
| **Z-Wave** | 30m | 100 kbps | 868/915 MHz | Propriétaire, fiable | Moins d'appareils | 🔄 Prévu ? |
| **Thread** | 100m | 250 kbps | 2.4 GHz | Mesh, Matter compatible | Récent, peu d'appareils | 🔄 Prévu ? |
| **Matter** | Dépend protocole | Variable | Dépend | Standard ouvert | Encore en maturation | 🔄 À évaluer |
| **WiFi** | 50m | 11-866 Mbps | 2.4/5 GHz | Débit, ubiquitaire | Haute consommation | ✅ Shelly, Samsung, etc. |

### Solutions propriétaires

| Système | Protocole | Hub requis | Intégration Manoir |
|---------|-----------|------------|-------------------|
| **Philips Hue** | Zigbee | Oui (Hue Bridge) | ✅ Via HueHelper |
| **IKEA Tradfri** | Zigbee | Oui (Gateway) | ✅ Via Zigbee2MQTT |
| **Shelly** | WiFi + Cloud | Non | ✅ Détection Freeia |
| **Samsung SmartThings** | Multi-protocole | Oui | ✅ Via SmartThingsHelper |
| **Divoom** | WiFi | Non | ✅ Via DivoomHelper |
| **WLED** | WiFi | Non | ✅ Via WledHelper |

### Serveurs/Hubs centralisés

| Solution | Protocoles | Rôle dans Manoir |
|----------|-----------|------------------|
| **Zigbee2MQTT** | Zigbee | Hub Zigbee, expose via MQTT |
| **ZipatoBox** | Multi | Hub général domotique |
| **Home Assistant** | Multi | Possible intégration future |
| **Freeia** | Réseau/WiFi | Hub réseau, détecte Shelly |

### Technologies futures/À explorer

| Technologie | Description | Priorité |
|-------------|-------------|----------|
| **HomeKit** | Bridge HomeKit Apple | Basse |
| **Google Home** | Bridge Google | Basse |
| **KNX** | Standard domotique européen | Moyenne |
| **LoRaWAN** | IoT longue portée | Basse |
| **Bluetooth Mesh** | Mesh Bluetooth | À évaluer |
