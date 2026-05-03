# Spécifications Manoir

**Manoir** est une plateforme complète de "smart life" pour la gestion d'une maison familiale, incluant domotique, agenda, communications, sécurité, et services du quotidien.

## Index des spécifications

### Architecture & Infrastructure
- [Architecture générale](architecture-generale.md) - Vue d'ensemble du système, agents, communication

### Planification & Use Cases
- [Use Cases](use-cases.md) - Scénarios d'utilisation par domaine et transversaux, dépendances, phases

### Domaines fonctionnels
- [Domotique](domotique.md) - Gestion des devices, automatisations, scènes (Sarah)
- [Communication & Notifications](communication.md) - Messages, voix, alertes, animation visuelle (Aurore)
 - [Majordome](majordome.md) - Agenda, tâches, emails, rendez-vous (Clara)
- [Sécurité & Présence](securite-presence.md) - Détection présence, alertes météo, sécurité des personnes (Erza)
- [Réseau & Infrastructure](reseau-infrastructure.md) - Box Internet, monitoring réseau (Freeia)
- [Cluster & Déploiement](cluster-deploiement.md) - Gestion Kubernetes (Gaia)
- [Inventaire](inventaire.md) - Stocks maison (Vesta) : consommables, actifs, localisation
 - [Alimentation](alimentation-erina.md) - Menus, recettes, courses (Erina)

### Transverse
- **Utilisateurs & Permissions** - Profils, rôles, multi-mesh
- **Intégrations externes** - Services SaaS, APIs tierces