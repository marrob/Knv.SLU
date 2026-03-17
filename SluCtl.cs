/*
 * A QuickUSB
 * https://github.com/BitwiseSystems/QuickUSB
 * 
 * A QuckUSB az SLU- kártyáival az alábbi jelekke kommunikál:
 * 1.Card Reset
 * 2.Card Select
 * 3.Read/Write
 * 4.Address bus 8bit
 * 5.Data bus 8bit
 * 6.Strobe
 * 
 * 
 * Szabalyok:
 * 
 * Az SLU cimek 0-tol kell hogy kezodjenek es nem lehet kzottuk szunet
 * Ervenyes SLU citaromany: 0,1,2 3
 * Erventelen SLU cimtartomany:0,2,3,4
 * 
 * 
 */

namespace Knv.SLU.Discovery
{
    //C:\Program Files (x86)\Bitwise Systems\QuickUsb\Library\Assembly
    using BitwiseSystems;
    using Common;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class SluCtl : IDisposable
    {
        public const byte ADDR_TYPE_REG = 0x00;
        public const byte ADDR_CFG_REG = 0x01;
        public const byte ADDR_STATUS_REG = 0x02;
        public const byte ADDR_READONLY_1_REG = 0x03;
        public const byte ADDR_PROTECTION_REG = 0x04;
        public const byte ADDR_READONLY_2_REG = 0x05;

        public const byte ADDR_ADC_RESULT_B1_REG = 0xE6;
        public const byte ADDR_ADC_RESULT_B2_REG = 0xE7;
        public const byte ADDR_ADC_RESULT_B3_REG = 0xE8;
        public const byte ADDR_ADC_RESULT_B4_REG = 0xE9;
        public const byte ADDR_ADC_RESULT_B5_REG = 0xEA;
        public const byte ADDR_ADC_RESULT_B6_REG = 0xEB;
        public const byte ADDR_ADC_RESULT_B7_REG = 0xEC;
        public const byte ADDR_ADC_RESULT_B8_REG = 0xED;

        public const byte ADDR_UID_B1_REG = 0xF3;
        public const byte ADDR_UID_B2_REG = 0xF4;
        public const byte ADDR_UID_B3_REG = 0xF5;
        public const byte ADDR_UID_B4_REG = 0xF6;
        public const byte ADDR_UID_B5_REG = 0xF7;
        public const byte ADDR_UID_B6_REG = 0xF8;
        public const byte ADDR_UID_B7_REG = 0xF9;
        public const byte ADDR_UID_B8_REG = 0xFA;

        public const byte ADDR_VERSION_B1_REG = 0xFB;
        public const byte ADDR_VERSION_B2_REG = 0xFC;
        public const byte ADDR_VERSION_B3_REG = 0xFD;
        public const byte ADDR_VERSION_B4_REG = 0xFE;
        public const byte ADDR_INV_TYPE_REG = 0xFF;

        public const int MAX_CARD_COUNT_IN_RACK = 21; //+1 SLU itself

        /// <summary>
        /// Az SLU unit nak is van fixture id-ja, ez az SLU cime pl SLU0, SLU1...stb.
        /// </summary>
        public const byte REG_FIXTURE_ID = 0x01;
        readonly QuickUsb _qusb;
        readonly List<string> _logLines = new List<string>();
        public Dictionary<int, string> CardTypes = new Dictionary<int, string>();
        bool _disposed = false;

        public SluCtl(string qusbDeviceName) 
        {
            CardTypes.Add(0x01, "E6175A");
            CardTypes.Add(0x02, "E6176A");
            CardTypes.Add(0x03, "E6177A");
            CardTypes.Add(0x04, "E6178B");
            CardTypes.Add(0x05, "N9378A");
            CardTypes.Add(0x06, "N9379");
            CardTypes.Add(0x07, "N9377A");
            CardTypes.Add(0x0A, "E8792A");
            CardTypes.Add(0x0B, "E8793A");
            CardTypes.Add(0X14, "E8794A");
            CardTypes.Add(0x18, "U7177A");
            CardTypes.Add(0x19, "U7178A");
            CardTypes.Add(0x20, "U7179A");
            CardTypes.Add(0x1E, "E6198A"); //"old" SLU
            CardTypes.Add(0x32, "E6198B"); //Ez mindig a Slot-0-as cimen van, ez az SLU
            CardTypes.Add(0x43, "E8782A");
            CardTypes.Add(0x47, "E8783A");

            var qusb = new QuickUsb(); 
            qusb.Open(qusbDeviceName);
            qusb.WriteSetting(QuickUsb.Setting.SETTING_WORDWIDE, 0x0000); //0x01
            qusb.WriteSetting(QuickUsb.Setting.SETTING_FIFO_CONFIG, 0x00A2); //0x03
            qusb.WriteSetting(QuickUsb.Setting.SETTING_PORTA, 0xFFFF); //0x09
            qusb.WriteSetting(QuickUsb.Setting.SETTING_PORTC, 0xFFFF); //0x0B
            qusb.WriteSetting(QuickUsb.Setting.SETTING_PORTD, 0xFFFF); //0x0C
            qusb.WriteSetting(QuickUsb.Setting.SETTING_PORTA, 0xFFFE); //0x09
            System.Threading.Thread.Sleep(200);
            qusb.WriteSetting(QuickUsb.Setting.SETTING_PORTA, 0xFFFF); //0x09
            _qusb = qusb;
        }

        /// <summary>
        /// Card Model Number is Name of type code...
        /// example: E6175A -> 0x01
        /// </summary>
        /// <param name="unit">SLU0:0 SLU1:1</param>
        /// <param name="slot">Index of slots 1..21. The Slot 0 is the SLU itself.</param>
        /// <returns> E6198B </returns>
        public string GetCardModel(byte unit, byte slot)
        {
            var code = ReadRegister(unit, slot, ADDR_TYPE_REG);
            string type = string.Empty;
            if (!CardTypes.TryGetValue(code, out type))
                type = "UNKNOWN";
            return type;
        }

        /// <summary>
        /// Card Type is valu of ADDR_TYPE_REG register
        /// example: 0x01
        /// </summary>
        /// <param name="unit">SLU0:0 SLU1:1</param>
        /// <param name="slot">Index of slots 1..21. The Slot 0 is the SLU itself.</param>
        /// <returns> 0x01 </returns>
        public byte GetCardType(byte unit, byte slot)
        {
            return ReadRegister(unit, slot, ADDR_TYPE_REG);
        }

        /// <summary>
        /// Check card presence
        /// </summary>
        /// <param name="unit">SLU0:0 SLU1:1</param>
        /// <param name="slot">Index of slots 1..21. The Slot 0 is the SLU itself.</param>
        /// <returns>It is true if slot is not empty.</returns>
        public bool CardIsPresent(byte unit, byte slot)
        {
            var code = ReadRegister(unit, slot, ADDR_TYPE_REG);
            return code != 0xFF;
        }

        /// <summary>
        /// Write a card Register
        /// </summary>
        /// <param name="slu">SLU0:0 SLU1:1</param>
        /// <param name="slot">Indes of slots 1..21. The Slot 0 is the SLU itself. </param>
        /// <param name="register">Register Address of Card</param>
        /// <param name="data">Write Value of the register</param>
        public void WriteRegister(byte slu, byte slot, byte register, byte data)
        {
            /*
             * Example:
             * 
             * SLU0 Slot21 K925 Row1-Aux1 E8782A close-open
             * 0A 15 09 01 FF FF FF FF
             * 0A 15 09 00 FF FF FF FF
             * 
             * SLU0 Slot21 K125 Abus1-Row1 E8782A close-open Page B-75
             * 0A 15 11 01 FF FF FF FF
             * 0A 15 11 00 FF FF FF FF
             * 
             * SLU1 Slot21 K125 Abus1-Row1 E8782A close-open
             * 0A 35 11 01 FF FF FF FF
             * 0A 35 11 00 FF FF FF FF
             * 
             * SLU1 Slot8 K25 Load1.1-Chan1 N9379A close-open
             * 0A 28 06 01 FF FF FF FF
             * 0A 28 06 00 FF FF FF FF
             * 
             */

            var bytes2write = new byte[] { 0x0A, (byte)((slu << 5) | slot), register, data, 0xFF, 0xFF, 0xFF, 0xFF };
            LogWriteLine($"WrReg Tx: {Tools.ConvertByteArrayToLogString(bytes2write)}");
            uint length = (uint)bytes2write.Length;
            _qusb.WriteDataEx(bytes2write, ref length, QuickUsb.DataFlags.None);
        }

        /// <summary>
        /// Read a Register
        /// </summary>
        /// <param name="slu">SLU0:0 SLU1:1</param>
        /// <param name="slot">Indes of slots 1..21. The Slot 0 is the SLU itself.</param>
        /// <param name="register">Register Address of the Card</param>
        /// <returns>Value of the register</returns>
        public byte ReadRegister(byte slu, byte slot, byte register)
        {
            byte retval = 0;
            /* Example
             * 
             * SLU1 Slot21 Register:0
             * 0B 35 00 FF FF FF FF FF
             * FF 00 00 43 43 43 43 43\
             * 
             */

            var bytes2write = new byte[] { 0x0B, (byte)((slu << 5) | slot), register, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            LogWriteLine($"RdReg Tx: {Tools.ConvertByteArrayToLogString(bytes2write)}");
            uint wrlength = (uint)bytes2write.Length;

            _qusb.WriteDataEx(bytes2write, ref wrlength, QuickUsb.DataFlags.None);
            System.Threading.Thread.Sleep(10);

            byte[] readBytes = new byte[8];
            uint rdLength = (uint)readBytes.Length;
            
            _qusb.ReadData(readBytes, ref rdLength);
            LogWriteLine($"RdReg Rx: {Tools.ConvertByteArrayToLogString(readBytes)}");
            retval = readBytes[7];
            return retval;
        }

        /// <summary>
        /// Csak az SLU-hoz hasznalahto
        /// </summary>
        /// <param name="qusb"></param>
        /// <param name="register"></param>
        /// <returns></returns>
        byte ReadRegister(QuickUsb qusb, byte unit, byte register)
        {
            byte retval = 0;
            var bytes2write = new byte[] { 0x0B, (byte)(unit << 5), register, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            LogWriteLine($"RdReg Tx: {Tools.ConvertByteArrayToLogString(bytes2write)}");
            uint wrlength = (uint)bytes2write.Length;
            qusb.WriteDataEx(bytes2write, ref wrlength, QuickUsb.DataFlags.None);

            System.Threading.Thread.Sleep(10);
            byte[] readBytes = new byte[8];
            uint rdLength = (uint)readBytes.Length;
            qusb.ReadData(readBytes, ref rdLength);
            LogWriteLine($"RdReg Rx: {Tools.ConvertByteArrayToLogString(readBytes)}");
            retval = readBytes[3];
            return retval;
        }

        public bool IsMRLY240314Card(byte unit, byte slot)
        {
            byte type = ReadRegister(unit, slot, register: ADDR_TYPE_REG);
            byte invert_type = (byte)~ReadRegister(unit, slot, register: ADDR_INV_TYPE_REG);

            if (type == invert_type)
                return true;
            else
                return false;
        }

        public string GetUid(byte unit, byte slot) 
        {
            byte[] uidBytes = new byte[8];
            uidBytes[0] = ReadRegister(unit, slot, register: ADDR_UID_B1_REG);
            uidBytes[1] = ReadRegister(unit, slot, register: ADDR_UID_B2_REG);
            uidBytes[2] = ReadRegister(unit, slot, register: ADDR_UID_B3_REG);
            uidBytes[3] = ReadRegister(unit, slot, register: ADDR_UID_B4_REG);
            uidBytes[4] = ReadRegister(unit, slot, register: ADDR_UID_B5_REG);
            uidBytes[5] = ReadRegister(unit, slot, register: ADDR_UID_B6_REG);
            uidBytes[6] = ReadRegister(unit, slot, register: ADDR_UID_B7_REG);
            uidBytes[7] = ReadRegister(unit, slot, register: ADDR_UID_B8_REG);
            string uid = Encoding.ASCII.GetString(uidBytes).Trim('\0');
            return uid;
        }

        public double MeasureResistance(byte unit, byte slot)
        {
            byte[] bytes = new byte[8];
            bytes[0] = ReadRegister(unit, slot, register: ADDR_ADC_RESULT_B1_REG);
            bytes[1] = ReadRegister(unit, slot, register: ADDR_ADC_RESULT_B2_REG);
            bytes[2] = ReadRegister(unit, slot, register: ADDR_ADC_RESULT_B3_REG);
            bytes[3] = ReadRegister(unit, slot, register: ADDR_ADC_RESULT_B4_REG);
            bytes[4] = ReadRegister(unit, slot, register: ADDR_ADC_RESULT_B5_REG);
            bytes[5] = ReadRegister(unit, slot, register: ADDR_ADC_RESULT_B6_REG);
            bytes[6] = ReadRegister(unit, slot, register: ADDR_ADC_RESULT_B7_REG);
            bytes[7] = ReadRegister(unit, slot, register: ADDR_ADC_RESULT_B8_REG);
            double ohms = BitConverter.ToDouble(bytes, startIndex: 0);
            return ohms;
        }


        #region Logging
        public void LogWriteLine(string line)
        {
            var dt = DateTime.Now;
            _logLines.Add($"{dt:yyyy}.{dt:MM}.{dt:dd} {dt:HH}:{dt:mm}:{dt:ss}:{dt:fff} {line}");
        }

        public void LogSave(string directory)
        {
            LogSave(directory, "");
        }

        public void LogSave(string directory, string prefix)
        {
            var dt = DateTime.Now;
            var fileName = $"{prefix}_{dt:yyyy}{dt:MM}{dt:dd}_{dt:HH}{dt:mm}{dt:ss}.log";
            using (var file = new System.IO.StreamWriter($"{directory}\\{fileName}", true, Encoding.ASCII))
                _logLines.ForEach(file.WriteLine);
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            LogWriteLine($"SluIo.{nameof(Dispose)}.Begin");
            if (_disposed)
                return;

            if (disposing)
            {
                if (_qusb != null)
                    _qusb.Close();
            }
            _disposed = true;
            LogWriteLine($"SluIo.{nameof(Dispose)}.End");
        }
    }
}
