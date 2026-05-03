# Domaine Réseau & Infrastructure - Freeia

**Agent responsable** : Freeia  
**État** : Partiellement implémenté

## Vue d'ensemble

Freeia est l'agent de gestion du réseau et de l'infrastructure de Manoir. Elle gère :
- Monitoring et contrôle de la **Freebox** (box Internet FAI française)
- Détection automatique des devices Shelly sur le réseau
- Gestion des téléchargements (HTTP, FTP, Torrent, Magnet)
- Monitoring de la connectivité et statut WAN
- Gestion WiFi et sécurité réseau
- Tools réseau (DNS, ping, traceroute, etc.)

## Freebox API

**État** : ✅ Intégration complète

Freeia communique avec la Freebox v6+ via son API REST v4 authentifiée.

### Authentification
- **Système** : OAuth-like avec QR code
- **Flow** : App token → Session token → Requêtes authentifiées
- **Certificats** : Root CA Freebox (ECC et RSA) intégrés
- **Sécurité** : HTTPS + validation certificat

### Fonctionnalités Freebox implémentées

| Fonctionnalité | État | Responsabilité |
|----------------|------|----------------|
| **Statut connexion** | ✅ | Monitoring WAN (débit, IP, état) |
| **Reboot** | ✅ | Redémarrage de la box |
| **WiFi QR Code** | ✅ | Génération QR code WiFi |
| **Gestion WiFi** | ✅ | Configuration SSID, sécurité |
| **Download Manager** | ✅ | Gestion téléchargements |
| **Network Tools** | 🔄 | DNS, ping, traceroute |
| **Logs Freebox** | 🔄 | Accès logs système box |

## Détection Devices Shelly

**État** : ✅ Implémenté

Freeia détecte automatiquement les devices Shelly sur le réseau WiFi :
- **Scan** : Énumération des devices WiFi
- **Identification** : Reconnaissance des Shelly (par MAC, ou interrogation)
- **Info** : IP, model, firmware, alias
- **Communication** : Transmet à Sarah via discovery messages NATS

**Topic NATS** : `homeautomation.discovery.shelly.>`

## Gestion des Téléchargements

**État** : ✅ Partiellement implémenté

Gestion des téléchargements via la Freebox ou via applications externes :

### Protocoles supportés
- **HTTP/HTTPS** - Téléchargement direct
- **FTP/FTPS** - Protocole FTP
- **Magnet links** - Torrent via magnet link
- **Torrent files** - Fichiers torrent
- **YouTube** - Via youtube-dl

### Fonctionnement
1. Réception message NATS `DownloadItemMessage`
2. Validation URL et protocole
3. Envoi à Freebox Download Manager
4. Retour statut (queued, error, identifier)

### Limitation
- YouTube nécessite youtube-dl (externe, non intégré)

## Monitoring Réseau

**État** : ✅ Partiellement implémenté

### Statut WAN
- **Débit** : Rate down/up (en temps réel)
- **Bande passante** : Bandwidth utilisée
- **Données** : Bytes up/down (total session)
- **État** : Connected / Disconnected / Authenticating
- **Type connexion** : XDSL, Fibre, etc.
- **IP** : IPv4, IPv6, port range

### Notifications
- Changement état WAN (up/down)
- Alerte débit anomal ?
- 🔄 À préciser

## Network Tools

**État** : 🔄 Partiellement implémenté

Tools de diagnostic réseau via API Freebox :

| Outil | État | Usage |
|-------|------|-------|
| **DNS** | 🔄 | Résolution DNS |
| **Ping** | 🔄 | Diagnostic connectivité |
| **Traceroute** | 🔄 | Analyse chemin réseau |
| **Speed test** | 🔄 | Test débit |

## Architecture technique

### Communication NATS
Freeia écoute les topics :
- `freeia.>` - Messages dirigés à Freeia
- `system.network.devices.enumerate` - Énumération devices réseau
- `homeautomation.devices.freeia.>` - Actions domotique Freeia
- `system.triggers.change` - Changements triggers
- `DownloadItemMessage.TopicName` - Demandes téléchargement

### Composants principaux

| Composant | Fichier | Responsabilité |
|-----------|---------|----------------|
| Handler | FreeiaMessageHandler.cs | Traitement messages NATS |
| Freebox | FreeboxHelper.cs | API Freebox générale |
| Status | FreeboxHelper-Status.cs | Monitoring statut WAN |
| WiFi | FreeboxHelper-WifiTools.cs | Gestion WiFi |
| Downloads | FreeboxHelper-Downloads.cs | Gestion téléchargements |
| NetworkTools | FreeboxHelper-NetworkTools.cs | Tools réseau |
| Downloads | DownloadHelper.cs | Orchestration téléchargements |

## Questions en suspens

### 1. Box Internet
- Freebox est-elle le seul box supporté, ou faut-il abstraire pour d'autres BOX (Livebox, Bbox) ?
- Quelle version minimale de Freebox (v6+) ?
- Gestion de plusieurs Freebox (mode multi-location) ?

### 2. Network Tools
- Quels tools réseau sont essentiels ? (DNS, ping, traceroute, speed test)
- Intégration dans UI admin ou automatique uniquement ?
- Limitation de fréquence (throttling) des tests lourds ?

### 3. Téléchargements
- Limitation de bande passante pour les téléchargements ?
- Queue de priorité entre téléchargements ?
- Notification quand téléchargement termine ?
- Stockage sur disque interne Freebox ou NAS externe ?

### 4. Monitoring avancé
- Alertes sur anomalies de débit ?
- Historique de la connectivité (pour statistiques) ?
- Détection de coupures courtes vs longues ?
- Monitoring de latence / packet loss ?

### 5. Shelly
- Fréquence de scan des devices Shelly ?
- Gestion des changements (device ajouté/retiré) ?
- Synchronisation avec détections autres (BLE, caméra) ?

### 6. Sécurité réseau
- Gestion du pare-feu Freebox ?
- Gestion des ports ouverts / forwarding ?
- Alertes sur intrusions/scans ?
- Mode sécurisé (DMZ, isolation réseau) ?

### 7. WiFi
- Gestion multi-SSID ?
- Scheduling WiFi (on/off sur horaires) ?
- Isolation réseau guest vs main ?
- Canal/fréquence auto vs manuel ?
