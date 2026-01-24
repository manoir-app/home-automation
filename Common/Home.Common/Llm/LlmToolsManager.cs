using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Common.Llm
{
    /// <summary>
    /// Gestionnaire des outils (tools) disponibles pour le LLM
    /// </summary>
    public class LlmToolsManager
    {
        private readonly Dictionary<string, ILlmTool> _tools;

        public LlmToolsManager()
        {
            _tools = new Dictionary<string, ILlmTool>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Enregistre un nouvel outil
        /// </summary>
        public void RegisterTool(ILlmTool tool)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));

            if (string.IsNullOrWhiteSpace(tool.Name))
                throw new ArgumentException("Le nom de l'outil ne peut pas être vide", nameof(tool));

            _tools[tool.Name] = tool;
        }

        /// <summary>
        /// Récupère un outil par son nom
        /// </summary>
        public ILlmTool GetTool(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            _tools.TryGetValue(name, out var tool);
            return tool;
        }

        /// <summary>
        /// Liste tous les outils enregistrés
        /// </summary>
        public IEnumerable<ILlmTool> GetAllTools()
        {
            return _tools.Values;
        }

        /// <summary>
        /// Génère une description textuelle des outils pour le prompt système
        /// </summary>
        public string GetToolsDescriptionForPrompt()
        {
            if (_tools.Count == 0)
                return "Aucun outil disponible.";

            var sb = new StringBuilder();
            sb.AppendLine("Tu peux utiliser les outils suivants en utilisant le format [TOOL:nom_outil] ou [TOOL:nom_outil|{params_json}] :");
            sb.AppendLine();

            foreach (var tool in _tools.Values.OrderBy(t => t.Name))
            {
                sb.AppendLine($"- **{tool.Name}** : {tool.Description}");
                
                if (tool.ParametersSchema != null && tool.ParametersSchema.Count > 0)
                {
                    var schemaJson = JsonConvert.SerializeObject(tool.ParametersSchema, Formatting.Indented);
                    sb.AppendLine($"  Schéma des paramètres : {schemaJson}");
                }
                
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Exécute un outil avec les paramètres fournis
        /// </summary>
        /// <param name="toolName">Nom de l'outil</param>
        /// <param name="parameters">Paramètres (dictionnaire ou null)</param>
        /// <param name="cancellationToken">Token d'annulation</param>
        /// <returns>Résultat de l'exécution au format JSON</returns>
        public async Task<string> ExecuteToolAsync(
            string toolName, 
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            var tool = GetTool(toolName);
            if (tool == null)
                throw new InvalidOperationException($"Outil '{toolName}' introuvable");

            try
            {
                var result = await tool.ExecuteAsync(parameters ?? new Dictionary<string, object>(), cancellationToken);
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Exécute un outil avec des paramètres JSON
        /// </summary>
        /// <param name="toolName">Nom de l'outil</param>
        /// <param name="parametersJson">Paramètres au format JSON (ou null/vide)</param>
        /// <param name="cancellationToken">Token d'annulation</param>
        /// <returns>Résultat de l'exécution au format JSON</returns>
        public async Task<string> ExecuteToolAsync(
            string toolName,
            string parametersJson,
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, object> parameters = null;

            if (!string.IsNullOrWhiteSpace(parametersJson))
            {
                try
                {
                    parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(parametersJson);
                }
                catch (JsonException ex)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        error = $"Paramètres JSON invalides : {ex.Message}"
                    });
                }
            }

            return await ExecuteToolAsync(toolName, parameters, cancellationToken);
        }
    }
}
