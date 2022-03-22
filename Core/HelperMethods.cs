using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ZeiControl.Core
{
    class HelperMethods
    {
        public static byte RescaleToByteValue(double initialValue)
        {
            byte scaled = (byte)(initialValue / 110 * 255);

            return scaled;
        }

        public static byte[] TransformAsBytePacket32Bit(double value, byte commandByte)
        {
            byte[] arrayOfBytes = new byte[8];
            byte[] valueByteArray = BitConverter.GetBytes((uint)value);

            arrayOfBytes[0] = 0x23;
            arrayOfBytes[1] = commandByte;
            arrayOfBytes[2] = 0x5F;
            arrayOfBytes[3] = valueByteArray[0];
            arrayOfBytes[4] = valueByteArray[1];
            arrayOfBytes[5] = valueByteArray[2];
            arrayOfBytes[6] = valueByteArray[3];
            arrayOfBytes[7] = 0x23;

            return arrayOfBytes;
        }

        public static byte[] TransformAsBytePacket16Bit(double value,
            byte commandByte, byte deviceByte1, byte deviceByte2)
        {
            byte[] arrayOfBytes = new byte[8];
            byte[] valueByteArray = BitConverter.GetBytes((ushort)value);

            arrayOfBytes[0] = 0x23;
            arrayOfBytes[1] = commandByte;
            arrayOfBytes[2] = 0x5F;
            arrayOfBytes[3] = deviceByte1;
            arrayOfBytes[4] = deviceByte2;
            arrayOfBytes[5] = valueByteArray[0];
            arrayOfBytes[6] = valueByteArray[1];
            arrayOfBytes[7] = 0x23;

            return arrayOfBytes;
        }

        public static void ChangePropertyVariableWiFi(bool isEnabled)
        {
            if (isEnabled)
            {
                MainWindow.SaveDataButton.IsEnabled = true;
                MainWindow.ClearDataButton.IsEnabled = true;
                MainWindow.SettingsButton.IsEnabled = true;

                MainWindow.MoveForwardButton.IsEnabled = true;
                MainWindow.MoveLeftButton.IsEnabled = true;
                MainWindow.MoveRightButton.IsEnabled = true;
                MainWindow.MoveReverseButton.IsEnabled = true;
                MainWindow.MoveReverseLeftButton.IsEnabled = true;
                MainWindow.MoveReverseRightButton.IsEnabled = true;
                MainWindow.BuzzerEnableButton.IsEnabled = true;
                MainWindow.PLEDEnableButton.IsEnabled = true;
                MainWindow.StopEnableButton.IsEnabled = true;

                MainWindow.EnableCameraButton.IsEnabled = true;
                MainWindow.ZeroOutAxisButton.IsEnabled = true;

                MainWindow.Xslider.IsEnabled = true;
                MainWindow.Yslider.IsEnabled = true;

                MainWindow.CloseConnectionButton.IsEnabled = true;
                MainWindow.WiFiConnectButton.IsEnabled = false;
                MainWindow.WiFiConnectButton.Content = "Connected!";
                MainWindow.AutonomousDrivingButton.IsEnabled = true;
            }

            else
            {
                MainWindow.SaveDataButton.IsEnabled = false;
                MainWindow.ClearDataButton.IsEnabled = false;
                MainWindow.SettingsButton.IsEnabled = false;

                MainWindow.MoveForwardButton.IsEnabled = false;
                MainWindow.MoveLeftButton.IsEnabled = false;
                MainWindow.MoveRightButton.IsEnabled = false;
                MainWindow.MoveReverseButton.IsEnabled = false;
                MainWindow.MoveReverseLeftButton.IsEnabled = false;
                MainWindow.MoveReverseRightButton.IsEnabled = false;
                MainWindow.BuzzerEnableButton.IsEnabled = false;
                MainWindow.PLEDEnableButton.IsEnabled = false;
                MainWindow.StopEnableButton.IsEnabled = false;

                MainWindow.EnableCameraButton.IsEnabled = false;
                MainWindow.ZeroOutAxisButton.IsEnabled = false;

                MainWindow.Xslider.IsEnabled = false;
                MainWindow.Yslider.IsEnabled = false;

                MainWindow.FrontSensorTextBlock.Text = "---";
                MainWindow.LeftSensorTextBlock.Text = "---";
                MainWindow.RightSensorTextBlock.Text = "---";
                MainWindow.RRSITextBlock.Text = "---";
                MainWindow.RRSITextBlock.Foreground = Brushes.Black;
                MainWindow.CellVoltageTextBlock.Text = "---";
                MainWindow.CellVoltageTextBlock.Foreground = Brushes.Black;
                MainWindow.UptimeTextBlock.Text = "---";

                MainWindow.CloseConnectionButton.IsEnabled = false;
                MainWindow.WiFiConnectButton.IsEnabled = true;
                MainWindow.WiFiConnectButton.Content = "Connect";
                MainWindow.AutonomousDrivingButton.IsEnabled = false;

                //Variables reset
                MainWindow.EnableCameraButton.IsChecked = false;

                MessagingProtocol.UserNotifiedVoltage10 = false;
                MessagingProtocol.UserNotifiedVoltage25 = false;
                MessagingProtocol.UserNotifiedVoltage50 = false;

                MessagingProtocol.UserNotifiedRSSILow = false;
                MessagingProtocol.UserNotifiedRSSIVeryLow = false;

                MessagingProtocol.UserNotifiedUptimeOverflow = false;
            }
        }

        public static void FlushSocketBuffer(TcpClient tcpClient)
        {
            if (tcpClient.Available > 0)
            {
                try
                {
                    NetworkStream networkStream = tcpClient.GetStream();
                    if (networkStream.CanRead)
                    {
                        byte[] receivedBytes = new byte[tcpClient.Available];
                        networkStream.Read(receivedBytes, 0, tcpClient.Available);
                        Trace.WriteLine("Flushed buffer size: " + receivedBytes.Length + " bytes");
                    }
                }
                catch (Exception exc)
                {
                    Trace.WriteLine("Internal error: ");
                    Trace.WriteLine(exc.Message);
                }
            }
        }
    }
}
