using System;
using Microsoft.SPOT;
using ECRU.Utilities.HelpFunction;
using System.IO;
using ECRU.File.Files;
using ECRU.Utilities.EventBus;
using ECRU.Utilities.EventBus.Events;
using System.Net.Sockets;
using System.Collections;

namespace ECRU.File
{
    class NetworkManager : IDisposable
    {
        public const string connectionRQType = "filemanagerRq";
        CordinatorRole cordinator;
        byte[] cordinatorAddrs;
        readonly object ChangeLock = new object();
        readonly object BusyLock = new object(); 
        public bool isOnline { get; set; }

        const string InfoFolderName = "Info";

        LocalManager InfoManager;
        LocalManager NetFileManager;

        const string FileFolderName = "Files";

        public NetworkManager(string path)
        {
            isOnline = false;

            InfoManager = new LocalManager(path + "\\" + InfoFolderName);
            NetFileManager = new LocalManager(path + "\\" + FileFolderName);


            EventBus.Subscribe(typeof(NetworkStatusMessage), netStateChanged);
            EventBus.Subscribe(typeof(ConnectionRequestMessage), connectionEstablishedEvent);
        }



        #region NetStateChanged

        private void StartNetShare()
        {
            if (ECRU.Utilities.SystemInfo.SystemMAC != null)
            {
                // Get device with lowest Mac
                cordinatorAddrs = ECRU.Utilities.SystemInfo.ConnectionOverview.GetSortedMasters()[0].FromHex();
                if (cordinatorAddrs.ByteArrayCompare(ECRU.Utilities.SystemInfo.SystemMAC))
                {
                    //I am cordinator 
                    cordinator = new CordinatorRole(InfoManager,NetFileManager);
                    isOnline = true;
                }
            }
        }

        private void StopNetShare()
        {
            isOnline = false;
            MutexClear(); 
            if (cordinator != null)
            {
                cordinator.Dispose();
                cordinator = null;
            }
        }

        private void netStateChanged(object msg)
        {
            lock (ChangeLock)
            {
                NetworkStatusMessage changed = msg as NetworkStatusMessage;
                if (changed != null)
                {
                    if (isOnline == true && changed.isinsync == false)
                    {
                        StopNetShare();

                    }
                    if (isOnline == true && changed.isinsync == true)
                    {
                        StopNetShare();
                        StartNetShare();
                    }
                    if (changed.isinsync == true && isOnline == false)
                    {
                        StartNetShare();
                    }
                }
            }
        }

        #endregion

        //–--------------events ------------------―
        

        private void connectionEstablishedEvent(object msg)
        { 
           var con = msg as ConnectionRequestMessage;

           if (con != null)
           {
               if (con.connectionType == connectionRQType)
               {
                   Socket connection = con.GetSocket(); 
                   if(connection != null)
                   {
                       HandleNewConnection(connection);
                   }
               }
           }
            
        }

        private void HandleNewConnection(Socket socket)
        {
            try
            {
                var waitingForData = true;

                while (waitingForData)
                {
                    waitingForData = !socket.Poll(10, SelectMode.SelectRead) && !socket.Poll(10, SelectMode.SelectError);

                    if (socket.Available > 0)
                    {
                        byte[] data = new byte[socket.Available];
                        socket.Receive(data);

                        byte cmd = data[0];
                        byte[] pack = data.GetPart(1, data.Length - 1); 

                        switch(cmd)
                        {
                            case 0x03: //Update File
                                    UpdateFileCommand(pack);
                                break;

                            case 0x05: //Get File
                                {
                                    string file = data.GetString();

                                    FileBase b = InfoManager.GetReadOnlyFile(file);
                                    string infoname = Path.GetFileNameWithoutExtension(file) + ".info";
                                    FileBase i = InfoManager.GetReadOnlyFile(infoname);

                                    if(i != null && b != null)
                                    {
                                        byte[] filelength = new byte[2];
                                        filelength[0] = (byte)(b.Data.Length >> 8);
                                        filelength[1] = (byte)(b.Data.Length);
                                        byte[] infolength = new byte[2];
                                        infolength[0] = (byte)(i.Data.Length >> 8);
                                        infolength[1] = (byte)(i.Data.Length);

                                        socket.Send(filelength.Add(b.Data).Add(infolength).Add(i.Data)); 
                                    }

                                }
                                break;

                            case 0x07: //File States
                                {
                                    lock(BusyLock)
                                    {
                                        FileInfo[] files = NetFileManager.GetFiles(); 

                                        foreach(FileInfo f in files)
                                        {
                                            string infoname = Path.GetFileNameWithoutExtension(f.Name)+".info";
                                            string filename = f.Name;

                                            FileBase b = InfoManager.GetReadOnlyFile(infoname);
                                            if (b != null)
                                            {
                                                InfoFile infof = new InfoFile(b);

                                                byte[] packet = infof.Version.ToByte();
                                                packet.Add(filename.StringToBytes());
                                                socket.Send(packet);

                                            }
                                        }
                                    }
                                }
                                break;

                        }

                    }
                }

            }
            finally
            {
                if (socket != null)
                {
                    socket.Close();
                }
            }
        }

