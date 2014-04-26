using System;
using Microsoft.SPOT;
using System.Collections;
using ECRU.Utilities.HelpFunction;

namespace ECRU.File
{
    class CordinatorRole:IDisposable
    {
        const int MUTEX_MAX_LOCKTIME = 300000;  // 5 min

        ArrayList Mutexs = new ArrayList();
        readonly object Lock = new object();

        CordinatorState state { get; set; }

        public CordinatorRole()
        {
            state = CordinatorState.Starting;
            //init cor (fome event thread)
        }
            
        // adds filemutex to list if posible 
        public bool GetFileLock(string path, byte[] addr)
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
                            ReleasMutex(mutex.path, mutex.AddrHasLock);
                            break; 
                        }

                        if(mutex.AddrHasLock.ByteArrayCompare(addr))
                            return true; 
                        return false; 
                    }
                }

                Mutexs.Add(new FileMutex(path, addr));
                return true; 
            }
        }


        public void ReleasMutex(string path, byte[] addr)
        {
            lock (Lock)
            {
                for (int i = 0; i < Mutexs.Count; i++)
                {
                    FileMutex mutex = Mutexs[i] as FileMutex;
                    if (mutex != null && mutex.path == path && mutex.AddrHasLock.ByteArrayCompare(addr))
                    {
                        Mutexs.RemoveAt(i);
                    }
                }
            }
        }


        // check if mutex is taken. 
        public bool HasFileMutex(string path, byte[] addr)
        {
            for (int i = 0; i < Mutexs.Count; i++)
            {
                FileMutex mutex = Mutexs[i] as FileMutex;
                if (mutex != null && mutex.path == path)
                {
                    if ((DateTime.Now - mutex.LockTime).Milliseconds > MUTEX_MAX_LOCKTIME)
                    {
                        ReleasMutex(mutex.path, mutex.AddrHasLock);
                        return false; 
                    }

                    if (mutex.AddrHasLock.ByteArrayCompare(addr))
                        return true;
                }
            }
            return false; 
        }


        class FileMutex
        {
            public string path { get; set; }
            public DateTime LockTime { get; set; }
            public byte[] AddrHasLock { get; set;  }

            public FileMutex(string Path, byte[] addr)
            {
                path = Path;
                AddrHasLock = addr;
                LockTime = DateTime.Now; 
            }
        }


        public void Dispose()
        {
            
        }
    }

    enum CordinatorState
    {
        Starting,
        Running,
        Stoped
    }

}
