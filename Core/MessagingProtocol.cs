﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ZeiControl.Core
{
    class MessagingProtocol
    {
        private bool isTransmittingImage;
        private bool isTransmittingRequestedCaptureSD;
        private bool isTransmittingRequestedCaptureHD;

        public static bool UserNotifiedVoltage50 { get; set; }
        public static bool UserNotifiedVoltage25 { get; set; }
        public static bool UserNotifiedVoltage10 { get; set; }

        public static bool UserNotifiedRSSILow { get; set; }
        public static bool UserNotifiedRSSIVeryLow { get; set; }

        public static bool UserNotifiedUptimeOverflow { get; set; }

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
        public static readonly byte[] cameraReqConfSDPacket = { 0x23, 0x43, 0x5F, 0x00, 0x00, 0xFF, 0x00, 0x23 };
        public static readonly byte[] cameraReqConfHDPacket = { 0x23, 0x43, 0x5F, 0x00, 0x00, 0xFF, 0xFF, 0x23 };

        //Misc commands (lights, beep)
        public static readonly byte[] powerLEDonPacket = { 0x23, 0x4C, 0x5F, 0x00, 0xFF, 0xFF, 0xFF, 0x23 };
        public static readonly byte[] powerLEDoffPacket = { 0x23, 0x4C, 0x5F, 0x00, 0xFF, 0x00, 0x00, 0x23 };
        public static readonly byte[] buzzerOnPacket = { 0x23, 0x42, 0x5F, 0xFF, 0xFF, 0xFF, 0xFF, 0x23 };
        public static readonly byte[] buzzerOffPacket = { 0x23, 0x42, 0x5F, 0x00, 0x00, 0x00, 0x00, 0x23 };

        //Sensor Commands
        public static readonly byte[] requestTempValuePacket = { 0x23, 0x49, 0x5F, 0xFF, 0x00, 0x00, 0x00, 0x23 };

        //Autonomous Driving commands
        public static readonly byte[] autonomousDrivingEnablePacket = { 0x23, 0x41, 0x5F, 0x00, 0x00, 0x00, 0xFF, 0x23 };
        public static readonly byte[] autonomousDrivingDisablePacket = { 0x23, 0x41, 0x5F, 0x00, 0x00, 0x00, 0x00, 0x23 };
        public static readonly byte[] autonomousDrivingEnableConfPacket = { 0x23, 0x41, 0x5F, 0xFF, 0x00, 0x00, 0xFF, 0x23 };
        public static readonly byte[] autonomousDrivingDisableConfPacket = { 0x23, 0x41, 0x5F, 0xFF, 0x00, 0x00, 0x00, 0x23 };

        public MessagingProtocol()
        {
            isTransmittingImage = false;
            isTransmittingRequestedCaptureSD = false;
            isTransmittingRequestedCaptureHD = false;
            UserNotifiedVoltage50 = false;
            UserNotifiedVoltage25 = false;
            UserNotifiedVoltage10 = false;

            UserNotifiedRSSILow = false;
            UserNotifiedRSSIVeryLow = false;

            UserNotifiedUptimeOverflow = false;
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
                    Trace.WriteLine("Expected size: " + jpegSize);

                    isTransmittingImage = true;
                    NetworkHandling.rxThreshold = jpegSize;
                }

                else if (data[1] == 0x43)
                {
                    if (data[3] == 0x00 && data[4] == 0x00 && data[5] == 0xFF)
                    {
                        if (data[6] == 0x00)
                        {
                            isTransmittingRequestedCaptureSD = true;
                        }
                        else if (data[6] == 0xFF)
                        {
                            isTransmittingRequestedCaptureHD = true;
                        }
                    }
                }

                else if (data[1] == 0x54)
                {
                    byte[] valueData = { data[5], data[6] };
                    double tempValue =
                        Math.Round(BitConverter.ToUInt16(valueData) * 5.0 / 1023.0 * 100, 2);

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

                        MainWindow.FrontSensorTextBlock.Text =
                            distanceValue is <= 80 and >= 10 ? string.Concat(
                                Math.Round(distanceValue, 1).ToString(), " cm") : "Overrange";
                    }

                }

                else if (data[1] == 0x55)
                {
                    uint uptimeValue = BitConverter.ToUInt32(valueData32Bit);
                    TimeSpan currentUptime =
                        TimeSpan.FromSeconds(uptimeValue / 1000);

                    MainWindow.UptimeTextBlock.Text = currentUptime.ToString();

                    if (uptimeValue > 4208567295) // Overflow warning 24hrs before event
                    {
                        if (!UserNotifiedUptimeOverflow)
                        {
                            MainWindow.NotificationsListView.Items.Insert(0, NotificationData.UptimeNearLimit);
                            UserNotifiedUptimeOverflow = true;
                        }
                    }
                }

                else if (data[1] == 0x44)
                {
                    ushort diagnosticsValue = BitConverter.ToUInt16(valueData16Bit);

                    if (data[3] == 0x00 && data[4] == 0x00)
                    {
                        double voltageValue = Math.Round(diagnosticsValue * 8.4 / 1023.0, 2);

                        if (voltageValue is < 6.0 or > 8.4)
                        {
                            MainWindow.CellVoltageTextBlock.Text = voltageValue < 6.0 ? "UNDERVOLTAGE" : "OVERVOLTAGE";
                            MainWindow.CellVoltageTextBlock.Foreground = Brushes.Red;
                        }
                        else
                        {
                            MainWindow.CellVoltageTextBlock.Text =
                                string.Concat(voltageValue.ToString(), " V");

                            byte voltageScale255 = HelperMethods.RescaleToByteValue(6, 8.4, 0, 255, voltageValue);

                            byte redValue = voltageScale255;
                            byte greenValue = (byte)(255 - redValue);
                            SolidColorBrush voltageColorBrush = new(Color.FromRgb(greenValue, redValue, 0)); // Colors are reversed

                            MainWindow.CellVoltageTextBlock.Foreground = voltageColorBrush;
                        }

                        //Safe voltage range for 2S Lithium Polymer battery is 6.0V - 8.4V
                        if (voltageValue < 6.24)
                        {
                            if (!UserNotifiedVoltage10)
                            {
                                MainWindow.NotificationsListView.Items.Insert(0,
                                NotificationData.BatteryBelow10);
                                UserNotifiedVoltage10 = true;
                            }
                        }

                        else if (voltageValue < 6.6)
                        {
                            if (!UserNotifiedVoltage25)
                            {
                                MainWindow.NotificationsListView.Items.Insert(0,
                                NotificationData.BatteryBelow25);
                                UserNotifiedVoltage25 = true;
                            }
                        }

                        else if (voltageValue < 7.2)
                        {
                            if (!UserNotifiedVoltage50)
                            {
                                MainWindow.NotificationsListView.Items.Insert(0,
                                NotificationData.BatteryBelow50);
                                UserNotifiedVoltage50 = true;
                            }
                        }
                    }

                    else if (data[3] == 0x00 && data[4] == 0xFF)
                    {
                        if (diagnosticsValue < 30)
                        {
                            diagnosticsValue = 30;
                        }
                        else if (diagnosticsValue > 105)
                        {
                            diagnosticsValue = 105;
                        }

                        MainWindow.RRSITextBlock.Text =
                            string.Concat("-", diagnosticsValue.ToString(), " dBm");

                        byte RSSIScale255 = HelperMethods.RescaleToByteValue(30, 105, 0, 255, diagnosticsValue);

                        byte redValue = RSSIScale255;
                        byte greenValue = (byte)(255 - redValue);
                        SolidColorBrush RSSIColorBrush = new(Color.FromRgb(redValue, greenValue, 0));

                        MainWindow.RRSITextBlock.Foreground = RSSIColorBrush;

                        if (diagnosticsValue > 100)
                        {
                            if (!UserNotifiedRSSIVeryLow)
                            {
                                MainWindow.NotificationsListView.Items.Insert(0, NotificationData.RSSIVeryLow);
                                UserNotifiedRSSIVeryLow = true;
                            }
                        }
                        else if (diagnosticsValue > 85)
                        {
                            if (!UserNotifiedRSSILow)
                            {
                                MainWindow.NotificationsListView.Items.Insert(0, NotificationData.RSSILow);
                                UserNotifiedRSSILow = true;
                            }
                        }
                    }
                }

                else if (data[1] == 0x41)
                {
                    if (data[3] == 0xFF && data[4] == 0x00)
                    {
                        if (data[6] == 0xFF)
                        {
                            HelperMethods.EMModeEnabled(true);
                            MainWindow.NotificationsListView.Items.Insert(0, NotificationData.AutonomousDrivingActive);
                        }
                        else if (data[6] == 0x00)
                        {
                            HelperMethods.EMModeEnabled(false);
                            MainWindow.NotificationsListView.Items.Insert(0, NotificationData.AutonomousDrivingInactive);
                        }
                    }
                }

                else if (data[1] == 0x49)
                {
                    ushort intervalValue = BitConverter.ToUInt16(valueData16Bit);

                    if (data[3] == 0x00)
                    {
                        if (data[4] == 0x00)
                        {
                            SensorSettingsWindow.TextBoxTempInterval.Text = intervalValue.ToString();
                        }
                    }
                }
                else
                {
                    Trace.WriteLine("Unknown Packet");
                }
            }
            else if (isTransmittingImage && data[0] == 0xFF && data[1] == 0xD8)
            {
                isTransmittingImage = false;

                if (isTransmittingRequestedCaptureSD)
                {
                    isTransmittingRequestedCaptureSD = false;
                    MainWindow.RequestedSDImage = false;

                    HelperMethods.SaveImageFromByteArray(data, true);

                    string path = MainWindow.ProgramPath + "Captures";

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        Arguments = path,
                        FileName = "explorer.exe"
                    };
                    _ = Process.Start(startInfo);
                }

                else if (isTransmittingRequestedCaptureHD)
                {
                    isTransmittingRequestedCaptureHD = false;
                    MainWindow.RequestedHDImage = false;

                    HelperMethods.SaveImageFromByteArray(data, false);

                    string path = MainWindow.ProgramPath + "Captures";

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        Arguments = path,
                        FileName = "explorer.exe"
                    };
                    _ = Process.Start(startInfo);
                }

                //Update image object to received frame
                else if (MainWindow.EnableCameraButton.IsChecked == true)
                {
                    MainWindow.StreamSourceFrame.Source =
                        (BitmapSource)new ImageSourceConverter().ConvertFrom(data);
                }

                NetworkHandling.rxThreshold = 8;
            }
        }

        public static void ProcessOutgoingData(byte[] data)
        {
            if (NetworkHandling.isConnected)
            {
                NetworkHandling.txMessageQueue.Add(data);
            }
        }

        public static void SendMessageTempInterval(int value)
        {
            ProcessOutgoingData(HelperMethods.TransformToBytePacket16Bit(value, 0x49, 0x00, 0x00));
        }

        public static void SendMessageProximityInterval(int value)
        {
            ProcessOutgoingData(HelperMethods.TransformToBytePacket16Bit(value, 0x49, 0x00, 0xFF));
        }
    }
}
