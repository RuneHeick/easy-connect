using System;
using Microsoft.SPOT;
using System.Collections;
using ECRU.Utilities.HelpFunction;
using ECRU.Utilities.EventBus.Events;
using ECRU.Utilities.EventBus;
using System.Net.Sockets;
using ECRU.File.Files;
using System.IO;
using System.Threading;

namespace ECRU.File
{
    class CordinatorRole:IDisposable
    {
        const int MUTEX_MAX_LOCKTIME = 300000;  // 5 min
        const string CordinatorType = "CordinatorRq";
        const int ConnectionTimeOut = 3000; 

        ArrayList Mutexs = new ArrayList();
        readonly object Lock = new object();
        LocalManager InfoManager;
        LocalManager NetFileManager;

        Thread MutexHandler;
        bool Disposed = false; 

        CordinatorState state { get; set; }
        string[] Users;


        public CordinatorRole(LocalManager InfoManager, LocalManager NetFileManager)
        {
            MutexHandler = new Thread(HandelMutexConnections);
            MutexHandler.Start(); 

            state = CordinatorState.Starting;


            //init cor (fome event thread)
            this.InfoManager = InfoManager;
            this.NetFileManager = NetFileManager;
            
            EventBus.Subscribe(typeof(ConnectionRequestMessage), ConnectionHandel);
            Users = Utilities.SystemInfo.ConnectionOverview.GetSortedMasters();
            if (Users.Length != 1)
            {
                GetFileStates();
            }
            else
            {
                StartCordinator();
            }
        }

        private void ConnectionHandel(object message)
        {
            var msg = message as ConnectionRequestMessage;
            
            if (msg != null  && state == CordinatorState.Running)
            {
                if(msg.connectionType == CordinatorType)
                {
                    Socket con = msg.GetSocket();
                    if (con != null)
                    {
                        DateTime StartTime = DateTime.Now;

                        bool Handeled = false;
                        while ((DateTime.Now - StartTime).Milliseconds < ConnectionTimeOut)
                        {
                            try
                            {

                                if (( !con.Poll(10, SelectMode.SelectRead) && !con.Poll(10, SelectMode.SelectError)) == false)
                                    break;

                                if (con.Available > 0)
                                {
                                    byte[] buffer = new byte[con.Available];
                                    con.Receive(buffer);

                                    switch (buffer[0])
                                    {
                                        case 0x01:
                                            {
                                                byte[] ret = new byte[1];
                                                string path = buffer.GetPart(1, buffer.Length - 1).GetString();
                                                if(GetFileLock(path, con))
                                                {
                                                    ret[0] = 1;
                                                    con.Send(ret);
                                                    Handeled = true; 
                                                }
                                                else
                                                {
                                                    ret[0] = 0;
                                                    con.Send(ret);
                                                }
                                            }
                                            break;
                                        case 0x02: // Copy 
                                            {
                                               byte[] ret = new byte[1];
                                               string path = buffer.GetPart(1, buffer.Length - 1).GetString();
                                               FileBase file = NetFileManager.GetFile(path);
                                               if(file != null)
                                               {
                                                   con.Send(file.Data);
                                                   file.Close(); 
                                               }
                                               else
                                               {
                                                   ret[0] = 0;
                                                   con.Send(ret);
                                               }
                                            }
                                            break;
                                        case 0x04: // Rqu File 
                                            {
                                                byte[] ret = new byte[1];
                                                string path = buffer.GetPart(1, buffer.Length - 1).GetString();
                                                FileBase file = NetFileManager.GetFile(path);
                                                FileBase info = InfoManager.GetFile(Path.GetFileNameWithoutExtension(path) + ".info");

                                                if (file != null && info != null)
                                                {
                                                    int fileLength = file.Data.Length;
                                                    int infoLength = info.Data.Length;

                                                    byte[] data = new byte[2 + fileLength + 2 + infoLength];
                                                    data.Set(file.Data, 2);
                                                    data.Set(info.Data, 2 + fileLength+2);
                                                    data[0] = (byte)(fileLength>>8);
                                                    data[1] = (byte)(fileLength);

                                                    data[2 + fileLength] = (byte)(infoLength >> 8);
                                                    data[2 + fileLength + 1] = (byte)(infoLength);

                                                    con.Send(data);
                                                    file.Close();
                                                    info.Close(); 
                                                }
                                                else
                                                {
                                                    ret[0] = 0;
                                                    con.Send(ret);
                                                }
                                            }
                                            break; 
                                        case 0x06: //Request Info 
                                            {
                                                byte[] ret = new byte[1];
                                                string path = buffer.GetPart(1, buffer.Length - 1).GetString();
                                                FileBase info = InfoManager.GetFile(Path.GetFileNameWithoutExtension(path) + ".info");
                                                if (info != null)
                                                {
                                                    con.Send(info.Data);
                                                    info.Close(); 
                                                }
                                                else
                                                {
                                                    ret[0] = 0;
                                                    con.Send(ret);
                                                }
                                            }
                                            break;

                                        default:

                                            break;

                                    }

                                    break;
                                }
                            }
                            catch
                            {
                                ReleasMutex(con);
                                con.Close();
                                break; 
                            }

                        }

                        if(Handeled == false)
                            con.Close();
                    }

                }

            }
        }

