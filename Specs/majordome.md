# Domaine Majordome - Clara

Agent responsable: Clara

## Rôle

Clara orchestre le PIM familial (agenda, tâches, routines) et centralise des flux utiles au quotidien (services ménagers, vacances scolaires, infos/veille, téléchargements). Elle synchronise avec des services externes (Microsoft 365/To Do, fournisseurs de services), transforme des signaux (emails structurés, flux RSS/ICS) en éléments exploitables (évènements, tâches) et pilote des scénarios au bon moment.

## Sous-domaines

### Calendrier & Évènements
- Sources internes: évènements issus de règles de récurrence sur `TodoItem` (scheduler), routines (heure de réveil par utilisateur), entité « Soleil » (lever/coucher, statut jour/nuit).
- Sources externes:
  - Vacances scolaires France (data.gouv ICS) via SchoolPlanning, mappées en `TodoItem` de type `EventItem` par zone.
  - Prestataires domicile (ex. Azaé ménage) via scraping Playwright, convertis en évènements (`AutoActivate`, `ScenarioOnStart`/`ScenarioOnEnd`, `AssociatedUsers`).
  - Réservations (voyage/hôtel/voiture/resto) détectées dans les emails via JSON‑LD schema.org → évènements planifiés. (À finaliser)
- Activation automatique: quand l’heure de début est atteinte, l’évènement passe `InProgress` et déclenche `ScenarioOnStart`; à l’échéance (`Duration`), l’évènement se termine et déclenche `ScenarioOnEnd`.
- Fenêtre de synchronisation: focus sur les 4–5 prochaines semaines pour le nettoyage/sync; rafraîchis périodiquement.

### Tâches & Listes (ToDo)
- Modèle: `TodoList` (utilisateur ou mesh), `TodoItem` (types: `TodoItem`, `EventItem`).
- Synchronisation Microsoft To Do (Graph) — bidirectionnelle:
  - Listes: création automatique si absente côté Microsoft, association via `SyncDatas` (`ExternalServiceId = MSTODOS`).
  - Flux sortant (Manoir → MS): création/mise à jour de tâches/évènements locaux vers Microsoft; conservation de l’`ItemId` et de l’`ETag`.
  - Flux entrant (MS → Manoir): ingestion périodique (Graph `deltaLink`) des modifications: titre, description, échéance, statut (done/active), importance, suppression.
  - Conflits: politique « last write wins » horodatée; si conflit critique (statut + contenu), priorité à la clôture (« done ») côté Microsoft, log et marquage pour revue.
  - Suppressions: suppression côté Microsoft ⇒ archivage ou suppression côté Manoir selon `PrivacyLevel`/type; suppression côté Manoir ⇒ suppression côté Microsoft.
  - Champs mappés: `title` ↔ `Label`, `body` ↔ `Description`, `dueDateTime` ↔ `DueDate`, `status` ↔ `Status`, catégories basiques ↔ `Categories` (mapping configurable), liens ↔ `LinkedResources`.
  - Périodicité: sync delta toutes 2–5 min par utilisateur avec backoff; premier run initialisant les listes et l’ancrage delta.
- Métadonnées: `Categories`, `PrivacyLevel`, `AssociatedUsers` (dont `ShouldUpdatePresence`), `AutoActivate`, scénarios liés, `Origin`/`OriginItemData` pour la déduplication et le suivi d’origine.

### Emails
- Source: Microsoft 365 (Microsoft Graph) par utilisateur (token AzureAD).
- Objectif: analyser les emails HTML/JSON‑LD schema.org (Reservation, FlightReservation, LodgingReservation, etc.) et créer automatiquement évènements/tâches correspondants, avec métadonnées (dates, lieux, transport/hôtel, personnes).
- État: collecte opérationnelle; parsing JSON‑LD en cours de finalisation.

### Informations (Veille/News)
- Sources: YouTube, RSS (configurables par utilisateur).
- Classification: affectation automatique à un `InformationItemsBucket` (par utilisateur) avant publication côté Home.Graph.
- Fréquence: collecte périodique, mémorisation du dernier check par source.

