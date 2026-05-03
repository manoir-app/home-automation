# Domaine Sécurité & Présence - Erza

**Agent responsable** : Erza  
**État** : Partiellement implémenté

## Vue d'ensemble

Erza est l'agent de sécurité, monitoring et gestion de présence de Manoir. Elle gère :
- Détection et suivi de présence des utilisateurs
- Alertes météorologiques et aléas (tempête, canicule, etc.)
- Monitoring de la connectivité réseau et serveurs
- Vérification des certificats SSL/TLS
- Gestion du mode privé (privacy mode)
- Nettoyage et maintenance système
- Gestion des invités (guests)

## Modules principaux

### 1. Présence des utilisateurs
**État** : ✅ Partiellement implémenté

#### Détection de présence
- **Input** : Topic NATS `users.presence.>` 
- **Sources d'input** (priorité) :
  1. **Téléphone sur réseau** (WiFi MAC, DHCP) - 🔄 Implémenté (principal)
  2. **Beacon BLE** - 🔄 À implémenter (secondaire)
  3. **Identification caméra + IA** - 🔄 À implémenter (tertiaire)
  4. **Activité compte utilisateur** - 🔄 À implémenter (détection interactive)
  5. **Entrée manuelle** - ✅ À supporter (override/fallback)
- **Logique** : 
  - Combine multiples sources pour haute confiance
  - Détection automatique via entrées Mesh
  - Détection via activités (interaction avec devices, login account)
  - Nettoyage des présences stales (toutes les 2 minutes)
- **Dépendances** : Données location/rooms pour contexte

#### État de présence
- **`User.PresenceState`** : `Present` / `Away` / `Offline` / `Unknown`
- **Durée minimale** : Pour éviter fluctuations, prestige doit être stale d'au moins 2min avant changement
- **Contexte** : Quelle room, à quel timestamp

#### Notifications de présence
- Déclenche des messages NATS : `users.presence.changed`
- Notifie Aurore pour accueils et affichages
- Peut déclencher des automatisations (scènes, triggers)

### 2. Météo et Aléas
**État** : ✅ Partiellement implémenté

#### Providers météo
| Provider | Type | État | Fonctionnalité |
|----------|------|------|----------------|
| **Met.no** | ✅ | Implémenté | Prévisions météo |
| **Météo-France** | ✅ | Implémenté | Aléas (tempête, canicule, etc.) |
| Custom | 🔄 | À implémenter | Autres sources possibles |

#### Données collectées
- **Température** → propriété de room
- **Humidité** → propriété de room  
- **Pression** → propriété de room
- **Aléas** → alertes (tempête, grand froid, canicule, inondation, etc.)

#### Fréquence de mise à jour
- Vérifie toutes les 15 minutes (~900 secondes)
- Envoie notifications seulement si changement significatif

### 3. Monitoring Réseau
**État** : ✅ Partiellement implémenté

#### Vérification connectivité
- **Offline check** : Test connectivité WAN (ping)
- **Public server check** : Vérification statut des serveurs publics
- **Fréquence** : Toutes les 30 secondes

#### Vérification certificats SSL/TLS
- **Certificats importants** : Services internes, APIs externes
- **Alertes** : Certificat expirant, expiré, invalide
- **Fréquence** : Quotidienne ou hebdomadaire

#### Monitoring OVH/Azure
- Intégration APIs OVH et Microsoft Azure
- Alertes statut services
- 🔄 Détails à préciser

### 4. Mode Privé
**État** : ✅ Concept existant, 🔄 À spécifier

Mode de sécurité global du Mesh activable manuellement ou automatiquement :

**Objectif** : Ne pas afficher les données privées quand il y a des invités/inconnus.

**Activation** :
- Manuelle : Admin peut activer/désactiver
- Automatique : Horaires (ex: 22h-7h privé), détection invités
- Déclenchement : Changement statut utilisateur (guest arrive, user quitte)

**Effets quand ACTIF** :
- Aurore : Mute notifications non-essentielles
- Écrans : Cache informations sensibles (agendas, cameras, statuts devices)
- Domotique : Peut désactiver certaines scènes/automations
- Monitoring : Logs cachés, alertes non-critiques mutées

**Scope** : 
- Concerne tous les utilisateurs présents au Mesh
- Un invité/guest arrive → passe en mode privé
- Plus d'invité/guest → peut revenir à mode normal

**Topic NATS** : `system.mesh.privacymode.changed`
**Responsable d'activation** : Erza (basée sur présence de guests)

