using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
        //byte[] bytePacket = { 0x23, 0x4D, 0x5F, 0xFF, 0x00, 0x00, 0x00, 0x23 };

        private bool isConnected;
        private bool isTransmittingImage;
        private int rxThreshold = 8;

        public void StartConnectionStream()
        {
            if (!isConnected)
            {
                try
                {
                    tcpClient = new();
                    tcpClient.ReceiveBufferSize = 131072; //128kb buffer (images go up to around 110kb)
                    tcpClient.SendBufferSize = 2048;
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
                    //tcpClient.Client.Shutdown(SocketShutdown.Both);
                    //tcpClient.Client.Close();

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
                        Trace.WriteLine("Message sent");
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
                            byte[] receivedBytes = new byte[tcpClient.ReceiveBufferSize];
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
            ProcessIncomingData(data);
        }

        private void ProcessIncomingData(byte[] data)
        {
            if (data[0] == 0x23 && data[7] == 0x23)
            {
                if (data[1] == 0x50)
                {
                    byte[] sizeData = { data[3], data[4], data[5], data[6] };
                    int jpegSize = BitConverter.ToInt32(sizeData);
                    isTransmittingImage = true;
                    rxThreshold = jpegSize;
                }
            }
            else if (isTransmittingImage)
            {
                if (data[0] == 0xFF && data[1] == 0xD8)
                {
                    BitmapSource bitmapSource = (BitmapSource)new ImageSourceConverter().ConvertFrom(data);
                    isTransmittingImage = false;
                    rxThreshold = 8;
                }
            }
        }

    }
}
