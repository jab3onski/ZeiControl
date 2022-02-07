using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static byte[] TransformAsBytePacket(int value, byte commandByte)
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
    }
}
