using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Aurore.Llm.Tools
{
    /// <summary>
    /// Gestionnaire des outils (tools) disponibles pour le LLM
    /// </summary>
    public class AuroreLlmToolsManager
    {
        private readonly Dictionary<string, ILlmTool> _tools;

        public AuroreLlmToolsManager()
        {
            _tools = new Dictionary<string, ILlmTool>();
            RegisterDefaultTools();
        }

        /// <summary>
        /// Enregistre les outils par défaut d'Aurore
        /// </summary>
        private void RegisterDefaultTools()
        {
            RegisterTool(new GetMainUsersTool());
            RegisterTool(new GetPresentUsersTool());
            RegisterTool(new SendNotificationTool());
            RegisterTool(new GetWeatherTool());
            RegisterTool(new GetUserTodosTool());
        }

        /// <summary>
        /// Enregistre un nouvel outil
        /// </summary>
        public void RegisterTool(ILlmTool tool)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));

            _tools[tool.Name] = tool;
        }

        /// <summary>
        /// Récupère un outil par son nom
        /// </summary>
        public ILlmTool GetTool(string name)
        {
            return _tools.ContainsKey(name) ? _tools[name] : null;
        }

        /// <summary>
        /// Récupère tous les outils enregistrés
        /// </summary>
        public List<ILlmTool> GetAllTools()
        {
            return _tools.Values.ToList();
        }

        /// <summary>
        /// Génère la description des outils pour le prompt système
        /// </summary>
        public string GetToolsDescriptionForPrompt()
        {
            if (_tools.Count == 0)
                return "Aucun outil disponible.";

            var descriptions = _tools.Values.Select(tool => 
                $"- **{tool.Name}** : {tool.Description}"
            );

            return "Outils disponibles :\n" + string.Join("\n", descriptions);
        }

        /// <summary>
        /// Génère le schéma JSON des outils (format OpenAI function calling)
        /// </summary>
        public List<object> GetToolsSchemaForApi()
        {
            return _tools.Values.Select(tool => new
            {
                type = "function",
                function = new
                {
                    name = tool.Name,
                    description = tool.Description,
                    parameters = tool.ParametersSchema
                }
            }).Cast<object>().ToList();
        }

        /// <summary>
        /// Exécute un outil et retourne son résultat
        /// </summary>
        /// <param name="toolName">Nom de l'outil à exécuter</param>
        /// <param name="parametersJson">Paramètres au format JSON</param>
        /// <param name="cancellationToken">Token d'annulation</param>
        /// <returns>Résultat de l'exécution (sérialisé en JSON)</returns>
        public async Task<string> ExecuteToolAsync(
            string toolName, 
            string parametersJson,
            CancellationToken cancellationToken = default)
        {
            var tool = GetTool(toolName);
            if (tool == null)
                return JsonConvert.SerializeObject(new { error = $"Outil '{toolName}' non trouvé" });

            try
            {
                // Désérialiser les paramètres
                var parameters = string.IsNullOrEmpty(parametersJson) 
                    ? new Dictionary<string, object>() 
                    : JsonConvert.DeserializeObject<Dictionary<string, object>>(parametersJson);

                // Exécuter l'outil
                var result = await tool.ExecuteAsync(parameters, cancellationToken);

                // Retourner le résultat sérialisé
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message, tool = toolName });
            }
        }

        /// <summary>
        /// Exécute un outil avec des paramètres déjà désérialisés
        /// </summary>
        public async Task<object> ExecuteToolAsync(
            string toolName,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            var tool = GetTool(toolName);
            if (tool == null)
                return new { error = $"Outil '{toolName}' non trouvé" };

            try
            {
                return await tool.ExecuteAsync(parameters ?? new Dictionary<string, object>(), cancellationToken);
            }
            catch (Exception ex)
            {
                return new { error = ex.Message, tool = toolName };
            }
        }
    }
}
