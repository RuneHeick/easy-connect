using System.IO;

namespace ECRU.Utilities
{
    public static class FileSystem
    {
        private const string LocalPath = @"\SD\Local";
        private const string NetworkPath = @"\SD\Shared";

        private static readonly NetworkManager networkManager;
        private static readonly LocalManager localManager;

        static FileSystem()
        {
            var rootDirectory = new DirectoryInfo(@"\SD\");
            if (rootDirectory.Exists)
            {
                var local = new DirectoryInfo(LocalPath);
                var network = new DirectoryInfo(NetworkPath);
                if (!local.Exists)
                {
                    local.Create();
                }
                if (!network.Exists)
                {
                    network.Create();
                }
                networkManager = new NetworkManager(NetworkPath);
                localManager = new LocalManager(LocalPath);
            }
        }


        public static FileBase GetFile(string path, FileAccess access, FileType type)
        {
            if (type == FileType.Local)
            {
                if (access == FileAccess.Read)
                {
                    return localManager.GetReadOnlyFile(path);
                }
                return localManager.GetFile(path);
            }
            if (access == FileAccess.Read)
            {
                return networkManager.GetReadOnlyFile(path);
            }
            return networkManager.GetFile(path);

            return null;
        }

        public static FileBase CreateFile(string path, FileType type)
        {
            if (type == FileType.Local)
            {
                return localManager.CreateFile(path);
            }
            return networkManager.GetFile(path);


            return null;
        }

        public static bool Exists(string path, FileType type)
        {
            if (type == FileType.Local)
            {
                return localManager.FileExists(path);
            }
            return networkManager.FileExists(path);

            return false;
        }

        public static void DeleteFile(string path, FileType type)
        {
            if (type == FileType.Local)
            {
                localManager.DeleteFile(path);
            }
            else
            {
                networkManager.DeleteFile(path);
            }
        }
    }


    public enum FileAccess
    {
        Read,
        ReadWrite
    }

    public enum FileType
    {
        Local,
        Shared
    }
}