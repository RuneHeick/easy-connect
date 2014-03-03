using System;
using System.Collections;
using Microsoft.SPOT;

namespace ECRU.EventBus
{
    /// <summary>
    ///     The EventBus class implements the Publisher/Subscriber design pattern. The class is threadsafe and does not spawn
    ///     new threads.
    /// </summary>
    public class EventBus
    {
        private static readonly Hashtable Subscriptions = new Hashtable();
        private static readonly Object Lock = new object();

        /// <summary>
        ///     Subscribe will add the subscriber to the Subscriptions hashtable.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>This method returns a bool.</returns>
        public static bool Subscribe(ISubscriber subscriber)
        {
            if (subscriber.Message == null) throw new ArgumentNullException("subscriber.Message");
            string message = subscriber.Message.Type;


            if (subscriber.FunctionPointer == null) throw new ArgumentNullException("subscriber.FunctionPointer");
            TMessageHandler functionPointer = subscriber.FunctionPointer;

            if (Subscriptions.Contains(message))
            {
                Debug.Print("MessageBus - Message in subscriptions");
                lock (Lock)
                {
                    var list = Subscriptions[message] as ArrayList;
                    if (list != null)
                    {
                        ((ArrayList) Subscriptions[message]).Add(functionPointer);
                    }
                }
                return true;
            }

            Debug.Print("MessageBus - Message not subscriptions");
            lock (Lock)
            {
                var list = new ArrayList {functionPointer};
                Subscriptions.Add(message, list);
            }
            return true;
        }

        /// <summary>
        ///     Unsubscribe will remove the subscriber from the Subscriptions hashtable.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>This method returns a bool.</returns>
        public static bool Unsubscribe(ISubscriber subscriber)
        {
            if (subscriber.Message == null) throw new ArgumentNullException("subscriber.Message");
            string message = subscriber.Message.Type;

            if (subscriber.FunctionPointer == null) throw new ArgumentNullException("subscriber.FunctionPointer");
            TMessageHandler functionPointer = subscriber.FunctionPointer;

            if (!Subscriptions.Contains(message))
            {
                throw new ArgumentException("message not in subscriptions");
            }

            Debug.Print("MessageBus - Message in subscriptions");
            lock (Lock)
            {
                var list = Subscriptions[message] as ArrayList;
                if (list == null)
                {
                    throw new ArgumentException("FunctionPointer list is empty");
                }
                list.Remove(functionPointer);

                if (list.Count.Equals(0))
                {
                    Subscriptions.Remove(message);
                }
                else
                {
                    Subscriptions[message] = list;
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
                var subscribers = Subscriptions[message] as ArrayList;

                if (subscribers == null) return;
                foreach (TMessageHandler subscriber in subscribers)
                {
                    subscriber.Handle(message);
                }
            }
        }
    }
}