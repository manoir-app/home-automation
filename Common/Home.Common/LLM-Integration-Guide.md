# 📋 Intégration LLM dans HomeGraph - Guide Complet

## 🎯 Vue d'ensemble

Ce document résume l'infrastructure LLM créée et guide l'intégration côté serveur API pour permettre aux utilisateurs de discuter avec le système domotique.

---

## 🏗️ Infrastructure Créée

### 📦 1. Composants de Base (`Home.Common`)

#### Client LLM - `LlmClient.cs`
- **URL Serveur** : `http://192.168.2.108:8040` (Lemonade Server)
- **API Compatible** : OpenAI
- **Endpoints** :
  - `/api/v1/chat/completions` - Chat completion
  - `/api/v1/models` - Liste des modèles disponibles

**Méthodes principales** :
```csharp
Task<LlmChatCompletionResponse> ChatCompletionAsync(LlmChatCompletionRequest request)
Task<string> ChatAsync(string model, List<LlmChatMessage> messages, double? temperature, int? maxTokens)
Task<List<string>> GetModelsAsync()
```

#### Modèles de Données

**`LlmChatMessage`**
```csharp
{
    string Role,        // "system", "user", "assistant"
    string Content,     // Contenu du message
    string Name         // Nom optionnel de l'émetteur
}
```

**`LlmChatCompletionRequest`**
```csharp
{
    string Model,                    // Ex: "qwen2.5-14b-instruct"
    List<LlmChatMessage> Messages,
    double? Temperature,             // 0.0 - 2.0 (défaut: 0.7)
    int? MaxTokens,
    double? TopP,
    bool? Stream
}
```

**`LlmConversation`** - Pour persistance MongoDB
```csharp
{
    string Id,
    string ConversationType,         // "agent-to-llm" ou "user-to-llm"
    string Initiator,                // Nom agent ou userId
    string Model,
    string Subject,
    List<LlmConversationMessage> Messages,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime ExpiresAt,              // 30j agents, 90j users
    Dictionary<string, object> Metadata
}
```

---

### 🤖 2. Service Aurore (Exemple d'Implémentation Agent)

#### `AuroreLlmService.cs`

**Prompt Système** :
```
Tu es Aurore, l'assistante intelligente du système domotique Manoir.
Ton rôle : interactions chaleureuses, notifications, messages personnalisés,
résumés d'actualités. Ton ton est convivial mais professionnel.
```

**Méthodes Spécialisées** :
- `GeneratePersonalizedMessageAsync(userName, context, tone)`
- `SummarizeNewsAsync(newsItems, maxLength)`
- `GenerateGreetingAsync(userName, timeOfDay, context)`
- `ChatAsync(userMessage, conversationHistory)` - Chat libre
- `ChatWithToolsAsync(userMessage, conversationHistory, maxToolCalls)` - Chat avec tools

---

### 🛠️ 3. Système de Tools

#### Interface `ILlmTool`
```csharp
{
    string Name,                     // Nom du tool
    string Description,              // Description pour le LLM
    Dictionary<string, object> ParametersSchema,  // Schéma JSON
    Task<object> ExecuteAsync(Dictionary<string, object> parameters)
}
```

#### Tools Disponibles (5 outils)

| Tool | Description | Paramètres |
|------|-------------|------------|
| **get_main_users** | Liste utilisateurs principaux | - |
| **get_present_users** | Utilisateurs présents | - |
| **send_notification** | Envoyer notification push | `userId`, `message`, `title?`, `importance?` |
| **get_weather** | Infos météo localisation | - |
| **get_user_todos** | Tâches TODO utilisateur | `userId`, `status?`, `limit?` |

#### `LlmToolsManager.cs`

**Méthodes** :
- `RegisterTool(ILlmTool tool)` - Enregistrer un tool
- `GetTool(string name)` - Récupérer un tool
- `GetAllTools()` - Liste tous les tools
- `GetToolsDescriptionForPrompt()` - Description pour prompt système
- `ExecuteToolAsync(string toolName, string parametersJson)` - Exécuter

