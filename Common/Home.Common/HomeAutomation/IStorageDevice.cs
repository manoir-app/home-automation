using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Home.Common.HomeAutomation
{

    public class StorageDeviceConfig
    {
        public StorageDeviceConfig()
        {
            UserMappings = new List<StorageDeviceUserMapping>();
        }

        public List<StorageDeviceUserMapping> UserMappings { get; set; }

        public string LocalRootPath { get; set; }
    }

    public class StorageDeviceUserMapping
    {
        public string UserId { get; set; }
        public string LocalUser { get; set; }
        public string LocalUserSecurityString { get; set; }
    }

    public class StorageDeviceFolder
    {
        public StorageDeviceFolder()
        {
            SubFolders = new List<string>();
        }

        public StorageDeviceFolder(string fullPath) : this()
        {
            this.FullPath = fullPath;
            this.Name = Path.GetFileName(fullPath);
        }

        public string Name { get; set; }
        public string FullPath { get; set; }
        public List<string> SubFolders { get; set; }
    }

    public class StorageDeviceFile
    {
        public string Name { get; set; }
        public string FullPath { get; set; }

        public long Size { get; set; }

        public StorageDeviceFileMetadata Metadata { get; set; }
    }

    public enum StorageDeviceFileType
    {
        Video,
        Audio,
    }

    public class StorageDeviceFileMetadata
    {
        public StorageDeviceFileType FileType { get; set; }
        public string Title { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Studio { get; set; }
        public string[] Actors { get; set; }
        public string[] Directors { get; set; }

        public string Serie { get; set; }
    }

    public enum StorageDeviceFileOperationKind
    {
        CopyToStorage        
    }

    public enum StorageDeviceFileOperationStatus
    {
        New,
        InProgress,
        Completed,
        Failed
    }


    public class StorageDeviceFileOperation
    {
        public StorageDeviceFileOperationKind Kind { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public StorageDeviceFileOperationStatus Status { get; set; }
        public int ProgressPercent { get; set; }
        public string StatusMessage { get; set; }
    }

    public interface IStorageDevice
    {
        /// <summary>
        ///  Obtient la liste des opérations de transferts actives
        /// </summary>
        /// <returns>La liste des opérations (y compris celles qui viennent
        /// de se terminer ou de planter)</returns>
        IEnumerable<StorageDeviceFileOperation> GetCurrentOperations();
        
        /// <summary>
        /// Retire de la liste des opérations toutes les opé "finies"
        /// </summary>
        /// <param name="keepFailed"><c>false</c> pour supprimmer aussi les opé qui
        /// ont planté</param>
        /// <returns></returns>
        IEnumerable<StorageDeviceFileOperation> ClearCompletedOperations(bool keepFailed);

        /// <summary>
        /// Ajoute une tâche de transfert
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        StorageDeviceFileOperation AddOperation(StorageDeviceFileOperationKind kind, string source, string destination);

        /// <summary>
        /// Obtient un dossier
        /// </summary>
        /// <param name="fullpath"></param>
        /// <returns></returns>
        StorageDeviceFolder GetFolder(string fullpath);

        /// <summary>
        /// Obtient le contenu d'un dossier
        /// </summary>
        /// <param name="fullpath"></param>
        /// <returns></returns>
        StorageDeviceFile[] GetFilesInFolder(string fullpath);

    }
}
