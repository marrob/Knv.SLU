

namespace Knv.SLU.Discovery
{
    using BitwiseSystems;
    using NUnit.Framework;
    using System;
    using System.Linq;

    [TestFixture]
    internal class SluIo_UnitTes
    {
        string LOG_ROOT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        [Test]
        public void Slu0AttachCheck()
        {
            var devname = QuickUsb.FindModules().ToList<string>();
            Assert.IsTrue(devname.Contains("QUSB-0"));
        }


        [Test]
        public void IsSlu0InstCardIsPresent()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                var cardtype = slu.ReadRegister(0, 21, 0);
                Assert.IsTrue(slu.CardIsPresent(0, 21));
            }
        }

        [Test]
        public void WriteReadRelyRegister()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                byte testvalue = 0x5A;

                slu.WriteRegister(0, 1, 0x04, testvalue); //SLU:0, Slot:1, Load 1/CH1/K34 On, U7179A  konyv B-29
                // The register of relyas are write only... (when you read you will read always 0xFF)
                Assert.AreEqual(0xFF, slu.ReadRegister(0, 1, 0x04));
            }
        }

        [Test]
        public void TestSwitching()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                for (int i = 0; i < 20; i++)
                {
                    slu.WriteRegister(0, 1, 0x04, 0x01); //SLU:0, Slot:1, Load 1/CH1/K34 On, U7179A  konyv B-2
                    System.Threading.Thread.Sleep(5);
                    slu.WriteRegister(0, 1, 0x04, 0x00); //SLU:0, Slot:1, Load 1/CH1/K34 On, U7179A  konyv B-29
                    System.Threading.Thread.Sleep(5);
                }
            }
        }

    }
}