**Format d'Appel** :
```
[TOOL:nom_outil]
[TOOL:nom_outil|{params_json}]

Exemples:
[TOOL:get_present_users]
[TOOL:send_notification|{"userId":"john","message":"Bonjour !"}]
```

---

## 🚀 Intégration Serveur API

### 📍 1. Nouveau Controller : `LlmController.cs`

**Route Base** : `/v1.0/llm`

#### Endpoints à Créer

**Chat Simple**
```http
POST /v1.0/llm/chat
Authorization: Bearer {token}

Request:
{
    "message": "Qui est présent à la maison ?",
    "userId": "john"
}

Response:
{
    "response": "Marie et Thomas sont présents.",
    "conversationId": "conv-abc123",
    "toolsExecuted": ["get_present_users"]
}
```

**Chat avec Historique**
```http
POST /v1.0/llm/chat/{conversationId}

Request:
{
    "message": "Envoie-leur une notification"
}
```

**Récupérer Historique**
```http
GET /v1.0/llm/conversations/{conversationId}

Response:
{
    "id": "conv-abc123",
    "messages": [
        { "role": "user", "content": "...", "timestamp": "..." },
        { "role": "assistant", "content": "...", "timestamp": "..." }
    ],
    "createdAt": "...",
    "model": "qwen2.5-14b-instruct"
}
```

**Liste Conversations Utilisateur**
```http
GET /v1.0/llm/conversations?userId={userId}&limit=20

Response:
{
    "conversations": [
        { "id": "...", "subject": "...", "lastMessage": "...", "updatedAt": "..." }
    ],
    "total": 15
}
```

**Supprimer Conversation**
```http
DELETE /v1.0/llm/conversations/{conversationId}
```

**Liste Modèles Disponibles**
```http
GET /v1.0/llm/models

Response:
{
    "models": ["qwen2.5-14b-instruct", "llama3.1", ...]
}
```

---

### 🔧 2. Service Backend : `UserLlmService.cs`

**Localisation** : `Home.Graph.Server/Services/Llm/`

**Rôle** : Gestion des conversations utilisateur → LLM

#### Différences vs `AuroreLlmService`

| Aspect | Agent (Aurore) | User (API) |
|--------|----------------|------------|
| Prompt | Rôle agent spécifique | Assistant domotique général |
| Tools | Read-only principalement | Actions (contrôle appareils) |
| Persistance | Optionnelle (30j) | **Obligatoire** (90j) |
| Permissions | Aucune vérification | **Vérification stricte** |

#### Structure Suggérée

```csharp
public class UserLlmService
{
    private readonly LlmClient _llmClient;
    private readonly UserLlmToolsManager _toolsManager;
    private readonly IMongoCollection<LlmConversation> _conversationsCollection;
    
    // Constructeur
    public UserLlmService(IMongoDatabase mongoDb)
    {
        _llmClient = new LlmClient("http://192.168.2.108:8040");
        _toolsManager = new UserLlmToolsManager();
        _conversationsCollection = mongoDb.GetCollection<LlmConversation>("llm_conversations");
    }
    
    // Méthodes principales
    public async Task<ChatResponse> ChatAsync(
        string userId, 
        string message, 
        string conversationId = null
    )
    
    public async Task<LlmConversation> GetConversationAsync(string conversationId)
    
    public async Task<List<LlmConversation>> GetUserConversationsAsync(
        string userId, 
        int limit = 20
    )
    
    public async Task DeleteConversationAsync(string conversationId)
}
```

#### Prompt Système pour Users

```
Tu es l'assistant intelligent du système domotique Manoir.

Tu peux :
- Répondre aux questions sur l'état de la maison
- Contrôler les appareils connectés (lumières, thermostats, etc.)
- Exécuter des scènes domotiques
- Gérer le calendrier et les tâches
- Envoyer des notifications aux membres du foyer

Outils disponibles :
[liste générée par GetToolsDescriptionForPrompt()]

Ton ton est amical et proactif. 
Tu donnes des réponses claires et actionables.
Langue : Français (sauf demande contraire).
```