### Téléchargements (agrégateur)
- Sources: flux RSS (trackers) → extraction de liens magnet.
- Enrichissement: détection langue, résolution, tags/genres (via `VideoAndDownloadsHelper`), repérage épisodes, scoring simple par genre/langue.
- Publication: envoi vers Home.Graph (`/v1.0/downloads`) pour traitement (ex. téléchargement via Freebox, suivi de progression par topics dédiés).

### Déplacements (Commute)
- Données OpenData: priorité aux jeux de données disponibles (transport.data.gouv.fr) en formats GTFS (statique) + GTFS‑RT (temps réel).
- Modèle: stations/lignes/horaires + événements temps réel (retards/suppressions/changements de quai) mappés sur `CommuteSchedule` et `CommuteLineScheduleEvent`.
- Providers: registre par réseau (ex: IDFM, SNCF, TER/TAN/STAR, etc.) avec configuration par identifiants de stations/lignes; découverte via catalogues OpenData lorsque possible.
- Fréquence: statique (GTFS) rafraîchi quotidien/hebdo selon dataset; temps réel (GTFS‑RT) poll toutes 30–60s avec backoff.
- Notifications (Aurore):
  - Retard ≥ X minutes ou suppression ⇒ notification « haute importance » au(x) concerné(s).
  - Approche départ (T‑min) ⇒ rappel contextuel (écran/voix), option de re‑routing si alternative.
- Intégration agenda: création d’évènements contextuels optionnels (départs critiques) et mise à jour des évènements existants en cas d’impact majeur.
- MVP: implémenter un provider basé sur datasets OpenData disponibles (GTFS + GTFS‑RT) pour un premier réseau local; config simple par station(s) favorite(s) et lignes.
- Topics (NATS, à réserver): `clara.commute.alerts.*` (évènements), `clara.commute.status.*` (état suivi) pour consommation par Aurore.

#### MVP Valenciennes (Transvilles)
- Dataset: Réseau urbain Transvilles (Valenciennes) — ressource ID 82442.
  - Format: GTFS statique (bus, tramway), ~83 lignes, ~1 816 arrêts.
  - Couverture: 23/01/2026 → 31/08/2026 (au moment de la spec).
  - Téléchargement direct: https://www.data.gouv.fr/api/1/datasets/r/15438966-8d3c-4dd9-8905-189379ea4c7d
  - Champs notables: `routes.txt` (noms/couleurs), `trips.txt`/`stop_times.txt`, `shapes.txt`. Fichiers tarifs (`fare_*`) absents.
- Temps réel: intégrer GTFS‑RT si disponible pour Transvilles (TripUpdates/ServiceAlerts/VehiclePositions). À défaut, commencer en « statique pur ».
- Config proposée (Clara):
  - Ajout d’un bloc `Commute` dans `ClaraConfigurationData`:

    Exemple JSON:
    {
      "Commute": {
        "Datasets": [
          {
            "Name": "Transvilles",
            "GtfsUrl": "https://www.data.gouv.fr/api/1/datasets/r/15438966-8d3c-4dd9-8905-189379ea4c7d",
            "RtTripUpdatesUrl": null,
            "RtServiceAlertsUrl": null,
            "RtVehiclePositionsUrl": null,
            "Stations": [
              { "Ids": ["STOP_ID_1", "STOP_ID_2"], "Lines": ["L1", "L2"] }
            ]
          }
        ],
        "PollIntervals": { "StaticHours": 24, "RealtimeSeconds": 60 }
      }
    }

  - Comportement: chargement GTFS à J0 puis refresh toutes 24h; si URLs RT renseignées, sondage 60s avec backoff.
  - Sortie: `clara.commute.alerts.transvilles.*` pour alertes; `clara.commute.status.transvilles` pour état de suivi.

