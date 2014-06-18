using System;
using Microsoft.SPOT;
using System.Collections;

namespace ECRU.Utilities
{
    static class CoordainatorQueue
    {
        private static Queue TCPMessages = new Queue();
        private static QueueItem currentItem = null; 
        public static void Send(byte[] reciver, string connectiontype, NewNetworkConnectionCallback callback)
        {
            lock(TCPMessages)
            {
                TCPMessages.Enqueue(new QueueItem() { Reciver = reciver, Connectiontype = connectiontype, Callback = callback }); 
                if(currentItem == null)
                {
                    DequeueItem();
                }
            }
        }

        public static void Clear()
        {
            lock(TCPMessages)
            {
                TCPMessages.Clear();
                currentItem = null; 
            }
        }

        private static void DequeueItem()
        {
            if (TCPMessages.Count > 0 && currentItem == null)
            {
                lock (TCPMessages)
                {
                    currentItem = TCPMessages.Dequeue() as QueueItem;
                }
                NewConnectionMessage rq = new NewConnectionMessage() { Receiver = currentItem.Reciver, ConnectionType = currentItem.Connectiontype, ConnectionCallback = (socket, resiver) => GotConection(socket, resiver, currentItem) };
                EventBus.Publish(rq);
            }
        }

        private static void GotConection(System.Net.Sockets.Socket socket, byte[] resiver, QueueItem item)
        {
            if (currentItem != null && currentItem == item)
            {
                lock (TCPMessages)
                {
                    currentItem = null;
                }
                DequeueItem();
                item.Callback(socket, resiver);
            }
        }


        private class QueueItem
        {
            public byte[] Reciver { get; set; }
            public string Connectiontype { get; set; }
            public NewNetworkConnectionCallback Callback { get; set; }
        }

    }
}
