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
        public static readonly byte[] smallLEDonPacket = { 0x23, 0x4C, 0x5F, 0x00, 0x00, 0xFF, 0xFF, 0x23 };
        public static readonly byte[] smallLEDoffPacket = { 0x23, 0x4C, 0x5F, 0x00, 0x00, 0x00, 0x00, 0x23 };
        public static readonly byte[] powerLEDonPacket = { 0x23, 0x4C, 0x5F, 0x00, 0xFF, 0xFF, 0xFF, 0x23 };
        public static readonly byte[] powerLEDoffPacket = { 0x23, 0x4C, 0x5F, 0x00, 0xFF, 0x00, 0x00, 0x23 };
        public static readonly byte[] laserOnPacket = { 0x23, 0x4C, 0x5F, 0xFF, 0x00, 0xFF, 0xFF, 0x23 };
        public static readonly byte[] laserOffPacket = { 0x23, 0x4C, 0x5F, 0xFF, 0x00, 0x00, 0x00, 0x23 };
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
                if (data[1] == 0x4A)
                {
                    byte[] sizeData = { data[3], data[4], data[5], data[6] };
                    int jpegSize = BitConverter.ToInt32(sizeData);

                    //send datasize to database and add to DatabaseListView
                    SQLiteConnection connection = DatabaseHandling.CreateConnection();
                    try
                    {
                        DatabaseHandling.AddSensorEntryToTemp(connection, "TestData", jpegSize);

                        _ = MainWindow.DbListView.Items.Add(
                            new SensorData { Id = MainWindow.SensorUpdatesCounter, SensorType = "Test", SensorValue = jpegSize, DateTimeValue = DateTime.Now });

                        MainWindow.SensorUpdatesCounter++;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                    connection.Close();

                    isTransmittingImage = true;
                    NetworkHandling.rxThreshold = jpegSize;
                }
            }
            else if (isTransmittingImage)
            {
                if (data[0] == 0xFF && data[1] == 0xD8)
                {
                    isTransmittingImage = false;

                    //Update image object to received frame
                    MainWindow.StreamSourceFrame.Source = (BitmapSource)new ImageSourceConverter().ConvertFrom(data);

                    NetworkHandling.rxThreshold = 8;
                }
            }
        }

        public static void ProcessOutgoingData(byte[] data)
        {
            if(NetworkHandling.isConnected)
            {
                Trace.WriteLine(BitConverter.ToString(data));
                NetworkHandling.txMessageQueue.Add(data);
            }
        }
    }
}
