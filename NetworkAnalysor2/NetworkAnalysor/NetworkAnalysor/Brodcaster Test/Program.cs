using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading; 

namespace Brodcaster_Test
{
    class Program
    {
        const int NUMBER_OF_ECRU = 10; 

        static void Main(string[] args)
        {
            UdpClient client = new UdpClient();

            List<ECRUPetent> items = new List<ECRUPetent>();

            for (int i = 0; i < NUMBER_OF_ECRU;i++)
            {
                items.Add(new ECRUPetent(client));
                Thread.Sleep(1000); 
            }

            Thread.Sleep(Timeout.Infinite);

            

        }
    }
}
