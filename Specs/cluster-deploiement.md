# Domaine Cluster & Déploiement - Gaia

**Agent responsable** : Gaia  
**État** : ✅ Implémenté

## Vue d'ensemble

Gaia est l'agent d'orchestration et de gestion du cluster Kubernetes où tourne Manoir. Elle gère :
- Monitoring du cluster K8s et ses ressources
- Déploiement/mise à jour des agents et extensions
- Gestion des certificats SSL/TLS
- Gestion des secrets et configurations
- Gestion des volumes de persistance
- Runners de jobs/batch
- Reverse proxy (FRP) pour exposition externe

## Kubernetes

**État** : ✅ Complètement implémenté

### Client K8s
- **Client** : official `Kubernetes` .NET client library
- **Authentification** : In-cluster config (service account)
- **Namespace** : `default` (par défaut)

### Monitoring du cluster

#### Agents
- **Vérification** : Toutes les 30 secondes
- **Checks** : Pods running, containers ready, restarts count
- **Alertes** : Pod crashlooping, not ready, image pull error

#### Certificats
- **Vérification** : Toutes les 5 minutes
- **Checks** : Validité certificats TLS Ingress, secrets cert
- **Alertes** : Certificat expirant, expiré

## Déploiement d'Extensions

**État** : ✅ Implémenté

Déploiement d'extensions domotiques dans le cluster :

### Opérations
- **Create** : Déployer une nouvelle extension (Deployment K8s)
- **Restart** : Redémarrer une extension (rollout restart)
- **Terminate** : Arrêter une extension (delete Deployment)

### Configuration
- **Image** : Pull depuis registry (tag variable via `IMAGE_TAG` env var)
- **Environnement** : Variables config passées via ConfigMap
- **Secrets** : Credentials via K8s Secrets

## Gestion des Secrets et Configuration

### Secrets
- **Opaque** : Données binaires (configs, credentials)
- **TLS** : Certificats et clés
- **Update** : Modification contenu secret + restart deployment

### ConfigMaps
- **Usage** : Configuration non-sensible des deployments

### Exemple : FRP (Reverse Proxy)
- Secret `frpc-config` contient `frpc.ini`
- Déploiement `frpc` monte le secret
- Topic NATS : `system.reverse-proxy.changed` → update secret → restart

## Gestion des Volumes

**État** : ✅ Implémenté

Persistance des données via PersistentVolumes :

### Types supportés
- **HostPath** : Montage répertoire local (dev/test)
- **NFS** : Partage réseau (production)
- 🔄 Autres types : Bloc storage, cloud volumes

### Capacité et Claims
- **Taille** : Par défaut 20Gi (configurable)
- **Access mode** : `ReadWriteOnce` (généralement)
- **StorageClass** : `manual` ou dynamique

### Exemple : Job volumes
- Créer PersistentVolume `/home-automation/{folder}`
- Créer PersistentVolumeClaim associée
- Monter dans Job Pods

## Jobs et Batch

**État** : ✅ Partiellement implémenté

Exécution de jobs ponctuels (backup, nettoyage, etc.) :

### Runner
- Crée un K8s Job pour chaque exécution
- Volumes montés pour données persistantes
- Timeout et retry policy configurables

### Volumes de job
- Vérification existence volumes avant run
- Création automatique si manquant

## Certificats et TLS

**État** : ✅ Implémenté

Gestion des certificats SSL/TLS pour services :

### Sources
- **Let's Encrypt** : Certificats auto-renouvelés
- **Custom** : Upload certificat/clé manuel

### Operations
- **Change** : Topic NATS `system.certificate.changed` → update secret TLS
- **Validation** : Vérification certificat valide avant apply

### Ingress/Services
- Certificat TLS monté via secret dans Ingress
- Rechargement automatique à changement

## Reverse Proxy (FRP)

**État** : ✅ Implémenté

Exposition du cluster via reverse proxy (Frp) :

### Configuration
- **File** : `frpc.ini` stocké en Secret Kubernetes
- **Topics** : `system.reverse-proxy.changed` → update + restart
- **Deployment** : Pod `frpc` avec connexion au serveur FRP

### Usage
- Accès externe à Manoir derrière routeur
- Tunnel chiffré vers serveur FRP public
- Configuration sans ouvrir ports locaux

## Architecture technique

### Communication NATS
Gaia écoute les topics :
- `gaia.>` - Messages dirigés à Gaia
- `system.extensions.>` - Opérations extensions
- `security.>` - Messages sécurité (certificats)
- `monitoring.>` - Messages monitoring
- `DownloadItemMessage.TopicName` - Demandes téléchargement
- `SystemReverseProxyChangeMessage.TopicName` - Changes FRP
- `SystemCertificateChangeMessage.TopicName` - Changes certificats

### Composants principaux

| Composant | Fichier | Responsabilité |
|-----------|---------|----------------|
| Handler | GaiaMessageHandler.cs | Traitement messages NATS |
| K8s Checker | KubernetesChecker.cs | Monitoring cluster |
| Deployment | DeploymentHelper.cs | Gestion déploiements |
| Job Runner | JobRunnerHelper.cs | Runners de jobs batch |
| MQTT | MqttHandler.cs | Intégration MQTT |
| Download | DownloadHelper.cs | Gestion téléchargements |

## Questions en suspens

### 1. Haute disponibilité
- Multi-master K8s supporté ?
- Backup/restore du cluster ?
- Disaster recovery plan ?

### 2. Monitoring avancé
- Métriques Prometheus ?
- Alertes sur CPU/mémoire saturation ?
- Logging centralisé (ELK, Loki) ?

### 3. Scalabilité
- Auto-scaling des deployments ?
- Limites de ressources (CPU, mémoire) par pod ?

### 4. Security
- Network policies entre pods ?
- Pod security policies ?
- RBAC pour service accounts ?

### 5. Stockage
- Sauvegarde automatique des volumes ?
- Snapshot/restore capability ?

### 6. Jobs
- Gestion des failed jobs (retry, alertes) ?
- Logs des job runs ?
- Cleanup des vieux jobs ?
