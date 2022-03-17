using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZeiControl.Core
{
    class HelperMethods
    {
        public static int RescaleToAnalogValue(double initialValue)
        {
            initialValue += 90;
            int scaled = (int)(initialValue / 180 * 255);

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

        public static void ChangeEnablePropertyWiFi(bool isEnabled)
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

                MainWindow.CloseConnectionButton.IsEnabled = false;
            }
        }

    }
}