        #region Mutex

        private void ReleasMutex(Socket con)
        {
            lock (Lock)
            {
                for (int i = 0; i < Mutexs.Count; i++)
                {
                    FileMutex mutex = Mutexs[i] as FileMutex;
                    if (mutex != null && mutex.AddrHasLock == con)
                    {
                        if (mutex.AddrHasLock != null)
                        {
                            mutex.AddrHasLock.Close();
                        }
                        Mutexs.RemoveAt(i);
                    }
                }
            }
        }
            
        // adds filemutex to list if posible 
        public bool GetFileLock(string path, Socket addr)
        {
            lock(Lock)
            {
                for(int i = 0; i<Mutexs.Count; i++)
                {
                    FileMutex mutex = Mutexs[i] as FileMutex; 
                    if(mutex != null && mutex.path == path)
                    {
                        if ((DateTime.Now - mutex.LockTime).Milliseconds > MUTEX_MAX_LOCKTIME)
                        {
                            ReleasMutex(mutex.path);
                            break; 
                        }

                        if(mutex.AddrHasLock == addr)
                            return true; 
                        return false; 
                    }
                }

                Mutexs.Add(new FileMutex(path, addr));
                if (MutexHandler.ThreadState == ThreadState.Suspended)
                    MutexHandler.Resume(); 
                return true; 
            }
        }

        public void ReleasMutex(string path)
        {
            lock (Lock)
            {
                for (int i = 0; i < Mutexs.Count; i++)
                {
                    FileMutex mutex = Mutexs[i] as FileMutex;
                    if (mutex != null && mutex.path == path)
                    {
                        if (mutex.AddrHasLock != null )
                        {
                            mutex.AddrHasLock.Close();
                        }

                        Mutexs.RemoveAt(i);
                    }
                }
            }
        }

        // check if mutex is taken. 
        public bool HasFileMutex(string path, Socket addr)
        {
            for (int i = 0; i < Mutexs.Count; i++)
            {
                FileMutex mutex = Mutexs[i] as FileMutex;
                if (mutex != null && mutex.path == path)
                {
                    if ((DateTime.Now - mutex.LockTime).Milliseconds > MUTEX_MAX_LOCKTIME)
                    {
                        ReleasMutex(mutex.path);
                        return false; 
                    }

                    if (mutex.AddrHasLock == addr)
                        return true;
                }
            }
            return false; 
        }

