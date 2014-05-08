using System;
using Microsoft.SPOT;
using ECRU.Utilities.HelpFunction;
using System.IO;
using ECRU.File.Files;
using ECRU.Utilities.EventBus;
using ECRU.Utilities.EventBus.Events;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using ECRU.Utilities.Timers;

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
        private bool SingleMode { get; set;  }

        const string InfoFolderName = "Info";
        ECTimer StartTimer;


        LocalManager InfoManager;
        LocalManager NetFileManager;

        const string FileFolderName = "Files";

        public NetworkManager(string path)
        {
            StartTimer = new ECTimer((o) => startCordinator(), null, 10000, Timeout.Infinite);
            isOnline = false;
            SingleMode = true; 

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
                string[] Macs = ECRU.Utilities.SystemInfo.ConnectionOverview.GetSortedMasters();

                cordinatorAddrs = Macs[0].FromHex();
                SingleMode = Macs.Length == 1 ? true : false;

                Debug.Print("CORDINATOR LOAD : " + cordinatorAddrs.ToHex());
                if (cordinatorAddrs.ByteArrayCompare(ECRU.Utilities.SystemInfo.SystemMAC) && SingleMode == false)
                {
                    //I am cordinator 
                    StartTimer.Start(); 
                }
            }
            isOnline = true; 
        }

        private void startCordinator()
        {
            if(isOnline)
            {
                cordinator = new CordinatorRole(InfoManager,NetFileManager);
            }
            StartTimer.Stop(); 
        }

        private void StopNetShare()
        {
            StartTimer.Stop(); 
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
                    Debug.Print("is in Sync: " + changed.isinsync.ToString());
                    Debug.Print("NetStage: " + changed.NetState.ToString()); 
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
                                    string file = pack.GetString();

                                    FileBase b = NetFileManager.GetReadOnlyFile(file);
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
                                                packet = packet.Add(filename.StringToBytes());
                                                socket.Send(packet);

                                            }
                                        }

                                        CloseSocket(socket);
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
                    CloseSocket(socket);
                }
            }
        }

        private void UpdateFileCommand(byte[] pack)
        {

            int pathLength = (pack[0] << 8) + pack[1];
            byte[] filePath = pack.GetPart(2, pathLength);
            string path = filePath.GetString(); 

            int fileLength = (pack[2 + pathLength] << 8) + pack[3 + pathLength];
            byte[] file = pack.GetPart(3 + pathLength + 1, fileLength);

            int infoLength = (pack[3 + pathLength + 1 + fileLength] << 8) + pack[3 + pathLength + 2+ fileLength];
            byte[] info = pack.GetPart(3 + pathLength + 3 + fileLength, infoLength);

            freeMutex(path);

            lock (BusyLock)
            {
                FileBase File_file = null;
                FileBase File_info = null;

                if (fileLength == 0 && infoLength ==0) // Del file. 
                {
                    InfoManager.DeleteFile(Path.GetFileNameWithoutExtension(path) + ".info");
                    NetFileManager.DeleteFile(path);
                    return; 
                }

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
                    File_info = InfoManager.GetFile(Path.GetFileNameWithoutExtension(path) + ".info");
                }
                else
                {
                    File_info = InfoManager.CreateFile(Path.GetFileNameWithoutExtension(path) + ".info");
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

        //--------------------------------------------------------------------

        #region Mutex

        Hashtable MutexCollection = new Hashtable(); 

        class Mutex_t
        {
            public Socket socket { get; set; }

            public DateTime Locktime { get; set; }

        }


        private bool isMutexFree(string path)
        {
            lock (MutexCollection)
            {
                UpdateTimeLockMutex(path); 
                if (MutexCollection.Contains(path))
                {
                    Mutex_t m = MutexCollection[path] as Mutex_t;
                    if (m.socket != null && SocketConnected(m.socket))
                        return false;
                    else
                    {
                        freeMutex(path,m.socket);
                        return true;
                    }
                }
            }

            return true; 
        }

        private void UpdateTimeLockMutex(string path)
        {
            lock (MutexCollection)
            {
                if (MutexCollection.Contains(path))
                {
                    Mutex_t m = MutexCollection[path] as Mutex_t;
                    if ((DateTime.Now - m.Locktime).Ticks > TimeSpan.TicksPerSecond * CordinatorRole.MUTEX_MAX_LOCKTIME)
                        MutexCollection.Remove(m);
                }
            }
        }

        private void CloseSocket(Socket s)
        {
            if (s != null)
            {
                s.Close();
                lock (MutexCollection)
                {
                    foreach (object key in MutexCollection.Keys)
                    {
                        Mutex_t m = MutexCollection[key] as Mutex_t;
                        if (m.socket != null && s == m.socket)
                            m.socket = null;  
                    }
                }
            }
        }

        private bool HasMutex(string path)
        {
            return !isMutexFree(path); 
        }

        static bool SocketConnected(Socket socket)
        {
            if (socket != null)
            {
                bool ret = false; 
                try
                {
                    ret = !socket.Poll(1, SelectMode.SelectRead) && !socket.Poll(1, SelectMode.SelectError);
               
                    ret = socket.Available == 0 ? ret : ret;
                }
                catch
                {
                    ret = false;
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
                    Mutex_t m = new Mutex_t { socket = s, Locktime = DateTime.Now };
                    MutexCollection.Add(path, m);
                    return true;
                }
                return false;
            }
        }

        private bool freeMutex(string path, Socket socket)
        {
            lock (MutexCollection)
            {
                if (MutexCollection.Contains(path))
                {
                    Mutex_t m = MutexCollection[path] as Mutex_t;
                    if (m.socket == socket || m.socket == null)
                    {
                        if (m.socket != null)
                            CloseSocket(m.socket);

                        MutexCollection.Remove(path);
                        return true;
                    }
                }
                return false;
            }
        }

        private bool freeMutex(string path)
        {
            lock (MutexCollection)
            {
                if (MutexCollection.Contains(path))
                {
                    Mutex_t m = MutexCollection[path] as Mutex_t;
                    if (m.socket != null)
                        CloseSocket(m.socket);

                    MutexCollection.Remove(path);
                    return true;
                }
                return false;
            }
        }

        private void MutexClear()
        {
            lock (MutexCollection)
            {
                while (MutexCollection.Count > 0)
                {
                    var keys = MutexCollection.Keys.GetEnumerator();
                    string path = keys.Current as string;
                    Mutex_t m = MutexCollection[path] as Mutex_t;

                    if (m.socket != null)
                        CloseSocket(m.socket);

                    MutexCollection.Remove(path);
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------

        #region FileHandles

        public bool FileExists(string path)
        {
            return NetFileManager.FileExists(path);
        }

        public FileBase GetFile(string path)
        {
            if (SingleMode)
            {
                FileBase file = null;
                if (NetFileManager.FileExists(path))
                    file = NetFileManager.GetFile(path);
                else
                {
                    file = NetFileManager.CreateFile(path);
                }

                if (!InfoManager.FileExists(Path.GetFileNameWithoutExtension(path) + ".info"))
                {
                    FileBase b = InfoManager.CreateFile(Path.GetFileNameWithoutExtension(path) + ".info");
                    InfoFile i = new InfoFile(b);
                    i.Version = 0;
                    i.Close();
                }

                if (file != null)
                {
                    var oldClose = file.Closefunc;
                    file.Closefunc = (f) =>
                    {
                        SingleModeClose(f);
                        if (oldClose != null)
                            oldClose(f);
                    };
                }
                return file;
            }
            else
            {
                if (isMutexFree(path) && isOnline)
                {
                    Thread Ask = new Thread(() => AskCordinatorForMutex(path));
                    Ask.Start();
                    Ask.Join();

                    if (HasMutex(path))
                    {
                        FileBase file = null;
                        if (FileExists(path))
                        {
                            file = NetFileManager.GetReadOnlyFile(path);
                        }
                        else
                        {
                            file = NetFileManager.CreateFile(path);
                        }

                        if (file != null)
                        {
                            file.Closefunc = CloseFile;
                        }
                        return file;
                    }

                }
            }
            return null; 
        }

        public FileBase GetReadOnlyFile(string path)
        {
            return NetFileManager.GetReadOnlyFile(path);
        }

        public bool DeleteFile(string path)
        {
            if (SingleMode)
            {
                NetFileManager.DeleteFile(path);
                InfoManager.DeleteFile(Path.GetFileNameWithoutExtension(path) + ".info");
            }
            else
            {
                if (isMutexFree(path) && isOnline)
                {
                    Thread Ask = new Thread(() => AskCordinatorForMutex(path));
                    Ask.Start();
                    Ask.Join();

                    if (HasMutex(path))
                    {
                        SendDelCommand(path);
                        return true;
                    }


                }
            }
            return false;
        }
        
        private void CloseFile(FileBase localfile)
        {
            if (MutexCollection.Contains(localfile.Path) && isOnline)
            {
                Mutex_t mutexConnection = MutexCollection[localfile.Path] as Mutex_t;
                if (mutexConnection != null)
                {
                    byte[] fileData = localfile.Data;
                    if (fileData != null && fileData.Length > 0)
                    {
                        try
                        {
                            byte[] packet = new byte[] { 0x03 };
                            packet = packet.Add(localfile.Data);
                            mutexConnection.socket.Send(packet);
                        }
                        finally
                        {
                            CloseSocket(mutexConnection.socket);
                        }
                    }
                    else
                    {
                        SendDelCommand(localfile.Path);
                    }
                }

                freeMutex(localfile.Path, mutexConnection.socket);
            }
        }

        private void SingleModeClose(FileBase localfile)
        {
            FileBase info = InfoManager.GetFile(Path.GetFileNameWithoutExtension(localfile.Path) + ".info");
            if(info != null)
            {
                InfoFile fileinfo = new InfoFile(info);
                fileinfo.Version++;
                if (localfile.Data != null && localfile.Data.Length > 0)
                {
                    var md5State = new MD5();
                    md5State.HashCore(localfile.Data, 0, localfile.Data.Length);
                    fileinfo.Hash = md5State.HashAsLong;
                }
                fileinfo.Close(); 
            }
        }

        #endregion

        //----------------------Net ------------------------------------

        public class WorkerStat
        {
            private readonly object Lock = new object();
            private bool isDone = false;
            private Thread current;

            public WorkerStat(Thread current)
            {
                this.current = current;
            } 
            public bool IsDone
            {
                get
                {
                    lock (Lock)
                    {
                        return isDone; 
                    }
                }
                set
                {
                    lock (Lock)
                    {
                        isDone= value;
                        if (isDone == true && (current.ThreadState & ThreadState.Suspended) == ThreadState.Suspended)
                            current.Resume(); 
                    }
                }
            }

            public void WaitOnDone()
            {
                while (IsDone == false)
                    current.Suspend(); 
            }

        }

        private void AskCordinatorForMutex(string path)
        {
            Thread current = Thread.CurrentThread; 
            WorkerStat  state = new WorkerStat(current);

            NewConnectionMessage rq = new NewConnectionMessage() { Receiver = cordinatorAddrs, ConnectionType = CordinatorRole.CordinatorType, ConnectionCallback = (s, r) => AskCordinatorForMutexConnection(s, r, state, path) };
            EventBus.Publish(rq);

            state.WaitOnDone(); 

        }

        private void AskCordinatorForMutexConnection(Socket con, byte[] reciver, WorkerStat AskingThread, string path)
        {
            try
            {
                if (con != null)
                {

                    byte[] request = new byte[1+path.Length];
                    request[0] = 0x01; 
                    request.Set(path.StringToBytes(),1); 
                    con.Send(request);
                    var waitingforData = true;
                    while (waitingforData)
                    {
                        if (( !con.Poll(10, SelectMode.SelectRead) && !con.Poll(10, SelectMode.SelectError)) == false)
                            waitingforData = false; 

                        if (con.Available > 0)
                        {
                            byte[] buffer = new byte[con.Available];
                            con.Receive(buffer);
                            if(buffer[0]==0x01)
                            {
                                getMutex(path,con); 
                                return; 
                            }
                            else
                                break; 
                        }
                    }

                    CloseSocket(con);

                }

            }
            catch
            {
                if (con != null)
                    CloseSocket(con); 
            }
            finally
            {
                AskingThread.IsDone = true;
            }
        }

        private void SendDelCommand(string path)
        {
            Mutex_t mutexConnection = MutexCollection[path] as Mutex_t;
            if (mutexConnection != null)
            {
                try
                {
                    byte[] packet = new byte[] { 0x06 };
                    mutexConnection.socket.Send(packet);
                }
                finally
                {
                    CloseSocket(mutexConnection.socket);
                }
            }

            freeMutex(path, mutexConnection.socket);
        }

        //------------------------------------------------


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