#### Spécification du Provider "Transvilles"
- Nom logique: TransvillesCommuteProvider (spécification — pas d’implémentation ici).
- Entrées & configuration:
  - `GtfsUrl` (ZIP GTFS statique), `RtTripUpdatesUrl`/`RtServiceAlertsUrl`/`RtVehiclePositionsUrl` (optionnels).
  - Stations suivies: liste d’`stop_id` GTFS (et alias possibles) et lignes filtrées (`route_short_name` ou `route_id`).
  - Intervalles: `StaticHours` (refresh GTFS) et `RealtimeSeconds` (poll RT) avec backoff.
- Cycle de vie:
  1) Initialisation: téléchargement GTFS (ETag/Last-Modified si dispo), parsing `stops`, `routes`, `trips`, `stop_times`, `calendar[_dates]`, `shapes` (indexation en mémoire + cache disque tmp).
  2) Planification: calcul des `CommuteSchedule` 24–48h pour chaque station/ligne suivie (next departures/arrivals) en se basant sur `stop_times` et le service en vigueur.
  3) Poll temps réel (si RT dispo):
     - TripUpdates: mise à jour des horaires réels (retard, annulation) et génération d’événements correspondants.
     - ServiceAlerts: création d’alertes réseau/ligne/arrêt (travaux, perturbations) avec périmètre.
     - VehiclePositions: enrichissement facultatif (proximité/ETA).
  4) Publication NATS: status périodique + alertes ponctuelles (voir schémas ci-dessous).
  5) Refresh GTFS: rechargement quotidien/hebdo; si échec, fallback au dernier snapshot OK.
- Mapping GTFS → modèle Manoir:
  - `stops.stop_id` → `CommuteSchedule.StationId` ; `stops.name` → libellé station.
  - `routes.route_short_name/route_id/route_color` → identifiant et style ligne.
  - `trips.trip_id`, `trips.trip_headsign` → `CommuteSchedule.TripId` et destination.
  - `stop_times.departure_time/arrival_time` → `TheoricalSchedule` (choix sens en fonction du suivi départ/arrivée).
  - GTFS‑RT `delay/cancelled` → `RealtimeSchedule`/`RealtimeCancelled` + `CommuteLineScheduleEvent`.
- Détection d'événements & notifications (Aurore):
  - Retard ≥ seuil (configurable) ⇒ alerte « haute importance » ciblée.
  - Suppression course ⇒ alerte immédiate + suggestion d’alternative si possible.
  - Approche départ (T‑min) ⇒ rappel contextuel (voix/écran), option re‑routing en cas de perturbation.
- Topics NATS et payloads (exemples):
  - `clara.commute.status.transvilles` (périodique)
    {
      "provider":"transvilles",
      "lastStaticRefresh":"2026-01-23T14:53:00+01:00",
      "lastRealtimeRefresh":"2026-01-23T15:02:00+01:00",
      "stationsMonitored": 2,
      "linesMonitored": ["L1","L2"],
      "schedulesCached": 1240,
      "rtAvailable": false
    }
  - `clara.commute.alerts.transvilles.retard`
    {
      "provider":"transvilles",
      "routeId":"L1",
      "tripId":"TRIP123",
      "stopId":"STOP_ID_1",
      "planned":"2026-01-24T07:42:00+01:00",
      "realtime":"2026-01-24T07:49:00+01:00",
      "delaySeconds":420,
      "severity":"high",
      "message":"Retard estimé 7 min vers Hotel de Ville"
    }
  - `clara.commute.alerts.transvilles.suppression`
    {
      "provider":"transvilles",
      "routeId":"L2",
      "tripId":"TRIP987",
      "stopId":"STOP_ID_2",
      "planned":"2026-01-24T08:10:00+01:00",
      "cancelled":true,
      "severity":"critical",
      "message":"Course supprimée"
    }
- Interactions Home.Graph:
  - Par défaut: notifications (Aurore) sans création d’évènements. Option: création/MAJ d’évènements dans `TodoItem` quand impact majeur (annulations récurrentes sur trajets critiques).
