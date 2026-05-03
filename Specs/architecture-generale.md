# Architecture générale de Manoir

## Vue d'ensemble

**Manoir** est une plateforme smart life complète pour la gestion d'une maison familiale, allant au-delà de la simple domotique pour inclure la gestion de l'agenda, des communications, des services du quotidien (ménage, vacances scolaires, etc.), et de la sécurité.

## Composants principaux

### Home.Graph - Serveur central
- **Rôle** : Orchestration des données et point d'entrée principal
- **Technologies** :
  - API REST pour les interfaces utilisateur
  - Bus de messages NATS pour la communication avec les agents
- **Responsabilités** :
  - Centralisation des données
  - Exposition des APIs pour les UIs
  - Coordination entre les agents

### Agents opérationnels

Manoir est organisé autour de 7 agents spécialisés :

| Agent | Responsabilité | Statut |
|-------|----------------|--------|
| **Gaia** | Gestion du cluster Kubernetes où tourne Manoir | Actif |
| **Freeia** | Gestion de la box Internet et du réseau | Actif |
| **Erza** | Opérations système : sécurité, présence, alertes météo | Actif |
| **Aurore** | Communication et animation visuelle | Actif |
| **Sarah** | Domotique complète | Actif |
| **Erina** | Gestion de la nourriture et des courses | À créer |
| **Clara** | Majordome : rendez-vous, tâches, emails | Actif |

### Communication inter-composants

```
┌─────────────────┐
│  Interfaces UI  │ (Web admin, écrans muraux, mobile)
└────────┬────────┘
         │ API REST
         ▼
┌─────────────────┐
│   Home.Graph    │ (Serveur central, orchestrateur de données)
└────────┬────────┘
         │ NATS
         ▼
┌─────────────────────────────────────────────────┐
│  Agents : Gaia, Freeia, Erza, Aurore, Sarah,   │
│           Erina, Clara                          │
└─────────────────────────────────────────────────┘
```

- **UI ↔ Home.Graph** : API REST
- **Home.Graph ↔ Agents** : Bus de messages NATS
- **Agents ↔ Agents** : Messages via NATS

## Concept de Mesh

Un **Mesh** représente un lieu de vie (une maison, un appartement).

- Configuration principale : un Mesh unique pour la famille
- Extension possible : informations TRÈS RÉDUITES de Mesh connectés pour plusieurs lieux de vie
- Chaque Mesh dispose de sa propre instance de Manoir

## Utilisateurs et permissions

Manoir gère 4 niveaux d'utilisateurs :

| Niveau | Description | Cas d'usage |
|--------|-------------|-------------|
| **Admin** | Accès complet, configuration système | Chef de famille, configuration initiale |
| **Utilisateur normal** | Accès aux fonctionnalités quotidiennes | Membres adultes de la famille |
| **Enfant** | Accès restreint, contrôle parental | Enfants de la famille |
| **Invité** | Accès temporaire et limité | Visiteurs, baby-sitter |

## Interfaces utilisateur

### Interface d'administration
- Application web dans ce projet (Home.Graph.Public)
- Accès complet pour configuration et monitoring

### Interfaces ergonomiques
- Projets séparés pour interfaces spécialisées
- Écrans muraux (affichage d'informations, contrôle rapide)
- Applications mobiles (à venir)
- Interfaces vocales (via Aurore)

## Stack technique

- **Backend** : .NET (C#)
- **Orchestration** : Kubernetes
- **Bus de messages** : NATS
- **Base de données** : [À préciser]
- **Frontend** : [À préciser]
- **Déploiement** : Docker, manifests K8s

## Prochaines étapes de spécification

- Domaine Domotique (Sarah)
- Domaine Communication (Aurore)
- Domaine Majordome (Clara)
- Domaine Sécurité & Présence (Erza)
- Domaine Réseau (Freeia)
- Domaine Cluster (Gaia)
- Domaine Alimentation (Erina)