---

### 🔨 3. Tools Additionnels pour Utilisateurs

**Localisation** : `Home.Graph.Server/Services/Llm/Tools/`

#### Tools de Contrôle (Actions)

**`ExecuteSceneTool`**
- Exécuter une scène domotique
- Params: `sceneId`
- Vérification: Accès utilisateur à la scène

**`ControlLightTool`**
- Contrôler lumière (on/off/dim)
- Params: `deviceId`, `action`, `brightness?`
- Vérification: Accès utilisateur à l'appareil

**`SetThermostatTool`**
- Contrôler température
- Params: `deviceId`, `temperature`, `mode?`

**`PlayMusicTool`**
- Contrôle média (Sonos/Spotify)
- Params: `deviceId`, `action`, `playlist?`

#### Tools d'Information (Read-only)

**`ListScenesTool`**
- Liste scènes disponibles
- Params: `groupId?`

**`GetDeviceStatusTool`**
- État d'un appareil
- Params: `deviceId`

**`GetCalendarEventsTool`**
- Événements calendrier
- Params: `userId`, `days?` (défaut: 7)

**`CreateTodoTool`**
- Créer tâche
- Params: `userId`, `title`, `dueDate?`, `priority?`

---

### 💾 4. Persistance MongoDB

#### Collection : `llm_conversations`

**Indexes à Créer** :
```javascript
// Index pour recherche par utilisateur + date
db.llm_conversations.createIndex({ 
    "Initiator": 1, 
    "CreatedAt": -1 
})

// Index pour nettoyage automatique (TTL)
db.llm_conversations.createIndex({ 
    "ExpiresAt": 1 
}, { 
    expireAfterSeconds: 0 
})

// Index par type de conversation
db.llm_conversations.createIndex({ 
    "ConversationType": 1 
})
```

**Calcul ExpiresAt** :
```csharp
// Pour agent-to-llm
conversation.ExpiresAt = DateTime.UtcNow.AddDays(30);

// Pour user-to-llm
conversation.ExpiresAt = DateTime.UtcNow.AddDays(90);
```

---

## 🔐 Sécurité & Permissions

### Vérifications Obligatoires

#### 1. Authentification
```csharp
[Authorize(Roles = "User,Admin")]
public class LlmController : ControllerBase
```

#### 2. Isolation des Données
```csharp
// Un user ne peut accéder qu'à SES conversations
var conversation = await _llmService.GetConversationAsync(conversationId);
if (conversation.Initiator != currentUserId && !User.IsInRole("Admin"))
    return Forbid();
```

#### 3. Rate Limiting
```csharp
// Max 50 messages/heure par utilisateur
[RateLimit(MaxRequests = 50, TimeWindowSeconds = 3600)]
public async Task<IActionResult> Chat([FromBody] ChatRequest request)
```

#### 4. Validation Tools
```csharp
// Avant exécution d'un tool de contrôle
if (tool is IControlTool controlTool)
{
    if (!await _permissionService.CanUserControlDevice(userId, deviceId))
        return new { success = false, error = "Permission refusée" };
}
```

#### 5. Sanitization Paramètres
```csharp
// Valider les paramètres avant exécution
var validator = new ToolParameterValidator();
if (!validator.Validate(toolName, parameters, out var errors))
    return new { success = false, errors = errors };
```

---

## 🔄 Exemple de Flow Complet

### Scénario : "Allume les lumières du salon"

