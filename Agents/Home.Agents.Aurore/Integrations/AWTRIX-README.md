# Intégration Awtrix 3

Cette intégration permet de communiquer avec des appareils Awtrix 3 pour afficher des notifications et des applications personnalisées.

## Documentation officielle

- API Awtrix 3: https://blueforcer.github.io/awtrix3/
- Bibliothèque d'icônes: https://developer.lametric.com/icons

## Configuration

Pour configurer l'intégration, ajoutez les adresses IP de vos appareils Awtrix dans la configuration :

```
awtrix:devices=192.168.1.100,192.168.1.101
```

## Utilisation de base

### Envoyer un message simple

```csharp
using Home.Agents.Aurore.Integrations;

// Message simple
await AwtrixHelper.BroadcastMessageAsync("Bonjour !");

// Message avec icône et durée
await AwtrixHelper.BroadcastMessageAsync("Il fait 22°C", "52", duration: 10);
```

### Utilisation directe du client

```csharp
using (var awtrix = new AwTrix("192.168.1.100"))
{
    // Notification simple
    await awtrix.SendTextAsync("Hello World!");
    
    // Notification avec icône
    await awtrix.SendTextWithIconAsync("Température: 22°C", "52", duration: 10);
    
    // Notification personnalisée
    var notification = new AwtrixNotification
    {
        Text = "Téléchargement en cours",
        Icon = "1234",
        Progress = 75,
        ProgressColor = "#00FF00",
        Duration = 5
    };
    await awtrix.SendNotificationAsync(notification);
}
```

### Applications personnalisées

Les applications personnalisées restent affichées de manière cyclique avec les autres apps :

```csharp
using (var awtrix = new AwTrix("192.168.1.100"))
{
    // Créer une app météo
    var weatherApp = new AwtrixCustomApp
    {
        Text = "22°C Ensoleillé",
        Icon = "52",
        Duration = 15,
        Repeat = -1 // Répétition infinie
    };
    await awtrix.SetCustomAppAsync("weather", weatherApp);
    
    // Créer une app avec barre de progression
    var downloadApp = new AwtrixCustomApp
    {
        Text = "DL: 75%",
        Icon = "1234",
        Progress = 75,
        ProgressColor = "#00FF00",
        ProgressBackgroundColor = "#333333"
    };
    await awtrix.SetCustomAppAsync("download", downloadApp);
    
    // Supprimer une app
    await awtrix.DeleteCustomAppAsync("download");
}
```

### Fonctions helpers prédéfinies

```csharp
// Notification de bienvenue
await AwtrixHelper.SendWelcomeNotificationAsync("Jean");

// Alerte
await AwtrixHelper.SendAlertAsync("Porte d'entrée ouverte !");

// Notification météo
await AwtrixHelper.SendWeatherNotificationAsync("22°C Ensoleillé", "52");

// Notification de progression
await AwtrixHelper.SendProgressNotificationAsync("Téléchargement", 75, "1234");
```

## Exemples d'intégration dans le système

### 1. Notification lors d'une détection de présence

```csharp
// Dans un message handler
public static MessageResponse HandlePresenceDetected(MessageOrigin origin, string topic, string messageBody)
{
    var user = GetUserFromMessage(messageBody);
    await AwtrixHelper.SendWelcomeNotificationAsync(user.CommonName);
    return MessageResponse.OK;
}
```

### 2. Affichage de la météo

```csharp
// Mise à jour périodique de la météo
public static async Task UpdateWeatherOnAwtrix()
{
    var weather = await GetCurrentWeather();
    string iconId = GetWeatherIconId(weather.Condition);
    string text = $"{weather.Temperature}°C {weather.Description}";
    
    await AwtrixHelper.SendWeatherNotificationAsync(text, iconId);
}
```

### 3. Notification de téléchargement

```csharp
// Lors de la progression d'un téléchargement
public static async Task UpdateDownloadProgress(string fileName, int progress)
{
    await AwtrixHelper.SendProgressNotificationAsync(
        $"DL: {fileName}", 
        progress, 
        "1234"
    );
}
```

### 4. Affichage de TODO

```csharp
// Afficher le nombre de tâches en attente
public static async Task UpdateTodoCount()
{
    var todos = await GetPendingTodos();
    var app = new AwtrixCustomApp
    {
        Text = $"{todos.Count} tâches",
        Icon = "5555",
        Duration = 10,
        Color = todos.Count > 5 ? "#FF0000" : "#00FF00"
    };
    
    using (var awtrix = new AwTrix("192.168.1.100"))
    {
        await awtrix.SetCustomAppAsync("todos", app);
    }
}
```

## Icônes

Vous pouvez utiliser :
- Un ID d'icône de la bibliothèque LaMetric (ex: "52" pour météo)
- Une URL d'image (ex: "http://example.com/icon.png")

Exemples d'IDs d'icônes courantes :
- 52 : Soleil
- 2284 : Nuages
- 1234 : Téléchargement
- 5555 : Todo/Checklist
- 1234 : Home
- 120 : Alerte

## Contrôles avancés

```csharp
using (var awtrix = new AwTrix("192.168.1.100"))
{
    // Éteindre/allumer l'écran
    await awtrix.SetPowerAsync(false); // Éteindre
    await awtrix.SetPowerAsync(true);  // Allumer
    
    // Navigation entre apps
    await awtrix.NextAppAsync();
    await awtrix.PreviousAppAsync();
}
```

## Options de notification avancées

```csharp
var notification = new AwtrixNotification
{
    Text = "Message important",
    Icon = "1234",
    Color = "#FF0000",                    // Couleur du texte
    PushIcon = 2,                         // Décale l'icône de 2 pixels
    Repeat = 3,                           // Répète 3 fois
    Duration = 10,                        // Durée de 10 secondes
    Hold = true,                          // Maintient après changement d'app
    Sound = "alarm",                      // Joue un son
    Progress = 50,                        // Barre de progression à 50%
    ProgressColor = "#00FF00",           // Couleur de la barre
    ProgressBackgroundColor = "#333333", // Couleur de fond
    Rainbow = true,                       // Effet arc-en-ciel sur le texte
    ScrollSpeed = 100                     // Vitesse de défilement en ms
};

await awtrix.SendNotificationAsync(notification);
```

## Gestion des erreurs

Toutes les méthodes retournent `bool` indiquant le succès de l'opération :

```csharp
bool success = await AwtrixHelper.BroadcastMessageAsync("Test");
if (!success)
{
    Console.WriteLine("Échec de l'envoi à Awtrix");
}
```

## Nettoyage

Pensez à nettoyer les clients lors de l'arrêt de l'agent :

```csharp
// Dans le Program.cs, lors de l'arrêt
AwtrixHelper.Cleanup();
```
