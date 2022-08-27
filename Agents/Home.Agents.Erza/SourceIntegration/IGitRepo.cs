using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Erza.SourceIntegration
{
    public class GitFile
    {
        public string Path { get; set; }
        public string ServerSignature { get; set; }
        public byte[] Content { get; set; }
    }

    public class GitFileEntry
    {
        public string Type { get; set; }
        public string Path { get; set; }
        public string ServerSignature { get; set; }
    }


    public interface IGitRepo
    {
        void Init(AutomationMeshSouceCodeIntegration config);
        IEnumerable<GitFileEntry> GetFiles(string path);

        GitFile GetFile(string path);
        void DeleteFile(string path, string sha);
        void UpsertFile(string path, string content, string sha);
    }
}
