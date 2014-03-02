using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MessageBus
{
    /// <summary>
    /// The MessageBus class implements the Publisher/Subscriber design pattern. The class is threadsafe and does not spawn new threads. 
    /// </summary>
    public static class MessageBus
    {

        private static readonly Hashtable Subscriptions = new Hashtable();
        private static readonly Object Lock = new object();

        /// <summary>
        /// Subscribe will add the subscriber to the Subscriptions hashtable.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>This method returns a bool.</returns>
        public static bool Subscribe(ISubscriber subscriber)
        {

            if (subscriber.Message == null) throw new ArgumentNullException("subscriber.Message");
            var message = subscriber.Message;

            if (subscriber.FunctionPointer == null) throw new ArgumentNullException("subscriber.FunctionPointer");
            var functionPointer = subscriber.FunctionPointer;

            if (Subscriptions.ContainsKey(message))
            {
                Debug.Print("MessageBus - Message in subscriptions");
                lock (Lock)
                {
                    var list = Subscriptions[message] as ArrayList;
                    if (list != null)
                    {
                        list.Add(functionPointer);
                    }
                }
                return true;
            }

            Debug.Print("MessageBus - Message not subscriptions");
            lock (Lock)
            {
                var list = new ArrayList { functionPointer };
                Subscriptions.Add(message, list);
            }
            return true;
        }

        /// <summary>
        /// Unsubscribe will remove the subscriber from the Subscriptions hashtable.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>This method returns a bool.</returns>
        public static bool Unsubscribe(ISubscriber subscriber)
        {

            if (subscriber.Message == null) throw new ArgumentNullException("subscriber.Message");
            var message = subscriber.Message;

            if (subscriber.FunctionPointer == null) throw new ArgumentNullException("subscriber.FunctionPointer");
            var functionPointer = subscriber.FunctionPointer;

            if (!Subscriptions.ContainsKey(message))
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
                return true;
            }
        }

        public static bool Publish(TMessage message)
        {
            var subscribers = Subscriptions[message] as ArrayList;

            if (subscribers == null) return true;
            foreach ( TMessageHandler subscriber in subscribers)
            {
                subscriber.Handle(message);
            }
            return true;
        }
    }
}
