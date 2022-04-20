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
        public static int rxThreshold;

        private readonly MessagingProtocol messagingProtocol = new();

        public NetworkHandling()
        {
            rxThreshold = 8;
        }

        public void StartConnectionStream()
        {
            if (!isConnected)
            {
                try
                {
                    tcpClient = new();
                    tcpClient.ReceiveBufferSize = 524288; //512kb buffer (images go up to around 110kb)
                    tcpClient.Connect(hostEndPoint);
                    isConnected = true;

                    HelperMethods.FlushSocketBuffer(tcpClient);

                    _ = SendMessageTask();
                    _ = ReceiveMessageTask();

                    HelperMethods.ChangePropertyVariableWiFi(true);

                    MainWindow.NotificationsListView.Items.Insert(0, NotificationData.LoginAttemptSuccessfull);
                }
                catch (Exception exc)
                {
                    Trace.WriteLine("Internal error: ");
                    Trace.WriteLine(exc.Message);
                    MainWindow.NotificationsListView.Items.Insert(0, NotificationData.LoginAttemptFailed);
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

                    HelperMethods.ChangePropertyVariableWiFi(false);
                    MainWindow.NotificationsListView.Items.Insert(0, NotificationData.WiFiDisconnected);
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
                        networkStream.Write(txMessageQueue[0]);
                    }
                    catch (Exception exc)
                    {
                        Trace.WriteLine("Internal error: ");
                        Trace.WriteLine(exc.Message);
                        MainWindow.NotificationsListView.Items.Insert(0, NotificationData.WiFiInterrupted);
                        HelperMethods.ChangePropertyVariableWiFi(false);
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
            NetworkStream networkStream = tcpClient.GetStream();
            while (isConnected)
            {
                if (tcpClient.Available >= rxThreshold)
                {
                    try
                    {
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
            networkStream.Close();
        }

        private void OnDataReceived(byte[] data)
        {
            if (data.Length < 10)
            {
                //Trace.WriteLine(BitConverter.ToString(data));
            }
            messagingProtocol.ProcessIncomingData(data);
        }
    }
}
