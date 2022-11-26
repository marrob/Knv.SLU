
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
    internal class SluIo_UnitTes
    {

        string LOG_ROOT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        [Test]
        public void Slu0AttachCheck()
        {
            var devname = SluIo.GetAttachedNameOfRacks();
            Assert.IsTrue(devname.Contains("QUSB-0"));
        }

        [Test]
        public void SluCount()
        {
            var devname = SluIo.GetAttachedNameOfRacks();
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
                    Assert.AreEqual(0x43, cardtype);
                }
            }
        }

        [Test]
        public void SortOfSluNames()
        {
            string[] names = new string[] { "QUSB-2", "QUSB-1", "QUSB-0", "QUSB-3" };
            Array.Sort(names, StringComparer.CurrentCultureIgnoreCase);
            Assert.AreEqual(new string[] { "QUSB-0", "QUSB-1", "QUSB-2", "QUSB-3" }, names);
        }
    }
}