        private void UpdateFileCommand(byte[] pack)
        {

            int pathLength = (pack[0] << 8) + pack[1];
            byte[] filePath = pack.GetPart(3, pathLength);
            string path = filePath.GetString(); 

            int fileLength = (pack[3 + pathLength] << 8) + pack[4 + pathLength];
            byte[] file = pack.GetPart(4 + pathLength + 1, fileLength);

            int infoLength = (pack[4 + pathLength + 1 + fileLength] << 8) + pack[4 + pathLength + 2+ fileLength];
            byte[] info = pack.GetPart(4 + pathLength + 3 + fileLength, infoLength);

            lock (BusyLock)
            {
                FileBase File_file = null;
                FileBase File_info = null;

                if (NetFileManager.FileExists(path))
                {
                    File_file = NetFileManager.GetFile(path);
                }
                else
                {
                    File_file = NetFileManager.CreateFile(path);
                }

                if (InfoManager.FileExists(Path.GetFileNameWithoutExtension(path) + ".info"))
                {
                    InfoManager.GetFile(Path.GetFileNameWithoutExtension(path) + ".info");
                }
                else
                {
                    InfoManager.CreateFile(Path.GetFileNameWithoutExtension(path) + ".info");
                }

                if (File_file != null && File_info != null)
                {
                    File_file.Data = file;
                    File_info.Data = info;

                }

                if (File_info != null)
                    File_info.Close();
                if (File_file != null)
                    File_file.Close();
            }
        }

        private void ConnectionRq(object msg)
        {

        }

        //--------------------------------------------------------------------

        Hashtable MutexCollection = new Hashtable(); 

        private bool isMutexFree(string path)
        {
            lock (MutexCollection)
            {
                if (MutexCollection.Contains(path))
                {
                    Socket s = MutexCollection[path] as Socket;
                    if (SocketConnected(s))
                        return false;
                    else
                    {
                        freeMutex(path,s);
                        return true;
                    }
                }
            }

            return true; 
        }

        static bool SocketConnected(Socket socket)
        {
            if (socket != null)
            {
                bool ret = !socket.Poll(1, SelectMode.SelectRead) && !socket.Poll(1, SelectMode.SelectError);
                try
                {
                    ret = socket.Available == 0 ? ret : ret;
                }
                catch
                {
                    ret = false;
                    if (socket != null)
                        socket.Close();
                }
                return ret;
            }
            return false;
        }

        private bool getMutex(string path, Socket s)
        {
            lock (MutexCollection)
            {
                if (isMutexFree(path))
                {
                    MutexCollection.Add(path, s);
                    return true;
                }
                return false;
            }
        }

        private bool freeMutex(string path, Socket socket)
        {
            lock (MutexCollection)
            {
                if (!isMutexFree(path))
                {
                    Socket s = MutexCollection[path] as Socket;
                    if (s == socket)
                    {
                        if (s != null)
                            s.Close();

                        MutexCollection.Remove(path);
                        return true;
                    }
                }
                return false;
            }
        }

        private void MutexClear()
        {
            while(MutexCollection.Count>0)
            {
                var keys = MutexCollection.Keys.GetEnumerator();
                string path = keys.Current as string;
                Socket s = MutexCollection[path] as Socket;

                if (s != null)
                    s.Close();

                MutexCollection.Remove(path); 
            }
        }


        //--------------------------------------------------------------------

        public bool FileExists(string path)
        {
            return NetFileManager.FileExists(path);
        }

        public FileBase GetFile(string path)
        {
            if (isMutexFree(path) && isOnline)
            {

                //GetMutex 
                //Create Close Method; 
                // return file; 
            }
            return null; 
        }

        public FileBase GetReadOnlyFile(string path)
        {
            return NetFileManager.GetReadOnlyFile(path);
        }

        public bool DeleteFile(string path)
        {
            if (isOnline)
            {
                //Get Mutex 
                //Send Deleat File Command 
                //Close connection
                
            }
            return false;
        }


        /*
        public void CloseFile(FileBase localfile)
        {
            if (isOnline)
            {
                //Send Update.
                //Close Connection 
            }
        }
        */

        //--------------------------------------------------------------------


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
