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
            lock (poolLock)
            {
                for (int i = 0; i < pool.Length; i++)
                {
                    if (pool[i] == null || pool[i].IsAlive == false)
                    {

                        pool[i] = new Thread(()=>Threadrun(i));
                        pool[i].Start(); 
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


        private void Threadrun(int poolindex)
        {
            try
            {
                while (actionQueue.Count > 0)
                {
                    Action item = getJob();
                    if (item != null)
                        item();
                }
            }
            catch
            {
                pool[poolindex] = null; 
            }
        }

        public void StopAll()
        {
            lock (actionQueueLock)
            {
                actionQueue.Clear(); 
            }

            lock (poolLock)
            {
                for (int i = 0; i < pool.Length; i++)
                {
                    if (pool[i] != null)
                    {
                        if (pool[i].IsAlive == true)
                            pool[i].Abort();
                        pool[i] = null; 
                    }
                }
            }
        }
    }

    public delegate void Action();
}