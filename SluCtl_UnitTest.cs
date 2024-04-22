

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
    using BitwiseSystems;

    [TestFixture]
    internal class SluCtl_UnitTes
    {
        string LOG_ROOT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        [Test]
        public void Slu0AttachCheck()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");


            using (var slu = new SluCtl(qusb))
            {
                slu.WriteRegister(0, 1, 0x04, 0x01); //SLU:0, Slot:1, Load 1/CH1/K34 On, U7179A  konyv B-2
               // System.Threading.Thread.Sleep(5);
                slu.WriteRegister(0, 1, 0x04, 0x00); //SLU:0, Slot:1, Load 1/CH1/K34 On, U7179A  konyv B-29
               // System.Threading.Thread.Sleep(5);
            }
        }

        [Test]
        public void SluCount()
        {
            var devname = QuickUsb.FindModules().ToList<string>();
            Assert.IsTrue(devname.Contains("QUSB-0"));
        }

    }
}
