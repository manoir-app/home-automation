using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Common.Llm
{
    /// <summary>
    /// Interface pour un outil (tool) utilisable par le LLM
    /// </summary>
    public interface ILlmTool
    {
        /// <summary>
        /// Nom unique de l'outil
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
        /// <param name="parameters">Paramètres extraits de la demande du LLM</param>
        /// <param name="cancellationToken">Token d'annulation</param>
        /// <returns>Résultat de l'exécution (sera converti en JSON)</returns>
        Task<object> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
    }
}