```
1. User → API POST /v1.0/llm/chat
   {
       "message": "Allume les lumières du salon",
       "userId": "john"
   }

2. LlmController vérifie authentification
   - User connecté : ✓
   - Rate limit : 15/50 messages → OK

3. UserLlmService.ChatAsync()
   - Récupère ou crée conversation
   - ConversationType: "user-to-llm"
   - Initiator: "john"
   - Model: "qwen2.5-14b-instruct"
   
4. Construction de la requête LLM
   Messages:
   [
       { role: "system", content: "[Prompt système + liste tools]" },
       { role: "user", content: "Allume les lumières du salon" }
   ]

5. LLM répond
   "[TOOL:control_light|{\"room\":\"salon\",\"action\":\"on\"}]"

6. UserLlmService détecte appel tool
   - Parse: toolName="control_light", params={room:"salon", action:"on"}
   
7. Exécution du tool
   a. Vérification permissions
      - User "john" peut contrôler salon ? → OUI
   b. Résolution "salon" → [lamp-1, lamp-2]
   c. Appel API contrôle lumières
   d. Résultat: { success: true, devices: ["lamp-1", "lamp-2"] }

8. Résultat renvoyé au LLM
   Messages:
   [
       ...(historique précédent),
       { role: "assistant", content: "[TOOL:control_light|...]" },
       { role: "system", content: "Résultat: {success:true, devices:[...]}" }
   ]

9. LLM génère réponse finale
   "J'ai allumé les 2 lumières du salon ✨"

10. Sauvegarde conversation MongoDB
    - Ajout message user
    - Ajout appel tool (dans metadata)
    - Ajout réponse assistant
    - UpdatedAt = now

11. Réponse à l'utilisateur
    {
        "response": "J'ai allumé les 2 lumières du salon ✨",
        "conversationId": "conv-123abc",
        "toolsExecuted": ["control_light"],
        "devicesAffected": ["lamp-1", "lamp-2"]
    }
```

---

## ⚙️ Configuration

### Variables d'Environnement

**À ajouter dans `appsettings.json` ou variables d'env** :

```json
{
  "Llm": {
    "ServerUrl": "http://192.168.2.108:8040",
    "DefaultModel": "qwen2.5-14b-instruct",
    "MaxTokens": 2048,
    "Temperature": 0.7,
    "ConversationRetentionDays": {
      "User": 90,
      "Agent": 30
    },
    "RateLimiting": {
      "MaxMessagesPerHour": 50,
      "MaxTokensPerDay": 100000
    }
  }
}
```

---

## 📝 Plan d'Implémentation

### Phase 1 : Chat Simple (MVP)
- [ ] Créer `LlmController.cs`
- [ ] Endpoint POST `/v1.0/llm/chat` (sans tools)
- [ ] `UserLlmService.cs` basique
- [ ] Test avec prompt simple

### Phase 2 : Persistance
- [ ] Collection MongoDB `llm_conversations`
- [ ] Index TTL + recherche
- [ ] Sauvegarde conversations
- [ ] GET `/conversations/{id}`
- [ ] GET `/conversations?userId=...`
- [ ] DELETE `/conversations/{id}`

### Phase 3 : Système de Tools
- [ ] `UserLlmToolsManager.cs`
- [ ] 3-4 tools de base (get_present_users, send_notification, etc.)
- [ ] Méthode `ChatWithToolsAsync()`
- [ ] Tests appels tools

### Phase 4 : Sécurité
- [ ] Rate limiting (middleware)
- [ ] Validation permissions tools
- [ ] Sanitization paramètres
- [ ] Audit logs (qui a fait quoi)

### Phase 5 : Tools Avancés
- [ ] `ExecuteSceneTool`
- [ ] `ControlLightTool`
- [ ] `GetCalendarEventsTool`
- [ ] `CreateTodoTool`
- [ ] `PlayMusicTool`

---

## 🔁 Réutilisation Code Existant

### Depuis `AuroreLlmService` ✅

**À copier/adapter** :

