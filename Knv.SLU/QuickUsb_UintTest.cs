
namespace Knv.SLU
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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
    }
}
