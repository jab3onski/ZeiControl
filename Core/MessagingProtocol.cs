using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ZeiControl.Core
{
    class MessagingProtocol
    {
        private bool isTransmittingImage;

        //Protocol move commands
        public static readonly byte[] forwardPacket = { 0x23, 0x4D, 0x5F, 0xFF, 0x00, 0x00, 0x00, 0x23 };
        public static readonly byte[] leftPacket = { 0x23, 0x4D, 0x5F, 0x00, 0xFF, 0x00, 0x00, 0x23 };
        public static readonly byte[] rightPacket = { 0x23, 0x4D, 0x5F, 0x00, 0x00, 0xFF, 0x00, 0x23 };
        public static readonly byte[] reversePacket = { 0x23, 0x4D, 0x5F, 0x00, 0x00, 0x00, 0xFF, 0x23 };
        public static readonly byte[] reverseLeftPacket = { 0x23, 0x4D, 0x5F, 0x00, 0xFF, 0x00, 0xFF, 0x23 };
        public static readonly byte[] reverseRightPacket = { 0x23, 0x4D, 0x5F, 0x00, 0x00, 0xFF, 0xFF, 0x23 };
        public static readonly byte[] emergencyStopPacket = { 0x23, 0x4D, 0x5F, 0x00, 0x00, 0x00, 0x00, 0x23 };
        public static readonly byte[] emergencyOverPacket = { 0x23, 0x4D, 0x5F, 0xFF, 0xFF, 0xFF, 0x00, 0x23 };
        public static readonly byte[] stopPacket = { 0x23, 0x4D, 0x5F, 0xFF, 0xFF, 0xFF, 0xFF, 0x23 };

        //Protocol camera commands
        public static readonly byte[] cameraEnablePacket = { 0x23, 0x43, 0x5F, 0xFF, 0xFF, 0xFF, 0xFF, 0x23 };
        public static readonly byte[] cameraDisablePacket = { 0x23, 0x43, 0x5F, 0x00, 0x00, 0x00, 0x00, 0x23 };

        //Misc commands (lights, beep)
        public static readonly byte[] powerLEDonPacket = { 0x23, 0x4C, 0x5F, 0x00, 0xFF, 0xFF, 0xFF, 0x23 };
        public static readonly byte[] powerLEDoffPacket = { 0x23, 0x4C, 0x5F, 0x00, 0xFF, 0x00, 0x00, 0x23 };
        public static readonly byte[] buzzerOnPacket = { 0x23, 0x42, 0x5F, 0xFF, 0xFF, 0xFF, 0xFF, 0x23 };
        public static readonly byte[] buzzerOffPacket = { 0x23, 0x42, 0x5F, 0x00, 0x00, 0x00, 0x00, 0x23 };

        public MessagingProtocol()
        {
            isTransmittingImage = false;
        }

        public void ProcessIncomingData(byte[] data)
        {
            if (data[0] == 0x23 && data[7] == 0x23)
            {
                byte[] valueData32Bit = { data[3], data[4], data[5], data[6] };
                byte[] valueData16Bit = { data[5], data[6] };

                if (data[1] == 0x4A)
                {
                    int jpegSize = BitConverter.ToInt32(valueData32Bit);

                    isTransmittingImage = true;
                    NetworkHandling.rxThreshold = jpegSize;
                }

                else if (data[1] == 0x54)
                {
                    byte[] valueData = { data[5], data[6] };
                    double tempValue =
                        Math.Round(BitConverter.ToUInt16(valueData) * 5.0 / 1024.0 * 100, 2);

                    SQLiteConnection connection = DatabaseHandling.CreateConnection();
                    try
                    {
                        DatabaseHandling.AddSensorEntryToTemp(connection, "Temperature", tempValue);

                        MainWindow.DbListView.Items.Insert
                            (0, new SensorData
                            {
                                Id = MainWindow.SensorUpdatesCounter,
                                SensorType = "Temperature",
                                SensorValue = tempValue,
                                DateTimeValue = DateTime.Now.ToString()
                            });

                        MainWindow.SensorUpdatesCounter++;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                    connection.Close();
                }

                else if (data[1] == 0x50)
                {
                    ushort proximityValue = BitConverter.ToUInt16(valueData16Bit);

                    if (data[3] == 0x00 && data[4] == 0x00)
                    {
                        if (proximityValue < 1000)
                        {
                            MainWindow.LeftSensorTextBlock.Text = "TRIGGERED";
                            MainWindow.LeftSensorTextBlock.Foreground = Brushes.Red;
                        }
                        else
                        {
                            MainWindow.LeftSensorTextBlock.Text = "CLEAR";
                            MainWindow.LeftSensorTextBlock.Foreground = Brushes.Green;
                        }
                    }

                    else if (data[3] == 0x00 && data[4] == 0xFF)
                    {
                        if (proximityValue < 1000)
                        {
                            MainWindow.RightSensorTextBlock.Text = "TRIGGERED";
                            MainWindow.RightSensorTextBlock.Foreground = Brushes.Red;
                        }
                        else
                        {
                            MainWindow.RightSensorTextBlock.Text = "CLEAR";
                            MainWindow.RightSensorTextBlock.Foreground = Brushes.Green;
                        }
                    }

                    else if (data[3] == 0xFF && data[4] == 0x00)
                    {
                        double distanceValue =
                            Math.Pow(proximityValue * 3.15 / 675.0, -1.173) * 29.988;

                        if (distanceValue <= 90)
                        {
                            MainWindow.FrontSensorTextBlock.Text =
                            string.Concat(Math.Round(distanceValue, 1).ToString(), " cm");
                        }
                        else
                        {
                            MainWindow.FrontSensorTextBlock.Text = "Overrange";
                        }
                    }

                }

                else if (data[1] == 0x55)
                {
                    uint uptimeValue = BitConverter.ToUInt32(valueData32Bit) / 1000;
                    TimeSpan currentUptime =
                        TimeSpan.FromSeconds(uptimeValue);

                    MainWindow.UptimeTextBlock.Text = currentUptime.ToString();
                }

                else if (data[1] == 0x44)
                {
                    ushort diagnosticsValue = BitConverter.ToUInt16(valueData16Bit);

                    if (data[3] == 0x00 && data[4] == 0x00)
                    {
                        MainWindow.CellVoltageTextBlock.Text = diagnosticsValue.ToString();
                    }

                    else if (data[3] == 0x00 && data[4] == 0xFF)
                    {
                        MainWindow.RRSITextBlock.Text =
                            string.Concat("-", diagnosticsValue.ToString(), " dBm");
                    }
                }
            }
            else if (isTransmittingImage && data[0] == 0xFF && data[1] == 0xD8)
            {
                isTransmittingImage = false;

                //Update image object to received frame
                MainWindow.StreamSourceFrame.Source = (BitmapSource)new ImageSourceConverter().ConvertFrom(data);

                NetworkHandling.rxThreshold = 8;
            }
            else
            {
                Trace.Write("Unknown Packet!##: ");
                Trace.WriteLine(BitConverter.ToString(data).Substring(0, 8));
            }
        }

        public static void ProcessOutgoingData(byte[] data)
        {
            Trace.WriteLine(BitConverter.ToString(data));

            if (NetworkHandling.isConnected)
            {
                NetworkHandling.txMessageQueue.Add(data);
            }
        }

        public static void SendMessageTempInterval(int value)
        {
            ProcessOutgoingData(HelperMethods.TransformAsBytePacket16Bit(value, 0x49, 0x00, 0x00));
        }

        public static void SendMessageProximityInterval(int value)
        {
            ProcessOutgoingData(HelperMethods.TransformAsBytePacket16Bit(value, 0x49, 0x00, 0xFF));
        }
    }
}
