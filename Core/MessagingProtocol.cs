using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeiControl.Core
{
    class MessagingProtocol
    {

        public void ProcessIncomingData(byte[] data)
        {

        }

        public void ProcessOutgoingData(byte[] data)
        {
            NetworkHandling.txMessageQueue.Add(data);
        }
    }
}