        public bool HasMutex(string path)
        {
            for (int i = 0; i < Mutexs.Count; i++)
            {
                FileMutex mutex = Mutexs[i] as FileMutex;
                if (mutex != null && mutex.path == path)
                {
                    if ((DateTime.Now - mutex.LockTime).Milliseconds > MUTEX_MAX_LOCKTIME)
                    {
                        ReleasMutex(mutex.path);
                        return false;
                    }

                    return true;
                }
            }
            return false;
        }
         
        #endregion

        private void HandelMutexConnections()
        {
            try
            {
                while (Disposed == false)
                {
                    for (int i = 0; i < Mutexs.Count; i++)
                    {
                        FileMutex mutex = Mutexs[i] as FileMutex;
                        if (mutex != null)
                        {
                            Socket con = mutex.AddrHasLock;
                            try
                            {
                                if (con.Available > 0)
                                {
                                    byte[] buffer = new byte[con.Available];
                                    con.Receive(buffer);
                                    HandelRQ(mutex, buffer);
                                }
                                // Remove the item if timeout. 
                                if ((DateTime.Now - mutex.LockTime).Milliseconds > MUTEX_MAX_LOCKTIME)
                                {
                                    ReleasMutex(con);
                                }
                            }
                            catch
                            {
                                ReleasMutex(con);
                            }
                        }
                    }

                    if (Mutexs.Count == 0)
                        MutexHandler.Suspend();
                }
            }
            catch
            {
                // when thread is stoped; 
            }
        }

        private void HandelRQ(FileMutex mutex, byte[] buffer)
        {
            switch(buffer[0])
            {
                case 0x03: // Update File;
                    {
                        FileBase file;
                        FileBase info;
                        InfoFile infofile;

                        if (NetFileManager.FileExists(mutex.path))
                        {
                            file = NetFileManager.GetFile(mutex.path);
                            info = InfoManager.GetFile(Path.GetFileNameWithoutExtension(mutex.path) + ".info");
                        }
                        else
                        {
                            file = NetFileManager.CreateFile(mutex.path);
                            info = InfoManager.CreateFile(Path.GetFileNameWithoutExtension(mutex.path) + ".info");
                            infofile = new InfoFile(info);
                            infofile.Version = 0;
                            infofile.Hash = 0;
                        }

                        infofile = new InfoFile(info);
                        file.Data = buffer.GetPart(1, buffer.Length - 1);
                        infofile.Version++;

                        var md5State = new MD5();
                        md5State.HashCore(file.Data, 0, file.Data.Length);
                        infofile.Hash = md5State.HashAsLong;

                        byte[] ack = new byte[] { 0x01 };
                        mutex.AddrHasLock.Send(ack);
                        mutex.AddrHasLock.Close();
                        file.Close();
                        info.Close();

                        UpdateFile(mutex); //Releases Mutex

                    }
                    break;
                case 0x06: //del file 
                    {
                        if (NetFileManager.FileExists(mutex.path))
                        {

                            FileBase info = InfoManager.GetFile(Path.GetFileNameWithoutExtension(mutex.path) + ".info");
                           
                            NetFileManager.DeleteFile(mutex.path);
                            byte[] ack = new byte[] { 0x01 };
                            mutex.AddrHasLock.Send(ack);
                            mutex.AddrHasLock.Close();
                            InfoFile infofile = new InfoFile(info);
                            infofile.Hash = 0;
                            infofile.Version = 0;
                            info.Close();
                            InfoManager.DeleteFile(Path.GetFileNameWithoutExtension(mutex.path) + ".info");

                            UpdateFile(mutex);
                        }
                        else
                        {
                            ReleasMutex(mutex.path); //Releases Mutex
                        }
                    }
                    break; 
            }
        }

        private void UpdateFile(FileMutex mutex)
        {
            ReleasMutex(mutex.path);
            foreach(string RU in Users)
            {
                if(RU != Utilities.SystemInfo.SystemMAC.ToHex())
                {
                    NewConnectionMessage rq = new NewConnectionMessage() { Receiver = RU.FromHex(), ConnectionType = NetworkManager.connectionRQType, ConnectionCallback = (socket, r) => UpdateFileConnection(mutex.path,r, socket, this) };
                    EventBus.Publish(rq);
                }
            } 
        }

