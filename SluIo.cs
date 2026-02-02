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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    //C:\Program Files (x86)\Bitwise Systems\QuickUsb\Library\Assembly
    using BitwiseSystems;
    using Common;

    //[Obsolete("Túlságosan bele lett integrával a Quick USB és a Platform logika, ezért nehéz benchként használni")]
    public class SluIo : IDisposable
    {
        /// <summary>
        /// Type of Card example: 0x32 -> E6198B This the SLU
        /// </summary>
        public const byte REG_CARD_TYPE = 0x00;

        /// <summary>
        /// Az SLU unit nak is van fixture id-ja, ez az SLU cime pl SLU0, SLU1...stb.
        /// </summary>
        public const byte REG_FIXTURE_ID = 0x01;

        readonly Dictionary<int, QuickUsb> _quickUsbs = new Dictionary<int, QuickUsb>();
        readonly List<string> _logLines = new List<string>();
        public Dictionary<int, string> CardTypes = new Dictionary<int, string>();
        bool _disposed = false;

        public SluIo()
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
        }

        public void Open()
        {
            /*
             * A QUSB-0 nem feltetlenul az SLU0-hoz tartozik!!!
             * A QUSB-n-tol meg kell kerdezni hogy milyen cimhez tartozik... de ez a dokumentcioval ellentetben nem mukodik.
             * Ha a QUSB-x-en kikuldott slu type lekerdezes ertek nem 0xFF akkor az a slu az.
             */
            var devnames = QuickUsb.FindModules();//QUSB-0, QUSB-1...
            for (byte unit = 0; unit < devnames.Length; unit++)
            {
                var devname = devnames[unit];
                try
                {
                    var qusb = new QuickUsb();


                    if (qusb.Open(devname))
                    {
                        LogWriteLine($"SluIo.Open {devname} successful.");
                    }
                    else
                    {
                        LogWriteLine($"SluIo.Open {devname} failed.");
                    }

                    qusb.WriteSetting(QuickUsb.Setting.SETTING_WORDWIDE, 0x0000); //0x01
                    qusb.WriteSetting(QuickUsb.Setting.SETTING_FIFO_CONFIG, 0x00A2); //0x03
                    qusb.WriteSetting(QuickUsb.Setting.SETTING_PORTA, 0xFFFF); //0x09
                    qusb.WriteSetting(QuickUsb.Setting.SETTING_PORTC, 0xFFFF); //0x0B
                    qusb.WriteSetting(QuickUsb.Setting.SETTING_PORTD, 0xFFFF); //0x0C
                    qusb.WriteSetting(QuickUsb.Setting.SETTING_PORTA, 0xFFFE); //0x09
                    System.Threading.Thread.Sleep(200);
                    qusb.WriteSetting(QuickUsb.Setting.SETTING_PORTA, 0xFFFF); //0x09

                    //Beprobalkozik az kártya cimekkel, ahol jon valsz az ervenyes SLU cim
                    { 
                        for (byte slot = 0; slot < 21; slot++)
                        {
                            var slutype = ReadRegister(qusb, unit, slot, REG_CARD_TYPE);
                            if (slutype != 0xFF)
                            {
                                _quickUsbs.Add(unit, qusb);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWriteLine($"SluIo.Open Error {devname} {ex.Message}.");
                    throw ex;
                }
            }
        }
           

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAttachedNameOfUnits()
        {
            var nameOfRackes = QuickUsb.FindModules().ToList<string>();
            return nameOfRackes;       
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
            var code = ReadRegister(unit, slot, REG_CARD_TYPE);
            string type = string.Empty;
            if (!CardTypes.TryGetValue(code, out type))
                type = "UNKNOWN";
            return type;
        }

        /// <summary>
        /// Card Type is valu of REG_CARD_TYPE register
        /// example: 0x01
        /// </summary>
        /// <param name="unit">SLU0:0 SLU1:1</param>
        /// <param name="slot">Index of slots 1..21. The Slot 0 is the SLU itself.</param>
        /// <returns> 0x01 </returns>
        public byte GetCardType(byte unit, byte slot)
        {
            return ReadRegister(unit, slot, REG_CARD_TYPE);
        }

        /// <summary>
        /// Check card presence
        /// </summary>
        /// <param name="unit">SLU0:0 SLU1:1</param>
        /// <param name="slot">Index of slots 1..21. The Slot 0 is the SLU itself.</param>
        /// <returns>It is true if slot is not empty.</returns>
        public bool CardIsPresent(byte unit, byte slot)
        {
            var code = ReadRegister(unit, slot, REG_CARD_TYPE);
            return code != 0xFF;
        }

        /// <summary>
        /// Write a card Register
        /// </summary>
        /// <param name="unit">SLU0:0 SLU1:1</param>
        /// <param name="slot">Indes of slots 1..21. The Slot 0 is the SLU itself. </param>
        /// <param name="register">Register Address of Card</param>
        /// <param name="data">Write Value of the register</param>
        public void WriteRegister(byte unit, byte slot, byte register, byte data)
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

            var bytes2write = new byte[] { 0x0A, (byte)((unit << 5) | slot), register, data, 0xFF, 0xFF, 0xFF, 0xFF };
            LogWriteLine($"WrReg Tx: {Tools.ConvertByteArrayToLogString(bytes2write)}");
            uint length = (uint)bytes2write.Length;
            QuickUsb qusb;
            if (!_quickUsbs.TryGetValue(unit, out qusb))
                throw new Exception($"The requested SLU address {unit:X2} invalid");
            qusb.WriteDataEx(bytes2write, ref length, QuickUsb.DataFlags.None);
        }

        /// <summary>
        /// Read a Register
        /// </summary>
        /// <param name="unit">SLU0:0 SLU1:1</param>
        /// <param name="slot">Indes of slots 1..21. The Slot 0 is the SLU itself.</param>
        /// <param name="register">Register Address of the Card</param>
        /// <returns>Value of the register</returns>
        public byte ReadRegister(byte unit, byte slot, byte register)
        {
            byte retval = 0;
            /* Example
             * 
             * SLU1 Slot21 Register:0
             * 0B 35 00 FF FF FF FF FF
             * FF 00 00 43 43 43 43 43\
             * 
             * Szia Robi! Mizu? kutyagumi
             * Ne légy troll!!!!!!!!!!!!!!!!!! Te Fasz
             * 
             */

            QuickUsb qusb;
            if (!_quickUsbs.TryGetValue(unit, out qusb))
                throw new Exception($"The requested SLU address {unit:X2} invalid");

            var bytes2write = new byte[] { 0x0B, (byte)((unit << 5) | slot), register, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
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

        /// <summary>
        /// Csak az SLU-hoz hasznalahto
        /// </summary>
        /// <param name="qusb"></param>
        /// <param name="register"></param>
        /// <returns></returns>
        byte ReadRegister(QuickUsb qusb, byte unit, byte slot, byte register)
        {
            byte retval = 0;
            var bytes2write = new byte[] { 0x0B, (byte)((unit << 5) | slot), register, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
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

                foreach (KeyValuePair<int, QuickUsb> qusb in _quickUsbs)
                {
                    if (qusb.Value.IsOpened)
                    {
                        qusb.Value.Close();
                    } 
                }   
                _quickUsbs.Clear();
            }
            _disposed = true;
            LogWriteLine($"SluIo.{nameof(Dispose)}.End");
        }
    }
}
