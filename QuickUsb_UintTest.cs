
namespace Knv.SLU.Discovery
{
    using System;
    using BitwiseSystems;
    using NUnit.Framework;

    [TestFixture]
    internal class QuickUsb_UintTest
    {
        [Test]
        public void TestForUsbProtocolAnlyser()
        {
            var qu = new QuickUsb();
            qu.Open("QUSB-1");
            qu.WriteCommand(0x0A, new byte[] { 0xAA }, 1);
            var test = new byte[] { 0x01, 0x02, 0x03 };
            unsafe
            {
                fixed (byte* p = test)
                {
                    IntPtr xp = (IntPtr)p;
                    qu.WriteData(xp, (uint)test.Length);
                }

                Console.WriteLine(qu.LastError().ToString());
                qu.Close();
            }
        }

        [Test]
        public void QuickUsbReadSettings()
        {
            var qu = new QuickUsb();
            qu.Open("QUSB-1");

            int i = 0;
            foreach (QuickUsb.Setting param in Enum.GetValues(typeof(QuickUsb.Setting)))
            {
                ushort value;
                if ((i++ % 2) != 0)
                {
                    qu.ReadSetting(param, out value);
                    string line = $"{Enum.GetName(typeof(QuickUsb.Setting), param)}:{value:X4}";
                    Console.WriteLine(line);
                }
            }
            qu.Close();
        }

        [Test]
        public void SluRequesResponse()
        {
            var qu = new QuickUsb();
            qu.Open("QUSB-1");

            byte unit = 1;
            byte slot = 2;
            byte register = 0;

            var bytes2write = new byte[] { 0x0B, (byte)((unit << 5) | slot), register, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            uint wrlength = (uint)bytes2write.Length;

            qu.WriteDataEx(bytes2write, ref wrlength, QuickUsb.DataFlags.None);
            qu.WriteCommand(0x00, new byte[] { 0x08, 0x00, 0x00, 0x00 }, 4);

            byte[] readBytes = new byte[8];
            uint rdLength = (uint)readBytes.Length;
            qu.ReadData(readBytes, ref rdLength);

            qu.Close();
        }
    }
}
