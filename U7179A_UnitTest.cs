
namespace Knv.SLU.Discovery
{
    using System;
    using System.Linq;
    using System.Threading;
    using BitwiseSystems;
    using NUnit.Framework;

    [TestFixture]
    internal class U7179A_UnitTest
    {
        const byte SLOT = 0; //0..21
        const byte SLOT3 = 0; //0..21

        #region Load Disconnect
        [TestCase(0, Description = "Slot 0")]
        public void U7179A_LoadSelect_StepByStep_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x20, type); //0x20 -> U7179A     

                // Load Disconnect U7179A-K34 - es Oszlopban
                for (int i = 0; i < 16; i++)
                {
                    UInt16 mask = (UInt16)(1 << i);
                    byte mask_low = (byte)mask;
                    byte mask_high = (byte)(mask >> 8);

                    slu.WriteRegister(0, slot, 0x04, mask_low);
                    slu.WriteRegister(0, slot, 0x05, mask_high);
                    Thread.Sleep(100);// Ide tedd a töréspontot és kattintgasd végig egyenként...
                    slu.WriteRegister(0, slot, 0x04, 0x00);
                    slu.WriteRegister(0, slot, 0x05, 0x00);
                    Thread.Sleep(100);
                }
            }
        }

        [TestCase(0, Description = "Slot 0")]
        public void U7179A_LoadSelect_On_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x20, type); //0x20 -> U7179A     

                // Load Disconnect U7179A-K534 - es Oszlopban
                slu.WriteRegister(0, slot, 0x04, 0xFF);
                slu.WriteRegister(0, slot, 0x05, 0xFF);
                Thread.Sleep(100);
                slu.WriteRegister(0, slot, 0x04, 0x00);
                slu.WriteRegister(0, slot, 0x05, 0x00);
                Thread.Sleep(100);
            }
        }
        #endregion

        #region Power Select
        [TestCase(0, Description = "Slot 0")]
        public void U7179A_PowerSelect_StepByStep_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x20, type); //0x20 -> U7179A     

                // Power Select U7179A-K33- es Oszlopban
                for (int i = 0; i < 16; i++)
                {
                    UInt16 mask = (UInt16)(1 << i);
                    byte mask_low = (byte)mask;
                    byte mask_high = (byte)(mask >> 8);

                    slu.WriteRegister(0, slot, 0x06, mask_low);
                    slu.WriteRegister(0, slot, 0x07, mask_high);
                    Thread.Sleep(100);// Ide tedd a töréspontot és kattintgasd végig egyenként...
                    slu.WriteRegister(0, slot, 0x06, 0x00);
                    slu.WriteRegister(0, slot, 0x07, 0x00);
                    Thread.Sleep(100);
                }
            }
        }

        [TestCase(0, Description = "Slot 0")]
        public void U7179A_PowerSelect_On_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x20, type); //0x20 -> U7179A     

                // Load Disconnect U7179A-K33 - es Oszlopban
                slu.WriteRegister(0, slot, 0x06, 0xFF);
                slu.WriteRegister(0, slot, 0x07, 0xFF);
                Thread.Sleep(100);
                slu.WriteRegister(0, slot, 0x06, 0x00);
                slu.WriteRegister(0, slot, 0x07, 0x00);
                Thread.Sleep(100);
            }
        }
        #endregion

        #region Current Sense
        [TestCase(0, Description = "Slot 0")]
        public void U7179_CurrentSenseSelect_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x20, type); //0x20 -> U7179     

                for (int channel = 0; channel < 16; channel++)
                {
                    slu.WriteRegister(0, slot, 0x03, (byte)(channel + 1));
                    Thread.Sleep(100);// Ide tedd a töréspontot és kattintgasd végig egyenként...
                    slu.WriteRegister(0, slot, 0x03, 0);
                    Thread.Sleep(100);
                }
            }
        }
        #endregion
    }
}