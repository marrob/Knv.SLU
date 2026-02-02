
namespace Knv.SLU.Discovery
{
    using System;
    using System.Threading;
    using BitwiseSystems;
    using NUnit.Framework;


    [TestFixture]
    internal class N9377A_UnitTest
    {
        const byte SLOT = 0; //0..21
        const byte SLOT3 = 0; //0..21

        #region Load Disconnect
        [TestCase(0, Description = "Slot 0")]
        public void N9377A_LoadDisconnect_StepByStep_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x07, type); //0x07 -> N9377A     

                // Load Disconnect N9377A-K503 - es Oszlopban
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
        public void N9377A_LoadDisconnect_On_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x07, type); //0x07 -> N9377A     

                // Load Disconnect N9377A-K503 - es Oszlopban
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
        public void N9377A_PowerSelect_StepByStep_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x07, type); //0x07 -> N9377A     

                //Power Select K501 - es Oszlopban
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
        public void N9377A_PowerSelect_On_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x07, type); //0x07 -> N9377A     

                // Power Select K501- es Oszlopban
                slu.WriteRegister(0, slot, 0x06, 0xFF);
                slu.WriteRegister(0, slot, 0x07, 0xFF);
                Thread.Sleep(100);
                slu.WriteRegister(0, slot, 0x06, 0x00);
                slu.WriteRegister(0, slot, 0x07, 0x00);
                Thread.Sleep(100);

            }
        }
        #endregion

        #region Load Select
        [TestCase(0, Description = "Slot 0")]
        //[TestCase(3, Description = "Slot 3")]
        public void N9377A_LoadSelect_StepByStep_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x07, type); //0x07 -> N9377A     

                //Load Select N9377A - K502 - es Oszlopban
                for (int i = 0; i < 16; i++)
                {
                    UInt16 mask = (UInt16)(1 << i);
                    byte mask_low = (byte)mask;
                    byte mask_high = (byte)(mask >> 8);

                    slu.WriteRegister(0, slot, 0x08, mask_low);
                    slu.WriteRegister(0, slot, 0x09, mask_high);
                    Thread.Sleep(100);
                    slu.WriteRegister(0, slot, 0x08, 0x00);
                    slu.WriteRegister(0, slot, 0x09, 0x00);
                    Thread.Sleep(100);
                }
            }
        }

        [TestCase(0, Description = "Slot 0")]
        public void N9377A_LoadSelect_On_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x07, type); //0x07 -> N9377A     

                // Load Select N9377A-K502-es Oszlopban
                slu.WriteRegister(0, slot, 0x08, 0xFF);
                slu.WriteRegister(0, slot, 0x09, 0xFF);
                Thread.Sleep(100);
                slu.WriteRegister(0, slot, 0x08, 0x00);
                slu.WriteRegister(0, slot, 0x09, 0x00);
                Thread.Sleep(100);
            }
        }
        #endregion

        #region Current Sense
        [TestCase(0, Description = "Slot 0")]
        public void N9377A_CurrentSenseSelect_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x07, type); //0x07 -> N9377A     

                for (int channel = 0; channel < 16; channel++)
                {
                    slu.WriteRegister(0, slot, 0x03, (byte)(channel + 1));
                    Thread.Sleep(100); // Ide tedd a töréspontot és kattintgasd végig egyenként...
                    slu.WriteRegister(0, slot, 0x03, 0);
                    Thread.Sleep(100);
                }
            }
        }
        #endregion

        [TestCase(0, Description = "Slot 0")]
        public void N9377A_LoadDisconnect_On_And_PowerSelect_On_UnitTest(byte slot)
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {

                int type = slu.ReadRegister(0, slot, 0);
                Assert.AreEqual(0x07, type); //0x07 -> N9377A     

                // --- Load Disconnect N9377A-K503 - es Oszlopban ---
                slu.WriteRegister(0, slot, 0x04, 0xFF);
                slu.WriteRegister(0, slot, 0x05, 0xFF);
                Thread.Sleep(100);


                // --- Power Select K501- es Oszlopban ---
                slu.WriteRegister(0, slot, 0x06, 0xFF);
                slu.WriteRegister(0, slot, 0x07, 0xFF);
                Thread.Sleep(100);

                // --- Load Select On ---
                slu.WriteRegister(0, slot, 0x08, 0xFF);
                slu.WriteRegister(0, slot, 0x09, 0xFF);
                Thread.Sleep(100);

                // --- Select Sense --- 
                byte channel = 4; //ez pontosan a csatorna index
                slu.WriteRegister(0, slot, 0x03, channel);
                Thread.Sleep(100);


                // --- Select Sense OFF --- 
                slu.WriteRegister(0, slot, 0x03, 0);
                Thread.Sleep(100);

                // --- Load Select OFF ---
                slu.WriteRegister(0, slot, 0x08, 0x00);
                slu.WriteRegister(0, slot, 0x09, 0x00);
                Thread.Sleep(100);

                // --- Load Disconnect OFF ---
                slu.WriteRegister(0, slot, 0x04, 0x00);
                slu.WriteRegister(0, slot, 0x05, 0x00);
                Thread.Sleep(100);

                // --- Power Selectt OFF ---
                slu.WriteRegister(0, slot, 0x06, 0x00);
                slu.WriteRegister(0, slot, 0x07, 0x00);
                Thread.Sleep(100);



            }
        }
    }

}