        private static void UpdateFileConnection(string path, byte[] receiver, Socket socket, CordinatorRole item)
        {
            if (item.Disposed)
            {
                if (socket != null)
                    socket.Close();
                return;
            }

            if (socket != null)
            {
                    FileBase file = null;
                    FileBase info = null;
                try
                {
                        file = item.NetFileManager.GetFile(path);
                        if (file != null)
                        {
                            info = item.InfoManager.GetFile(Path.GetFileNameWithoutExtension(path) + ".info");

                            int fileLength = file.Data.Length;
                            int infoLength = info.Data.Length;
                            int pathlen = path.Length;

                            byte[] data = new byte[1 + + 2+ pathlen + 2 + fileLength + 2 + infoLength];
                            data[0] = 0x03;

                            data[1] = (byte)(pathlen >> 8);
                            data[2] = (byte)(pathlen);
                            data.Set(path.StringToBytes(), 3);

                            data[1 + pathlen+2] = (byte)(fileLength >> 8);
                            data[2 + pathlen+2] = (byte)(fileLength);
                            data.Set(file.Data, 3 + pathlen+2);

                            data[fileLength + pathlen + 2 +3] = (byte)(infoLength >> 8);
                            data[fileLength + pathlen + 2 + 4] = (byte)(infoLength);
                            data.Set(info.Data, fileLength + pathlen + 2 + 5);                  

                            socket.Send(data);
                        }
                }
                finally
                {
                    if (file != null)
                        file.Close();
                    if (file != null)
                        info.Close();

                    if (socket != null)
                    {
                        socket.Close();
                    }
                }

            }
            else
            {
                NewConnectionMessage rq = new NewConnectionMessage() { Receiver = receiver, ConnectionType = NetworkManager.connectionRQType, ConnectionCallback = (s, r) => UpdateFileConnection(path, r, s, item) };
                EventBus.Publish(rq);
            }
        }

        // ------------------------------- Starting --------------------------------------

        private void GetFileStates()
        {

            ArrayList PacketSendTo = new ArrayList();
            Hashtable FileState = new Hashtable();

            FileInfo[] files = NetFileManager.GetFiles();

            foreach (FileInfo f in files)
            {
                string infoname = Path.GetFileNameWithoutExtension(f.Name) + ".info";
                string filename = f.Name;

                FileBase b = InfoManager.GetReadOnlyFile(infoname);
                if (b != null)
                {
                    InfoFile infof = new InfoFile(b);

                    AddINTable(filename, FileState, infof.Version, Utilities.SystemInfo.SystemMAC, this);
                }
            }
            

            foreach (string RU in Users)
            {
                if (RU != Utilities.SystemInfo.SystemMAC.ToHex())
                {
                    lock (PacketSendTo)
                        PacketSendTo.Add(RU);
                }  
            }

            foreach (string RU in Users)
            {
                if (RU != Utilities.SystemInfo.SystemMAC.ToHex())
                {
                    NewConnectionMessage rq = new NewConnectionMessage() { Receiver = RU.FromHex(), ConnectionType = NetworkManager.connectionRQType, ConnectionCallback = (socket, resiver) => FileStateConnection(socket, resiver, PacketSendTo, FileState, this) };
                    EventBus.Publish(rq);
                    
                }
            } 
        }

