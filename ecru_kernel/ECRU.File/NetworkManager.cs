using System;
using Microsoft.SPOT;
using ECRU.Utilities.HelpFunction;
using System.IO;
using ECRU.File.Files; 

namespace ECRU.File
{
    class NetworkManager
    {
        CordinatorRole cordinator;
        byte[] cordinatorAddrs;
        bool isOnline { get; set; }

        string NetPath;
        const string InfoFolderName = "Info";
        LocalManager InfoManager;

        const string FileFolderName = "Files";

        public NetworkManager(string path)
        {
            isOnline = false;
            NetPath = path;
            DirectoryInfo info = new DirectoryInfo(NetPath + "\\" + InfoFolderName);
            DirectoryInfo files = new DirectoryInfo(NetPath + "\\" + FileFolderName);
            if(!info.Exists)
            {
                info.Create(); 
            }
            if (!files.Exists)
            {
                files.Create();
            }
            InfoManager = new LocalManager(NetPath + "\\" + InfoFolderName);

            if(!InfoManager.FileExists("Master.info"))
            {
                FileBase a = InfoManager.CreateFile("Master.info");
                InfoFile masterInfo = new InfoFile(a);
                masterInfo.Version = 0;
                masterInfo.Hash = 0;
                masterInfo.Close(); 
            }
        }

        public void StartNetShare()
        {
            if (ECRU.Utilities.SystemInfo.SystemMAC != null)
            {
                // Get device with lowest Mac
                cordinatorAddrs = ECRU.Utilities.SystemInfo.ConnectionOverview.GetSortedMasters()[0].FromHex();
                if (cordinatorAddrs.ByteArrayCompare(ECRU.Utilities.SystemInfo.SystemMAC))
                {
                    //I am cordinator 
                    cordinator = new CordinatorRole();
                    isOnline = true;
                }
            }
        }

        public void StopNetShare()
        {
            isOnline = false; 
            cordinator = null; 
        }
        

        //–--------------events ------------------―
        
        private void netStateChanged(object msg)
        {
        
        
        }

        private void connectionEstablished(object msg)
        { 
        
        }

        private void ConnectionRq(object msg)
        {

        }

    }
}
