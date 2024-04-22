namespace Knv.SLU.Common
{
    using System;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;
    using System.Globalization;

    internal static class Tools
    {
        /// <summary>
        /// Convert byte array to Log Style string
        /// </summary>
        /// <param name="byteArray">byte array</param>
        /// <returns>string eg.:00 FF AA </returns>
        public static string ConvertByteArrayToLogString(byte[] byteArray)
        {
            string retval = string.Empty;
       
            for (int i = 0 ; i< + byteArray.Length; i++)
              retval += string.Format("{0:X2} ", byteArray[i]);

                if (byteArray.Length > 1)
                    retval = retval.Remove(retval.Length - 1, 1);
            return (retval);
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

        /// <summary>
        /// ByteArray To C-Style String.
        /// </summary>
        /// <param name="data">byte[] data</param>
        /// <returns>0001</returns>
        public static string ConvertByteArrayToString(byte[] data)
        {
            string retval = string.Empty;
            for (int i = 0; i < data.Length; i++)
                retval += string.Format("{0:X2}", data[i]);

            if (data.Length > 1)
                retval = retval.Remove(retval.Length - 1, 1);
            return retval;
        }

        /// <summary>
        /// ByteArray To C-Style String.
        /// </summary>
        /// <param name="data">byte[] data</param>
        /// <returns>0x00,0x01</returns>
        public static string ConvertByteArrayToCStyleString(byte[] data)
        {
            string retval = string.Empty;
            for (int i = 0; i < data.Length; i++)
                retval += string.Format("0x{0:X2},", data[i]);
            //Az utolsó vessző törlése
            if (data.Length > 1)
                retval = retval.Remove(retval.Length - 1, 1);
            return retval;
        }

        public static byte HexaByteStrToByte(string value)
        {
            return byte.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }
        public static int HexaByteStrToInt(string value)
        {
            return int.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        public static UInt32 HexaByteStrToUInt32(string value)
        {
            return UInt32.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        public static DateTime TimeParse (string value)
        {
            try
            {
                return  DateTime.ParseExact(value, "HH:mm", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                Console.WriteLine("{0}: Bad Format", value);
            }
            catch (OverflowException)
            {
                Console.WriteLine("{0}: Overflow", value);
            }
            return DateTime.MinValue;
        }
    }
}
