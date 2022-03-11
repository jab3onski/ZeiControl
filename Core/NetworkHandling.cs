using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ZeiControl.Core
{
    class NetworkHandling
    {
        private static readonly IPAddress hostIP = IPAddress.Parse("192.168.1.107");
        private readonly IPEndPoint hostEndPoint = new(hostIP, 60555);
        public static List<byte[]> txMessageQueue = new();
        public static TcpClient tcpClient;

        public static bool isConnected;
        public static int rxThreshold = 8;

        private readonly MessagingProtocol messagingProtocol = new();

        public void StartConnectionStream()
        {
            if (!isConnected)
            {
                try
                {
                    tcpClient = new();
                    tcpClient.ReceiveBufferSize = 131072; //128kb buffer (images go up to around 110kb)
                    tcpClient.Connect(hostEndPoint);
                    isConnected = true;

                    _ = SendMessageTask();
                    _ = ReceiveMessageTask();
                }
                catch (Exception exc)
                {
                    Trace.WriteLine("Internal error: ");
                    Trace.WriteLine(exc.Message);
                    isConnected = false;
                }
            }
        }

        public void StopConnectionStream()
        {
            if(isConnected)
            {
                try
                {
                    txMessageQueue.Add(MessagingProtocol.stopPacket);
                    txMessageQueue.Add(MessagingProtocol.cameraDisablePacket);

                    tcpClient.Client.Shutdown(SocketShutdown.Both);
                    tcpClient.Client.Close();

                    isConnected = false;
                }
                catch (Exception exc)
                {
                    Trace.WriteLine("Internal error: ");
                    Trace.WriteLine(exc.Message);
                    isConnected = false;
                }
            }
        }

        private async Task SendMessageTask()
        {
            NetworkStream networkStream = tcpClient.GetStream();
            while (isConnected)
            {
                if (txMessageQueue.Count > 0)
                {
                    try
                    {
                        if (networkStream.CanWrite)
                        {
                            networkStream.Write(txMessageQueue[0]);
                        }
                    }
                    catch (Exception exc)
                    {
                        Trace.WriteLine("Internal error: ");
                        Trace.WriteLine(exc.Message);
                    }
                    finally
                    {
                        txMessageQueue.RemoveAt(0);
                    }
                }
                await Task.Delay(20);
            }
            networkStream.Close();
        }

        private async Task ReceiveMessageTask()
        {
            while (isConnected)
            {
                if (tcpClient.Available >= rxThreshold)
                {
                    try
                    {
                        NetworkStream networkStream = tcpClient.GetStream();
                        if (networkStream.CanRead)
                        {
                            byte[] receivedBytes = new byte[rxThreshold];
                            networkStream.Read(receivedBytes, 0, rxThreshold);
                            OnDataReceived(receivedBytes);
                        }
                    }
                    catch (Exception exc)
                    {
                        Trace.WriteLine("Internal error: ");
                        Trace.WriteLine(exc.Message);
                    }
                }
                await Task.Delay(20);
            }
        }

        private void OnDataReceived(byte[] data)
        {
            //Trace.WriteLine(BitConverter.ToString(data));
            messagingProtocol.ProcessIncomingData(data);
        }
    }
}
