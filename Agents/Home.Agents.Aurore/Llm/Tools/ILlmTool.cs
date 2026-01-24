using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Aurore.Llm.Tools
{
    /// <summary>
    /// Interface pour un outil (tool) que le LLM peut appeler
    /// </summary>
    public interface ILlmTool
    {
        /// <summary>
        /// Nom de l'outil (utilisé par le LLM pour l'identifier)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description de l'outil pour le LLM
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Schéma JSON des paramètres attendus
        /// </summary>
        Dictionary<string, object> ParametersSchema { get; }

        /// <summary>
        /// Exécute l'outil avec les paramètres fournis
        /// </summary>
        /// <param name="parameters">Paramètres de l'appel (JSON désérialisé)</param>
        /// <param name="cancellationToken">Token d'annulation</param>
        /// <returns>Résultat de l'exécution (sera formaté en JSON pour le LLM)</returns>
        Task<object> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
    }
}
