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

namespace ZeiControl.MVVM.ViewModel
{
    class MainWindowViewModel
    {
        public TcpClient tcpClient;
        private static readonly IPAddress hostIP = IPAddress.Parse("192.168.1.107");
        private readonly IPEndPoint hostEndPoint = new(hostIP, 60555);
        private readonly List<byte[]> txMessageQueue = new();

        private bool isConnected = false;
        private bool isTransmittingImage = false;
        private int rxThreshold = 8;

        private void StartConnectionStream()
        {
            _ = SendMessageTask();
            _ = ReceiveMessageTask();

            isConnected = true;
        }

        private void StopConnectionStream()
        {
            try
            {
                tcpClient.Client.Shutdown(SocketShutdown.Both);
                tcpClient.Client.Close();

                isConnected = false;
            }
            catch (Exception exc)
            {
                Trace.WriteLine("Internal error: ");
                Trace.WriteLine(exc.Message);
            }
        }

        public async Task SendMessageTask()
        {
            while (isConnected)
            {
                if (txMessageQueue.Count > 0)
                {
                    try
                    {
                        NetworkStream networkStream = tcpClient.GetStream();
                        if (networkStream.CanWrite)
                        {
                            networkStream.Write(txMessageQueue[0], 0, 8);
                            networkStream.Close();
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
        }

        public async Task ReceiveMessageTask()
        {
            while (isConnected)
            {
                if(tcpClient.Available >= rxThreshold)
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
            }
            await Task.Delay(20);
        }

        public void OnDataReceived(byte[] data)
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