1. **`ChatWithToolsAsync()`** - Logique de boucle
```csharp
while (toolCallsCount < maxToolCalls)
{
    var response = await _llmClient.ChatAsync(...);
    
    if (response.Contains("[TOOL:"))
    {
        var toolCall = ExtractToolCall(response);
        var result = await _toolsManager.ExecuteToolAsync(...);
        messages.Add(new LlmChatMessage("system", $"Résultat: {result}"));
        continue;
    }
    
    return response;
}
```

2. **`ExtractToolCall()`** - Parser demandes tools
```csharp
private (string toolName, Dictionary<string, object> parameters)? ExtractToolCall(string response)
{
    // Format: [TOOL:nom_outil|{json_params}]
    // ... (code existant à réutiliser)
}
```

3. **Pattern gestion erreurs**
```csharp
try
{
    var response = await _llmClient.ChatAsync(...);
    return response?.Trim() ?? string.Empty;
}
catch (Exception ex)
{
    LogHelper.LogException("llm", "chat-error", ex);
    return "Je rencontre un problème technique.";
}
```

### Depuis `LlmToolsManager` ✅

**À réutiliser intégralement** :

- `RegisterTool(ILlmTool tool)`
- `GetTool(string name)`
- `ExecuteToolAsync(string toolName, Dictionary<string, object> parameters)`
- `GetToolsDescriptionForPrompt()` → Pour prompt système

**À adapter** :

- Constructor : Enregistrer les tools User au lieu des tools Agent

---

## 📚 Exemples de Code

### Controller Minimal

```csharp
[ApiController]
[Route("v1.0/llm")]
[Authorize(Roles = "User,Admin")]
public class LlmController : ControllerBase
{
    private readonly UserLlmService _llmService;
    
    public LlmController(UserLlmService llmService)
    {
        _llmService = llmService;
    }
    
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var userId = User.Identity.Name;
        var response = await _llmService.ChatAsync(
            userId, 
            request.Message, 
            request.ConversationId
        );
        
        return Ok(response);
    }
    
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(
        [FromQuery] int limit = 20
    )
    {
        var userId = User.Identity.Name;
        var conversations = await _llmService.GetUserConversationsAsync(userId, limit);
        return Ok(conversations);
    }
}
```

### Tool Exemple : ExecuteSceneTool

```csharp
public class ExecuteSceneTool : ILlmTool
{
    public string Name => "execute_scene";
    
    public string Description => 
        "Exécute une scène domotique (ex: 'Cinéma', 'Bonne nuit', etc.)";
    
    public Dictionary<string, object> ParametersSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new Dictionary<string, object>
        {
            ["sceneId"] = new Dictionary<string, object>
            {
                ["type"] = "string",
                ["description"] = "ID de la scène à exécuter"
            }
        },
        ["required"] = new List<string> { "sceneId" }
    };
    
    public async Task<object> ExecuteAsync(
        Dictionary<string, object> parameters, 
        CancellationToken ct = default
    )
    {
        var sceneId = parameters["sceneId"].ToString();
        
        using var client = new MainApiAgentWebClient("llm-user");
        
        // Exécuter la scène
        await Task.Run(() => 
            client.DownloadString($"v1.0/homeautomation/scenes/execute/{sceneId}")
        );
        
        return new { success = true, sceneId = sceneId };
    }
}
```

---

## 🎯 Points Clés à Retenir

1. ✅ **Infrastructure LLM complète** déjà créée dans `Home.Common`
2. ✅ **Système de tools extensible** avec `ILlmTool` et `LlmToolsManager`
3. ⚠️ **Sécurité critique** : Permissions sur tools de contrôle
4. 💾 **Persistance obligatoire** pour conversations user (90j)
5. 🔁 **Réutiliser** le code d'Aurore pour la logique de tools
6. 🎨 **Adapter** le prompt système pour contexte utilisateur
7. 🛠️ **Créer** tools supplémentaires pour contrôle domotique

---

**Document créé le** : $(date)  
**Pour** : Intégration LLM dans Home.Graph.Server  
**Référence** : Implémentation Aurore + Architecture Tools
