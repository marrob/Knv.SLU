
namespace Knv.SLU
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Runtime.InteropServices;
    //C:\Program Files (x86)\Bitwise Systems\QuickUsb\Library\Assembly
    using BitwiseSystems;
    using Common;

    public class SluIo : IDisposable
    {
        readonly List<QuickUsb> _quickUsbs = new List<QuickUsb>();
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
            CardTypes.Add(0x32, "E6198B");
            CardTypes.Add(0x43, "E8782A");
            CardTypes.Add(0x47, "E8783A");
        }

        public void Open()
        {

            var devnames = QuickUsb.FindModules();//QUSB-0, QUSB-1...
            Array.Sort(devnames, StringComparer.CurrentCultureIgnoreCase);
            for (int unit = 0; unit < devnames.Length; unit++)
            {
                var devname = devnames[unit];
                try
                {
                    var qusb = new QuickUsb();
                    _quickUsbs.Add(qusb);

                    if (qusb.Open(devname))
                    {
                        LogWriteLine($"SluIo.Open {devname} successful.");
                    }
                    else
                    {
                        LogWriteLine($"SluIo.Open {devname} failed.");
                    }

                    var x = ReadRegister((byte)unit, 0, 0);
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
        /// Write a card Register
        /// </summary>
        /// <param name="unit">SLU0:0 SLU1:1</param>
        /// <param name="slot">Indes of slots 1..21 </param>
        /// <param name="register">Register of Card</param>
        /// <param name="data">Data of Register</param>
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

            var bytes2write = new byte[] { 0x0A, (byte)((unit << 5)| slot), register, data, 0xFF, 0xFF, 0xFF, 0xFF };
            LogWriteLine($"WrReg Tx: {Tools.ConvertByteArrayToLogString(bytes2write)}");
            uint length = (uint)bytes2write.Length;
            _quickUsbs[unit].WriteDataEx(bytes2write, ref length, QuickUsb.DataFlags.None);

        }


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
             * Ne légy troll!!!!!!!!!!!!!!!!!!
             * 
             */

            var bytes2write = new byte[] { 0x0B, (byte)((unit << 5) | slot), register, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            LogWriteLine($"RdReg Tx: {Tools.ConvertByteArrayToLogString(bytes2write)}");
            uint wrlength = (uint)bytes2write.Length;
            _quickUsbs[unit].WriteDataEx(bytes2write, ref wrlength, QuickUsb.DataFlags.None);

            System.Threading.Thread.Sleep(10);

            _quickUsbs[unit].WriteCommand(0x08, new byte[] { }, 0);

            byte[] readBytes = new byte[8];
            uint rdLength = (uint)readBytes.Length;
            _quickUsbs[unit].ReadData(readBytes, ref rdLength);
            LogWriteLine($"RdReg Rx: {Tools.ConvertByteArrayToLogString(readBytes)}");
            retval = readBytes[3];
            return retval;
        }


        private void SluInit()
        { 
        
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
                for (int i = 0; i < _quickUsbs.Count; i++)
                {
                    var qusb = _quickUsbs[i];
                    if (qusb.IsOpened)
                    {
                        qusb.Close();
                        qusb = null;
                    }
                    _quickUsbs.Clear();
                }
            }
            _disposed = true;
            LogWriteLine($"SluIo.{nameof(Dispose)}.End");
        }
    }
}
