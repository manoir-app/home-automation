using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Home.Graph.Common.Integrations.Knowledge
{
    public partial class TriliumNotesClient
    {
        static Dictionary<string, HttpClient> _clients = new Dictionary<string, HttpClient>();

        public static TriliumNotesClient FromToken(ExternalToken token)
        {
            TriliumNotesClient triCli = null;
            if (_clients.TryGetValue(token.Id, out var cli))
                triCli = new TriliumNotesClient(cli);
            else
            {
                cli = new HttpClient();
                cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", $"ETAPI:{token.Token}");
                _clients[token.Id] = cli;
                triCli = new TriliumNotesClient(cli);
            }
            string server = "notes.anzin.carbenay.manoir.app"; // temporaire

            triCli.BaseUrl = $"https://{server}/etapi/";
            return triCli;
        }
    }
}
