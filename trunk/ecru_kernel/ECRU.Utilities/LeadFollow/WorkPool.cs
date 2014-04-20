using System;
using System.Collections;
using System.Threading;

namespace ECRU.Utilities.LeadFollow
{
    public class WorkPool
    {
        private ArrayList actionQueue = new ArrayList();
        private readonly Object actionQueueLock = new object();

        private  Thread[] pool;
        private readonly Object poolLock = new object();


        private bool running = true;

        public WorkPool(int ThreadPoolSize)
        {
            lock (poolLock)
            {
                pool = new Thread[ThreadPoolSize];
                for (int i = 0; i < pool.Length; i++)
                {
                    pool[i] = new Thread(Threadrun);
                    pool[i].Start();
                }
            }
        }


        public bool EnqueueAction(Action item)
        {
            try
            {
                lock (actionQueueLock)
                {
                    actionQueue.Add(item);
                    TryStart();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void AddThisThreadNoReturn()
        {
            Thread thisThread = Thread.CurrentThread;
            Thread[] Temppool = new Thread[pool.Length + 1];
            lock (poolLock)
            {
                for(int i = 0; i<pool.Length;i++)
                {
                    Temppool[i] = pool[i];
                    if (pool[i] == thisThread)
                        return;
                }
                Temppool[pool.Length] = thisThread;
                pool = Temppool;
            }
            Threadrun();
        }


        private void TryStart()
        {
            lock (poolLock)
            {
                foreach (Thread t in pool)
                {
                    if (t.ThreadState == ThreadState.Suspended)
                    {
                        t.Resume();
                        break;
                    }
                }
            }
        }

        private Action getJob()
        {
            lock (actionQueueLock)
            {
                if (actionQueue.Count > 0)
                {
                    var item = (Action) actionQueue[0];
                    actionQueue.RemoveAt(0);
                    return item;
                }
                return null;
            }
        }


        private void Threadrun()
        {
            while (running)
            {
                while (actionQueue.Count > 0)
                {
                    Action item = getJob();
                    if (item != null)
                        item();
                }
                Thread.CurrentThread.Suspend();
            }
        }
    }

    public delegate void Action();
}