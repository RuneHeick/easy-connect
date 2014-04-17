using System;
using System.Collections;
using System.Threading;

namespace ECRU.Utilities.LeadFollow
{
    public class WorkPool
    {
        private readonly ArrayList actionQueue = new ArrayList();
        private readonly Object actionQueueLock = new object();
        private readonly Thread[] pool;


        private bool running = true;

        public WorkPool(int ThreadPoolSize)
        {
            pool = new Thread[ThreadPoolSize];
            for (int i = 0; i < pool.Length; i++)
            {
                pool[i] = new Thread(Threadrun);
                pool[i].Start();
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


        private void TryStart()
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