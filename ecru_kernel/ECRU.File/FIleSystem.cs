using System;
using Microsoft.SPOT;
using System.IO;
using ECRU.Utilities.HelpFunction;
using ECRU.File.Files;

namespace ECRU.File
{
    public static class FileSystem
    {
        const string LocalPath = @"\SD\Local";
        const string NetworkPath = @"\SD\Shared";

        static NetworkManager networkManager;
        static LocalManager localManager; 

        static FileSystem()
        {
            DirectoryInfo rootDirectory = new DirectoryInfo(@"\SD\");
            if (rootDirectory.Exists)
            {
                DirectoryInfo local = new DirectoryInfo(LocalPath);
                DirectoryInfo network = new DirectoryInfo(NetworkPath);
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
                networkManager.StartNetShare();
            }
            else
            {
                //no SD
            }
        }



        public static FileBase GetFile(string path, FileAccess access, FileType type)
        {
            if(type == FileType.Local)
            {
                if(access == FileAccess.Read)
                {
                    return localManager.GetReadOnlyFile(path); 
                }
                else
                {
                    return localManager.GetFile(path); 
                }
            }
            else
            {

            }

            return null; 
        }

        public static FileBase CreateFile(string path, FileType type)
        {
            if (type == FileType.Local)
            {
                return localManager.CreateFile(path); 
            }
            else
            {

            }


            return null;
        }

        public static bool Exists(string path, FileType type)
        {
            if(type == FileType.Local)
            {
                return localManager.FileExists(path);
            }

            return false;
        }

        public static void DeleteFile(string path, FileType type)
        {
            if (type == FileType.Local)
            {
                localManager.DeleteFile(path);
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