        private static void FileStateConnection(Socket socket, byte[] resiver, ArrayList PacketSendTo, Hashtable FileState, CordinatorRole item)
        {
            if (item.Disposed)
            {
                if (socket != null)
                    socket.Close();
                return;
            }

            if (socket != null)
            {
                try
                {
                    var waitingForData = true;

                    byte[] rq = new byte[] { 0x07 };
                    socket.Send(rq);

                    while (waitingForData)
                    {
                        waitingForData = !socket.Poll(-1, SelectMode.SelectRead) && !socket.Poll(-1, SelectMode.SelectError);

                        if (socket.Available > 8)
                        {
                            byte[] data = new byte[socket.Available];
                            socket.Receive(data);

                            long version = data.ToLong(0);
                            string Path = data.GetPart(8, data.Length - 8).GetString();

                            lock (FileState)
                            {
                                AddINTable(Path, FileState, version, resiver, item);
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

                lock (PacketSendTo)
                {
                    PacketSendTo.Remove(resiver.ToHex());
                    if (PacketSendTo.Count == 0)
                    {
                        item.GetNewestFiles(FileState);
                    }
                }
            }
            else //Retry;
            {
                NewConnectionMessage rq = new NewConnectionMessage() { Receiver = resiver, ConnectionType = NetworkManager.connectionRQType, ConnectionCallback = (so, re) => FileStateConnection(so, re, PacketSendTo, FileState, item) };
                EventBus.Publish(rq);
                return;

            }

        }

        private static void AddINTable(string Path, Hashtable FileState, long version, byte[] resiver, CordinatorRole item)
        {
            if (item.Disposed == false)
            {
                if (FileState.Contains(Path))
                {
                    Hashtable file = FileState[Path] as Hashtable;
                    if (file.Contains(resiver.ToHex()))
                    {
                        file[resiver.ToHex()] = version;
                    }
                    else
                    {
                        file.Add(resiver.ToHex(), version);
                    }

                }
                else
                {
                    Hashtable table = new Hashtable();
                    table.Add(resiver.ToHex(), version);

                    foreach (string user in item.Users)
                    {
                        if (!table.Contains(user))
                            table.Add(user, 0);
                    }

                    FileState.Add(Path, table);
                }
            }
        }

        private void GetNewestFiles(Hashtable FileState)
        {
            bool HasAllFiles = true;
            lock(FileState)
            {
                
                foreach(object key in FileState.Keys)
                {
                    Hashtable Filestates = FileState[key] as Hashtable;
                    string path = key as string;

                    FileState_t best = GetBestFileState(Filestates);

                    FileBase file = InfoManager.GetFile(Path.GetFileNameWithoutExtension(path) + ".info");
                    InfoFile currentFile = null; 

                    if (file != null)
                    {
                       currentFile = new InfoFile(file);

                    }

                    if (currentFile == null || currentFile.Version < best.version)
                    {
                        NewConnectionMessage rq = new NewConnectionMessage() { Receiver = best.Mac, ConnectionType = NetworkManager.connectionRQType, ConnectionCallback = (socket, resiver) => GetFileConnection(socket, resiver, path, FileState, best.version, this) };
                        EventBus.Publish(rq);
                        HasAllFiles = false; 
                    }
                    else
                    {
                        lock (FileState)
                        {
                            Filestates.Remove(Utilities.SystemInfo.SystemMAC.ToHex());
                        }
                        UpdateFiles(path, Filestates, best.version, this);
                    }

                    file.Close(); 
                }
            }
                

        }

        private static FileState_t GetBestFileState(Hashtable Filestates)
        {
            string bestMac = null;
            long bestVersion = 0; 

            foreach (object key in Filestates.Keys)
            {
                long version = Convert.ToInt64(Filestates[key].ToString());
                string mac = key as string;

                if (version > bestVersion)
                {
                    bestVersion = version;
                    bestMac = mac;
                }

            }

            return new FileState_t{Mac = bestMac.FromHex(), version = bestVersion}; 

        }

        private static void GetFileConnection(Socket socket, byte[] resiver, string path, Hashtable MasterFileStates, long version, CordinatorRole item)
        {

            if (item.Disposed)
            {
                if (socket != null)
                    socket.Close();
                return;
            }
            
            if(socket != null)
            {
                try
                {
                    var waitingForData = true;

                    
                    byte[] rq = new byte[] { 0x05 };
                    rq = rq.Add(path.StringToBytes());
                    socket.Send(rq);

                    while (waitingForData)
                    {
                        waitingForData = !socket.Poll(10, SelectMode.SelectRead) && !socket.Poll(10, SelectMode.SelectError);

                        if (socket.Available > 8)
                        {
                            byte[] data = new byte[socket.Available];
                            socket.Receive(data);

                            int filelen = (data[0] << 8) + data[1];
                            int infolen = (data[2 + filelen] << 8) + data[2 + filelen+1];
                            
                            byte[] file = data.GetPart(2,filelen);
                            byte[] info = data.GetPart(2+filelen+2,infolen);

                            FileBase File_file = null;
                            FileBase File_info = null; 

                            if(item.NetFileManager.FileExists(path))
                            {
                                File_file = item.NetFileManager.GetFile(path);
                            }
                            else
                            {
                                File_file = item.NetFileManager.CreateFile(path);
                            }

                            if (item.InfoManager.FileExists(Path.GetFileNameWithoutExtension(path) + ".info"))
                            {
                                File_info = item.InfoManager.GetFile(Path.GetFileNameWithoutExtension(path) + ".info");
                            }
                            else
                            {
                                File_info = item.InfoManager.CreateFile(Path.GetFileNameWithoutExtension(path) + ".info");
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

                            break; 
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

                UpdateFiles(path,MasterFileStates, version, item);

            }
            else
            {
                NewConnectionMessage rq = new NewConnectionMessage() { Receiver = resiver, ConnectionType = NetworkManager.connectionRQType, ConnectionCallback = (so, re) => GetFileConnection(so, re, path, MasterFileStates, version, item) };
                EventBus.Publish(rq);
            }

        }


        private static void UpdateFiles(string path, Hashtable MasterFileStates, long version, CordinatorRole item)
        {
            lock (MasterFileStates)
            {
                // Update Others 
                if (MasterFileStates.Contains(path))
                {
                    Hashtable FileStates = MasterFileStates[path] as Hashtable;
                    FileStates.Remove(Utilities.SystemInfo.SystemMAC.ToHex()); // Remove Self. 

                    foreach (object key in FileStates.Keys)
                    {
                        long ver = Convert.ToInt64(FileStates[key].ToString());
                        string mac = key as string;

                        if (ver < version)
                        {
                            NewConnectionMessage rq = new NewConnectionMessage() { Receiver = mac.FromHex(), ConnectionType = NetworkManager.connectionRQType, ConnectionCallback = (s, r) => UpdateFileConnection(path,r, s, item) };
                            EventBus.Publish(rq);
                        }
                        else
                        {
                            FileStates.Remove(key);
                            if (FileStates.Count == 0)
                                MasterFileStates.Remove(path);
                        }
                    }

                    if (MasterFileStates.Count == 0) // Check if Last 
                    {
                        item.StartCordinator();
                    }
                }
            }
        }

        private void StartCordinator()
        {
            state = CordinatorState.Running; 
        }


        class FileMutex
        {
            public string path { get; set; }
            public DateTime LockTime { get; set; }
            public Socket AddrHasLock { get; set;  }

            public FileMutex(string Path, Socket addr)
            {
                path = Path;
                AddrHasLock = addr;
                LockTime = DateTime.Now; 
            }
        }

        class FileState_t
        {
            public long version { get; set; }
            public byte[] Mac { get; set; }

        }

        public void Dispose()
        {
            Disposed = true; 
            while (Mutexs.Count != 0)
            {
                FileMutex mutex = Mutexs[0] as FileMutex;
                if(mutex != null)
                {
                    ReleasMutex(mutex.path); 
                }
            }
            MutexHandler.Abort(); 
            EventBus.Unsubscribe(typeof(ConnectionRequestMessage), ConnectionHandel);
        }
    }

    enum CordinatorState
    {
        Starting,
        Running,
        Stoped
    }

}
