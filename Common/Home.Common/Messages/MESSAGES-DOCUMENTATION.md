# Documentation des Messages - Home Automation System

Ce document récapitule tous les messages disponibles dans le système d'automatisation domestique, leur usage et leur topic NATS associé.

## Table des matières

1. [Messages de Base](#messages-de-base)
2. [Messages Système](#messages-système)
3. [Messages Domotique](#messages-domotique)
4. [Messages Utilisateur](#messages-utilisateur)
5. [Messages PIM (Personal Information Management)](#messages-pim)
6. [Messages Réseau](#messages-réseau)
7. [Messages Intégration](#messages-intégration)
8. [Messages Communication](#messages-communication)
9. [Messages Monitoring](#messages-monitoring)

---

## Messages de Base

### BaseMessage
**Fichier**: `BaseMessage.cs`

Classe abstraite de base pour tous les messages du système.

**Propriétés**:
- `Topic`: Le topic NATS sur lequel le message est publié

**Méthodes utilitaires**:
- `ReadAs<T>()`: Désérialise un JSON en message typé
- `GetTopic()`: Extrait le topic d'un message JSON

**Classes associées**:
- `MessageResponse`: Réponse générique aux messages
- `MessageOrigin`: Enum définissant l'origine (External, System, Local)

---

## Messages Système

### SystemDeploymentMessage
**Topic**: `gaia.deployments`  
**Fichier**: `SystemDeploymentMessages.cs`

Gère le déploiement et le redémarrage des composants système.

**Usage**: Déployer des agents, applications web ou redémarrer des services  
**Propriétés**:
- `DeploymentName`: Nom du déploiement
- `Action`: Type d'action (Restart, DeployGeneric, DeployAgent, DeployWebApp)
- `SourceFileContent`: Contenu du fichier source à déployer

---

### SystemReverseProxyChangeMessage
**Topic**: `gaia.refresh-reverse-proxy`  
**Fichier**: `SystemReverseProxyChangeMessage.cs`

Notifie un changement de configuration du reverse proxy.

**Usage**: Rafraîchir la configuration FRP (Fast Reverse Proxy)  
**Propriétés**:
- `FrpConfigFile`: Nouveau fichier de configuration

---

### SystemGetContainersMessage
**Topic**: `gaia.getcontainers`  
**Fichier**: `SystemGetContainersMessage.cs`

Récupère la liste des conteneurs en cours d'exécution.

**Usage**: Monitoring et gestion des conteneurs  
**Réponse**: Liste des conteneurs avec leur nom et dernière mise à jour

---

### SystemCertificateChangeMessage
**Topic**: `gaia.refresh-certificate`  
**Fichier**: `SystemCertificateChangeMessage.cs`

Notifie un changement de certificat SSL/TLS.

**Usage**: Rafraîchir les certificats sur tous les services

---

### SystemScriptExecuteMessage
**Topic**: `system.script.execute`  
**Fichier**: `SystemScriptExecuteMessage.cs`

Execute un script système.

**Usage**: Exécution de scripts pour automatisation système  
**Propriétés**:
- `ScriptContent`: Contenu du script à exécuter

---

### MeshInfoMessage
**Topic**: `erza.getmeshinfos`  
**Fichier**: `MeshInfoMessage.cs`

Récupère les informations globales du mesh (système).

**Usage**: Obtenir la configuration système (langues, timezones, Kubernetes)  
**Réponse**:
- `SupportedLanguages`: Langues supportées
- `AvailableTimeZones`: Fuseaux horaires disponibles
- `KubernetesNamespace`: Namespace K8s
- `KubernetesServer`: Serveur K8s

---

### MeshScenarioMessage
**Topics**: 
- `system.mesh.globalscenario.changed` (notification)
- `system.mesh.globalscenario.set` (commande)

**Fichier**: `MeshScenarioMessage.cs`

Gère le scénario global du mesh (ex: Nuit, Jour, Vacances).

**Usage**: Définir ou être notifié du scénario global  
**Propriétés**:
- `Scenario`: Nom du scénario actif

---

### MeshStatusChangeMessage
**Topic**: `system.global.status.change`  
**Fichier**: `MeshStatusChangeMessage.cs`

Notifie un changement de statut global du système.

**Usage**: Monitoring des changements d'état système  
**Propriétés**:
- `MeshId`: ID du mesh
- `StatusKind`: Type de statut
- `NewStatus`: Nouveau statut

---

### MeshExtensionOperationMessage
**Topics**: 
- `system.extensions.create`
- `system.extensions.restart`
- `system.extensions.terminate`

**Fichier**: `MeshExtensionOperationMessage.cs`

Gère les opérations sur les extensions du mesh.

**Usage**: Créer, redémarrer ou terminer des extensions  
**Propriétés**:
- `ExtensionId`: ID de l'extension

---

### AgentStatusUpdateMessage
**Topic**: `system.agents.statusupdate.{agentName}`  
**Fichier**: `AgentStatusUpdateMessage.cs`

Notifie un changement de statut d'un agent.

**Usage**: Monitoring du statut des agents  
**Propriétés**:
- `AgentName`: Nom de l'agent
- `NewStatus`: Nouveau statut (enum AgentStatus)

---

### LogMessage
**Topic**: `system.mesh.log`  
**Fichier**: `LogMessage.cs`

Message de log centralisé.

**Usage**: Enregistrement centralisé des logs système  
**Propriétés**:
- `Data.Source`: Source du log
- `Data.SourceId`: ID de la source
- `Data.Message`: Message de log
- `Data.ImageUrl`: URL d'image optionnelle

---

### SourceCodeChangedMessage
**Topic**: `sources.changed`  
**Fichier**: `SourceCodeChangedMessage.cs`

Notifie un changement dans le code source (webhook Git).

**Usage**: Déclencher un redéploiement après un commit  
**Propriétés**:
- `CommitId`: ID du commit
- `Pusher`: Auteur du push

---

## Messages Domotique

### HomeAutomationMessage
**Topic**: `homeautomation.global`  
**Fichier**: `HomeAutomationMessage.cs`

Message générique pour contrôler les appareils domotiques.

**Usage**: Envoyer des commandes aux appareils (lumières, switches, etc.)  
**Propriétés**:
- `Operations`: Liste d'opérations à effectuer
  - `Role`: Rôle de l'appareil
  - `DeviceName`: Nom de l'appareil
  - `ElementName`: Nom de l'élément à contrôler
  - `Value`: Valeur à appliquer

**Réponse**: Liste des opérations réussies et échouées

---

### HomeAutomationPlatformCommandMessage
**Topic**: Variable (spécifique à chaque plateforme)  
**Fichier**: `HomeAutomationPlatformCommandMessage.cs`

Commande directe spécifique à une plateforme (Zigbee2MQTT, Shelly, etc.).

**Usage**: Envoyer des commandes natives à une plateforme  
**Propriétés**:
- `Operations`: Liste de commandes directes
  - `DeviceName`: Nom de l'appareil
  - `Command`: Commande à exécuter
  - `Data`: Données de la commande

---

### DeviceStateChangedMessage
**Topic**: `homeautomation.devices.state.changed`  
**Fichier**: `DeviceStateChangedMessage.cs`

Notifie un changement d'état d'un appareil.

**Usage**: Être informé des changements d'état des appareils  
**Propriétés**:
- `DevicePlatform`: Plateforme de l'appareil
- `DeviceId`: ID de l'appareil
- `DeviceRole`: Rôle de l'appareil
- `ChangedValues`: Liste des valeurs modifiées (Name, Value)

---

### DeviceDiscoveredMessage
**Topic**: Variable (selon la plateforme)  
**Fichier**: `DeviceDiscoveredMessage.cs`

Notifie la découverte d'un nouvel appareil.

**Usage**: Automatiser l'ajout de nouveaux appareils  
**Propriétés**:
- `Device`: Informations de l'appareil découvert
- `DiscoveryTime`: Date/heure de découverte

---

### SensorValueChangedMessage
**Topics**:
- `home.measures.temperature`
- `home.measures.humidity`
- `home.measures.pressure`
- `home.measures.occupancy`

**Fichier**: `SensorValueChangedMessage.cs`

Notifie un changement de valeur de capteur.

**Usage**: Recevoir les mesures des capteurs  
**Propriétés**:
- `ItemType`: Type d'item (device, room, level, mesh)
- `ItemId`: ID de l'item
- `Measure`: Valeur mesurée

---

### ExecuteScenarioHomeAutomationMessage
**Topics**:
- `homeautomation.scenario.execute`
- `homeautomation.scenario.disable`

**Fichier**: `ScenarioHomeAutomationMessage.cs`

Exécute ou désactive une scène domotique.

**Usage**: Activer/désactiver des scènes (ex: "Soirée cinéma")  
**Propriétés**:
- `SceneId`: ID de la scène

---

### ScenarioContentChangedMessage
**Topic**: `homeautomation.scenario.contentupdated`  
**Fichier**: `ScenarioContentChangedMessage.cs`

Notifie qu'une scène a été modifiée.

**Usage**: Recharger les scènes après modification  
**Propriétés**:
- `SceneId`: ID de la scène
- `SceneGroupId`: ID du groupe de scènes

---

## Messages Utilisateur

### UserChangeMessage
**Topics**:
- `users.accounts.create` / `users.accounts.create.guest`
- `users.accounts.update` / `users.accounts.update.guest`
- `users.accounts.delete` / `users.accounts.delete.guest`

**Fichier**: `UserChangeMessage.cs`

Notifie un changement sur un compte utilisateur.

**Usage**: Synchroniser les données utilisateur  
**Propriétés**:
- `UserId`: ID de l'utilisateur

---

### PresenceChangedMessage
**Topic**: `users.presence.changed`  
**Fichier**: `PresenceChangedMessage.cs`

Notifie un changement de présence d'un utilisateur.

**Usage**: Automatisations basées sur la présence (géolocalisation, détection)  
**Propriétés**:
- `Data.UserId`: ID de l'utilisateur
- `Data.Presence`: Données de présence (lieu, statut)

---

### PresenceNotificationMessage
**Topic**: `users.presence.activity`  
**Fichier**: `PresenceNotificationMessage.cs`

Notifie une activité de présence.

**Usage**: Notifications d'arrivée/départ  
**Propriétés**:
- `Data`: Données de notification de présence

---

### ExternalTokenMessages
**Topics**:
- `users.security.extenaltokens.new`
- `users.security.extenaltokens.deleted`

**Fichier**: `ExternalTokenMessages.cs`

Gère les tokens d'authentification externe (OAuth, etc.).

**Usage**: Synchroniser les tokens d'intégrations tierces  
**Propriétés**:
- `UserId`: ID de l'utilisateur
- `TokenType`: Type de token

---

## Messages PIM

### PimItemUpdateMessage
**Topics**:
- `pim.item.new`
- `pim.item.update`
- `pim.item.done`
- `pim.scheduler.update.wakeuptime`

**Fichier**: `PimItemUpdateMessage.cs`

Gère les mises à jour d'items PIM (todos, info items, scheduler).

**Usage**: Synchroniser calendrier, todos, informations  
**Propriétés**:
- `ItemKind`: Type d'item (InformationItem, Scheduler)
- `ItemId`: ID de l'item
- `ItemTitle`: Titre de l'item

---

### ChatActivityMessage
**Topic**: `pim.chat.activity`  
**Fichier**: `ChatActivityMessage.cs`

Notifie une activité de chat.

**Usage**: Notifications de messages, synchronisation chat  
**Propriétés**:
- `ChannelId`: ID du canal
- `FromUserId`: ID de l'émetteur
- `Content`: Contenu du message

---

### JournalPageChangedMessage
**Topic**: `journal.page.updated`  
**Fichier**: `JournalPageChangedMessage.cs`

Notifie qu'une page de journal a été modifiée.

**Usage**: Synchroniser le journal personnel  
**Propriétés**:
- `PageId`: ID de la page
- `SectionId`: ID de la section

---

## Messages Réseau

### NetworkStatusChangeMessage
**Topic**: `system.network.status.change`  
**Fichier**: `NetworkStatusChangeMessage.cs`

Notifie un changement de statut réseau.

**Usage**: Monitoring de connectivité  
**Propriétés**:
- `NetworkId`: ID du réseau
- `NewStatus`: Nouveau statut de connexion

---

### NetworkSsidDetectedMessage
**Topic**: `system.network.wifi.ssid-detected`  
**Fichier**: `NetworkSsidDetected.cs`

Notifie la détection d'un SSID WiFi.

**Usage**: Automatisations basées sur la localisation WiFi  
**Propriétés**:
- `SsidName`: Nom du SSID détecté

---

### NetworkDeviceEnumerateMessage
**Topic**: `system.network.devices.enumerate`  
**Fichier**: `NetworkDeviceEnumerateMessage.cs`

Récupère la liste des appareils réseau.

**Usage**: Découverte et monitoring des appareils réseau  
**Réponse**:
- `Network`: Réseau scanné
- `Agent`: Agent ayant effectué le scan
- `ActiveDevices`: Appareils actifs
- `InactiveDevices`: Appareils inactifs

---

## Messages Intégration

### IntegrationListChangedMessage
**Topic**: `system.integration.list.changed`  
**Fichier**: `IntegrationListChangedMessage.cs`

Notifie un changement dans la liste des intégrations.

**Usage**: Recharger la liste des intégrations disponibles

---

### IntegrationInstancesListChangedMessage
**Topic**: `system.integration.instances.changed`  
**Fichier**: `IntegrationListChangedMessage.cs`

Notifie un changement dans les instances d'intégrations.

**Usage**: Recharger les instances configurées

---

### IntegrationConfigurationMessage
**Topic**: `{agent}.integration.configure`  
**Fichier**: `IntegrationConfigurationMessage.cs`

Configure une intégration.

**Usage**: Wizard de configuration d'intégrations  
**Propriétés**:
- `Agent`: Nom de l'agent
- `Integration`: Définition de l'intégration
- `Instance`: Instance à configurer
- `SetupValues`: Valeurs de configuration

**Réponse**:
- `Instance`: Instance configurée
- `ConfigurationCard`: Carte UI de configuration
- `IsFinalStep`: Si c'est l'étape finale

---

## Messages Communication

### SendMobileNotificationMessage
**Topic**: `communication.notification.mobile.send`  
**Fichier**: `SendMobileNotificationMessage.cs`

Envoie une notification mobile.

**Usage**: Envoyer des notifications push aux applications mobiles  
**Propriétés**:
- `User`: Utilisateur destinataire
- `Title`: Titre de la notification
- `Content`: Contenu de la notification

**Canaux disponibles**:
- `ApplicationNotifs`: Notifications générales
- `AlertsNotifs`: Alertes importantes
- `PersonalNotifs`: Notifications personnelles
- `DownloadsNotifs`: Notifications de téléchargement

---

### GreetingsMessage
**Topic**: `greetings.get.simple`  
**Fichier**: `GreetingsMessage.cs`

Récupère un message de salutation personnalisé.

**Usage**: Afficher des salutations matinales avec résumé de la journée  
**Propriétés**:
- `Users`: Utilisateurs concernés
- `Destination`: Destination (UserApp, Screen, Speakers)

**Réponse**: Liste d'items de contenu (header, main, date)

---

### VoiceIntentToParseMessage
**Topic**: `voice.intent.parsing`  
**Fichier**: `VoiceIntentToParseMessage.cs`

Parse une intention vocale.

**Usage**: Traiter les commandes vocales  
**Propriétés**:
- `Data.Topic`: Topic de l'intention
- `Data.Tokens`: Tokens reconnus
- `Data.Confidence`: Niveau de confiance
- `Data.Datas`: Données additionnelles

---

### HomeStatusItemsMessage
**Topic**: `communication.homestatus.items.get`  
**Fichier**: `HomeStatusItemsMessage.cs`

Récupère les items de statut de la maison.

**Usage**: Dashboard d'état de la maison  
**Propriétés**:
- `UserId`: ID de l'utilisateur

**Réponse**: Liste d'items de statut (date, type, message)

---

## Messages Monitoring

### MonitoringServiceStateChange
**Topic**: `monitoring.services.statechanged`  
**Fichier**: `MonitoringServiceStateChange.cs`

Notifie un changement d'état d'un service monitoré.

**Usage**: Alertes et monitoring de services  
**Propriétés**:
- `ServiceName`: Nom du service
- `NewStatus`: Nouveau statut (Started, Failed, Stopped)
- `ChangeDate`: Date du changement

---

### WeatherChangeMessage
**Topic**: `system.mesh.weather.updated`  
**Fichier**: `WeatherChangeMessage.cs`

Notifie une mise à jour météo.

**Usage**: Automatisations basées sur la météo  
**Propriétés**:
- `CurrentWeather`: Météo actuelle
- `Forecast`: Prévisions

---

### DownloadItemMessage
**Topic**: `services.downloads.queuefile`  
**Fichier**: `DownloadItemMessage.cs`

Ajoute un fichier à la file de téléchargement.

**Usage**: Gérer les téléchargements  
**Propriétés**:
- `SourceUrl`: URL source
- `DownloadId`: ID du téléchargement

**Messages associés**:
- `services.downloads.started`
- `services.downloads.paused`
- `services.downloads.cancelled`

---

## Messages Génériques

### ItemChangeMessage
**Topic**: Variable  
**Fichier**: `ItemChangeGenericMessage.cs`

Message générique de changement d'item.

**Usage**: Base pour notifications de changement  
**Propriétés**:
- `ItemId`: ID de l'item
- `Kind`: Type de changement (Update, Delete)

---

### AgentGenericMessage
**Topic**: Variable  
**Fichier**: `AgentGenericMessage.cs`

Message générique entre agents.

**Usage**: Communication inter-agents personnalisée  
**Propriétés**:
- `MessageContent`: Contenu du message

---

## Conventions de Nommage des Topics

Le système utilise une convention hiérarchique pour les topics NATS:

- `system.*`: Messages système globaux
- `homeautomation.*`: Messages domotique
- `users.*`: Messages utilisateur
- `pim.*`: Messages Personal Information Management
- `communication.*`: Messages de communication
- `monitoring.*`: Messages de monitoring
- `{agent}.*`: Messages spécifiques à un agent (ex: `gaia.*`, `erza.*`)

## Flux de Messages Typiques

### Scénario: Allumer une lumière
1. Client → `homeautomation.global` (HomeAutomationMessage)
2. Agent Sarah traite la commande
3. Agent Sarah → `homeautomation.devices.state.changed` (DeviceStateChangedMessage)
4. Tous les abonnés sont notifiés

### Scénario: Arrivée à la maison
1. Détection géolocalisation → `users.presence.changed` (PresenceChangedMessage)
2. Agent Erza met à jour le scénario → `system.mesh.globalscenario.changed` (MeshScenarioMessage)
3. Triggers déclenchés → `homeautomation.scenario.execute` (ExecuteScenarioHomeAutomationMessage)
4. Scène activée → Multiples `homeautomation.global` ou commandes directes

### Scénario: Notification mobile
1. Event système → Agent Aurore crée notification
2. Agent Aurore → `communication.notification.mobile.send` (SendMobileNotificationMessage)
3. Agent Freeia envoie aux appareils mobiles

---

**Version**: 1.0  
**Dernière mise à jour**: 2024  
**Projet**: Home Automation System
