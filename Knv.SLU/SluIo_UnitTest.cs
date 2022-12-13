

namespace Knv.SLU
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using BitwiseSystems;
    using NUnit.Framework;
    using System.Diagnostics;
    using System.Reflection;


    [TestFixture]
    internal class SluIo_UnitTes
    {
        string LOG_ROOT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        [Test]
        public void Slu0AttachCheck()
        {
            var devname = SluIo.GetAttachedNameOfUnits();
            Assert.IsTrue(devname.Contains("QUSB-0"));
        }

        [Test]
        public void SluCount()
        {
            var devname = SluIo.GetAttachedNameOfUnits();
            Assert.IsTrue(devname.Contains("QUSB-0"));
        }


        [Test]
        public void IsSlu0InstCardIsPresent()
        {
            using (var slu = new SluIo())
            {
                slu.Open();
                {
                    var cardtype = slu.ReadRegister(0, 21, 0);
                    Assert.IsTrue(slu.CardIsPresent(0, 21));
                }
            }
        }

        [Test]
        public void WriteReadRelyRegister()
        {
            using (var slu = new SluIo())
            {
                byte testvalue = 0x5A;
                slu.Open();
                {
                    slu.WriteRegister(0, 1, 0x04, testvalue); //SLU:0, Slot:1, Load 1/CH1/K34 On, U7179A  konyv B-29
                    // The register of relyas are write only... (when you read you will read always 0xFF)
                    Assert.AreEqual(0xFF, slu.ReadRegister(0, 1, 0x04));
                }
            }
        }

        [Test]
        public void TestSwitching()
        {
            using (var slu = new SluIo())
            {
                slu.Open();
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
}
