using System;
using System.Collections;
using ECRU.Utilities.LeadFollow;
using Microsoft.SPOT;

namespace ECRU.Utilities
{
    /// <summary>
    ///     The EventBus class implements the Publisher/Subscriber design pattern. The class is threadsafe.
    /// </summary>
    public static class EventBus
    {
        private static readonly Hashtable Subscriptions = new Hashtable();
        private static readonly Object Lock = new object();
        private static readonly WorkPool ThreadPool = new WorkPool(5);

        /// <summary>
        ///     Subscribe will add the subscriber to a Event.
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="messageHandler"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>This method returns a bool.</returns>
        public static bool Subscribe(Type messageType, EventHandler messageHandler)
        {
            if (messageType == null) throw new ArgumentNullException("subscriber.Message");
            if (messageHandler == null) throw new ArgumentNullException("subscriber.FunctionPointer");

            if (Subscriptions.Contains(messageType))
            {
                Debug.Print("MessageBus - Message in subscriptions");
                lock (Lock)
                {
                    var list = Subscriptions[messageType] as ArrayList;
                    if (list != null)
                    {
                        ((ArrayList) Subscriptions[messageType]).Add(messageHandler);
                    }
                }
                return true;
            }

            Debug.Print("MessageBus - Message not subscriptions");
            lock (Lock)
            {
                var list = new ArrayList {messageHandler};
                Subscriptions.Add(messageType, list);
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
        public static bool Unsubscribe(Type messageType, EventHandler messageHandler)
        {
            if (messageType == null) throw new ArgumentNullException("subscriber.Message");
            if (messageHandler == null) throw new ArgumentNullException("subscriber.FunctionPointer");

            if (Subscriptions.Contains(messageType))
            {
                Debug.Print("MessageBus - Message in subscriptions");
                lock (Lock)
                {
                    var list = Subscriptions[messageType] as ArrayList;
                    if (list == null)
                    {
                        throw new ArgumentException("FunctionPointer list is empty");
                    }
                    list.Remove(messageHandler);

                    if (list.Count.Equals(0))
                    {
                        Subscriptions.Remove(messageType);
                    }
                    else
                    {
                        Subscriptions[messageType] = list;
                    }

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Publish TMessage to subscribers. This method uses the current thread.
        /// </summary>
        /// <param name="message"></param>
        public static void Publish(object message)
        {
            lock (Lock)
            {
                Debug.Print("Publishing type: " + message.GetType());
                var subscribers = Subscriptions[message.GetType()] as ArrayList;

                if (subscribers != null)
                {
                    foreach (EventHandler subscriber in subscribers)
                    {
                        ThreadPool.EnqueueAction(() => subscriber(message));
                    }
                }
            }
        }
    }


    public delegate void EventHandler(object item);
}