- Résilience & qualité:
  - GTFS avec références manquantes (cf. rapport validation) ⇒ ignorer les enregistrements invalides, log, continuer.
  - Timezone: utiliser TZ du mesh; convertir heures GTFS (locales) correctement en `DateTimeOffset`.
  - Performances: index par `stop_id`/`route_id` et fenêtres temporelles; limiter le jeu recalculé à l’horizon utile.
  - Confidentialité: notifications ciblées utilisateurs concernés; obfuscation des détails en mode privé mesh.

### Routines & Réveil
- `RoutineDataWithUser.NextWakeUpTime`: recalcul quotidien tenant compte du fuseau horaire (déterminé via coordonnées du mesh).
- Mise à jour via API (`/v1.0/pim/scheduler/wakeuptime/next`). Utilisé pour préparatifs matinaux et synchronisation d’évènements.

### Entité Soleil
- Entité `sun` maintenue dans Home.Graph avec `TodaySunRise/Set`, `NextSunRise/Set` et `CurrentStatus` (jour/nuit) + images.
- Utilisation: affichage (Aurore), conditions de scénarios (Sarah), contextes temporels.

## Intégrations & Interfaces
- Écoute NATS: `clara.>`, progression téléchargements, `NewExternalTokenMessage` (tokens externes).
- APIs Home.Graph utilisées (exemples):
  - `v1.0/todos/*` (évènements, tâches, listes, bulk)
  - `v1.0/entities/all` (entité Soleil)
  - `v1.0/pim/*` (routines, informations/buckets)
  - `v1.0/downloads` (items de téléchargement)
- Scénarios: champs `ScenarioOnStart`/`ScenarioOnEnd` exécutés par Sarah au début/fin d’un évènement.
- Tokens externes: AzureAD par utilisateur pour Graph (Mail/To Do). Gestion via `NewExternalTokenMessage` et stockage côté Home.Graph.

## Confidentialité
- Respect de `PrivacyLevel` au niveau `TodoItem`/évènement; en mode privé (mesh), Aurore limite les détails dans les annonces/affichages.
- Données issues d’emails/réservations: marquage par défaut en « partagé avec utilisateurs » ou « privé » selon l’origine; configurable.

## Configuration (extraits)
- `ClaraConfigurationData` (via Home.Graph):
  - `NewsSources`: liste par utilisateur (type `youtube`/`rss`, bucket cible, `LastChecked`).
  - `TorrentSources`: flux à surveiller, tags, confidentialité.
  - `HomeServices`: dictionnaire de providers (ex. `azae`) avec `ServiceUsername/Password`, `ListId`, scénarios à démarrer/terminer, `ExternalUserIds`.
- Listes ToDo: mapping `SyncDatas` vers Microsoft To Do; création automatique si absent.

## MVP Actuel
- Vacances scolaires FR (ICS) → évènements (par zone), MAJ périodique.
- Prestataire ménage Azaé → évènements auto‑activés avec scénarios start/end.
- Synchronisation Microsoft To Do (création liste, push des tâches locales, suivi `ItemId`).
- Veille (YouTube/RSS) → buckets d’infos utilisateur.
- Gestion `sun` + règles d’activation d’évènements (start/end) + scheduler de récurrence.
- Agrégateur de téléchargements RSS → classification + publication Home.Graph.

## Prochaines étapes
- Emails: finaliser parsing JSON‑LD (réservations) → évènements/tâches + rattachement personnes/lieux.
- Déplacements: brancher un provider GTFS/GTFS‑RT OpenData (réseau disponible localement) + notifications de retards via Aurore.
- Tâches: implémenter la synchro entrante Graph (delta), gestion des suppressions et des conflits pour la bidirectionnalité.
- Intégration Vesta/Erina: création automatique de tâches « à acheter » (seuils/expirations), synchronisées sur listes courses.
- Admin UI: configuration sources (news/torrents/services), mapping catégories/buckets, préférences de confidentialité.
