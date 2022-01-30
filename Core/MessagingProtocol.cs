using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeiControl.Core
{
    class MessagingProtocol
    {
        //NetworkHandling nwHandling = new();

        public void ProcessIncomingData(byte[] data)
        {

        }

        public void ProcessOutgoingData(byte[] data)
        {
            NetworkHandling.txMessageQueue.Add(data);
        }
    }
}