### 5. Gestion des Utilisateurs
**État** : ✅ Partiellement implémenté

#### Users vs Guests
- **Users** : Résidents permanents
- **Guests** : Visiteurs temporaires

#### Lifecycle
- Création, modification, suppression d'utilisateurs
- Topic NATS : `users.accounts.>` (create, update, delete)
- Synchronisation avec autres agents

### 6. Nettoyage et Maintenance
**État** : ✅ Partiellement implémenté

#### Database maintenance
- Vérification intégrité MongoDB
- Nettoyage données obsolètes
- Optimisation indices
- Fréquence : Périodique (détails à préciser)

#### System cleanup
- Nettoyage fichiers temporaires
- Archivage logs
- Gestion quotas disque
- 🔄 Détails à affiner

### 7. Synchronisation source de données
**État** : ✅ Implémenté

**GitSync** : Synchronisation des configurations depuis Git/Gitea

| Élément | Fréquence | Déclencheur |
|---------|-----------|------------|
| **Intégrations** | À la demande | Topic NATS |
| **Triggers** | À la demande | Topic NATS |
| **Scènes** | À la demande | Topic NATS |
| **Pages Journal** | À la demande | Topic NATS |

## Architecture technique

### Communication NATS
Erza écoute les topics :
- `erza.>` - Messages dirigés à Erza
- `users.presence.>` - Changements de présence
- `system.mesh.globalscenario.>` - Scénarios globaux
- `system.triggers.change` - Modifications triggers
- `security.>` - Messages sécurité
- `monitoring.>` - Messages monitoring
- `LogMessage.LogMessageTopic` - Logs applicatifs
- `IntegrationInstancesListChangedMessage` - Changements intégrations
- `ScenarioContentChangedMessage` - Changements scénarios
- `SourceCodeChangedMessage` - Changements source
- `JournalPageChangedMessage` - Changements pages journal

### Composants principaux

| Composant | Fichier | Responsabilité |
|-----------|---------|----------------|
| Handler | ErzaMessageHandler.cs | Traitement messages NATS |
| Presence | PresenceHelper.cs | Détection et suivi de présence |
| Weather | WeatherIntegration.cs | Collecte alertes météo |
| Network | NetworkChecker.cs | Monitoring connectivité |
| Certificates | NetworkChecker-Certificate.cs | Vérification certificats |
| OVH/Azure | OvhHelper.cs, AzureNetHelper.cs | Monitoring infrastructure |
| Database | DatabaseMaintenanceThread.cs | Maintenance MongoDB |
| Cleanup | SystemCleanup.cs | Nettoyage système |
| Users | UserHelper.cs | Gestion utilisateurs/guests |
| Mesh | MeshHelper.cs | Infos globales mesh |
| GitSync | GitSync.cs | Synchronisation Git |

## Questions en suspens

### 3. Mode Privé (Privacy Mode)
- Qui peut activer/désactiver le mode privé ? (admin seulement, n'importe quel user)
- Activation automatique sur horaires ? (ex: 22h-7h = privé)
- Interaction avec domotique : couper caméras, fermer volets, etc. ?
- Rôle d'Erza vs autres agents dans l'implémentation ?

### 4. Seuils d'alerte météo
- Quels seuils pour canicule, grand froid, neige abondante, etc. ?
- Notifications immédiatement ou par batch quotidien ?
- Priorité des alertes (critical pour aléas majeurs ?) ?

### 5. Détection de présence multi-sources
- Algorithme de fusion des sources (AND, OR, weighted score) ?
- Confiance minimale avant considérer user present ?
- Timeout avant de considérer user absent (10min, 30min, 1h) ?

### 6. Détection caméra + IA
- Service IA à utiliser ? (local open-source ou cloud)
- Caméras domotiques à surveiller (Sarah) ou nouvelles caméras (Erza) ?
- Privacy : bonne gestion des données vidéo ?

### 7. Maintenance système
- Fréquence nettoyage BD (quotidienne, hebdomadaire) ?
- Durée de rétention des logs (7j, 30j, 1 an) ?
- Alertes si BD se dégrade ?

### 8. Certificats SSL
- Alerte combien de jours avant expiration ? (14j, 7j, 1j)
- Qui doit être alerté ? (admin, logs, notification)

### 9. Monitoring infrastructure
- Quels services OVH/Azure monitorer ?
- Actions automatiques en cas de panne ?

### 10. Guests
- Durée de vie automatique (créé/expiré) ?
- Permissions réduites vs users normaux ?
- Données personnelles supprimées après expiration ?
