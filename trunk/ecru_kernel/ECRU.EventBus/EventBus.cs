using System;
using System.Collections;
using Microsoft.SPOT;

namespace ECRU.EventBus
{
    /// <summary>
    ///     The EventBus class implements the Publisher/Subscriber design pattern. The class is threadsafe and does not spawn
    ///     new threads.
    /// </summary>
    public static class EventBus
    {
        private static readonly Hashtable Subscriptions = new Hashtable();
        private static readonly Object Lock = new object();

        /// <summary>
        ///     Subscribe will add the subscriber to the Subscriptions hashtable.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageHandler"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>This method returns a bool.</returns>
        public static bool Subscribe(TMessage message, TMessageHandler messageHandler)
        {
            if (message == null) throw new ArgumentNullException("subscriber.Message");
            var type = message.Type;

            if (messageHandler == null) throw new ArgumentNullException("subscriber.FunctionPointer");

            if (Subscriptions.Contains(type))
            {
                Debug.Print("MessageBus - Message in subscriptions");
                lock (Lock)
                {
                    var list = Subscriptions[type] as ArrayList;
                    if (list != null)
                    {
                        ((ArrayList)Subscriptions[type]).Add(messageHandler);
                    }
                }
                return true;
            }

            Debug.Print("MessageBus - Message not subscriptions");
            lock (Lock)
            {
                var list = new ArrayList { messageHandler };
                Subscriptions.Add(type, list);
            }
            return true;
        }

        /// <summary>
        ///     Unsubscribe will remove the subscriber from the Subscriptions hashtable.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageHandler"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>This method returns a bool.</returns>
        public static bool Unsubscribe(TMessage message, TMessageHandler messageHandler)
        {
            if (message == null) throw new ArgumentNullException("subscriber.Message");
            var type = message.Type;

            if (messageHandler == null) throw new ArgumentNullException("subscriber.FunctionPointer");

            if (!Subscriptions.Contains(type))
            {
                throw new ArgumentException("message not in subscriptions");
            }

            Debug.Print("MessageBus - Message in subscriptions");
            lock (Lock)
            {
                var list = Subscriptions[type] as ArrayList;
                if (list == null)
                {
                    throw new ArgumentException("FunctionPointer list is empty");
                }
                list.Remove(messageHandler);

                if (list.Count.Equals(0))
                {
                    Subscriptions.Remove(type);
                }
                else
                {
                    Subscriptions[type] = list;
                }

                return true;
            }
        }

        /// <summary>
        ///     Publish TMessage to subscribers. This method uses the current thread.
        /// </summary>
        /// <param name="message"></param>
        public static void Publish(TMessage message)
        {
            lock (Lock)
            {
                var subscribers = Subscriptions[message.Type] as ArrayList;

                if (subscribers == null) return;
                foreach (TMessageHandler subscriber in subscribers)
                {
                    subscriber.Handle(message);
                }
            }
        }
    }
}