

namespace Knv.SLU
{
    using System;
    using System.Linq;
    using System.Threading;
    using BitwiseSystems;
    using NUnit.Framework;


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

                int j =   slu.ReadRegister(0, 0, 0);

                /*
                slu.WriteRegister(0, 1, 0x04, 0x01); //SLU:0, Slot:1, Load 1/CH1/K34 On, U7179A  konyv B-2
               // System.Threading.Thread.Sleep(5);
                slu.WriteRegister(0, 1, 0x04, 0x00); //SLU:0, Slot:1, Load 1/CH1/K34 On, U7179A  konyv B-29
               // System.Threading.Thread.Sleep(5);
                */

                for (int i = 0; i < 100; i++)
                {
                    slu.WriteRegister(0, 1, 0x20, 0xFF); //SLU:0, Slot:1, Load 1/CH1/K34 On, U7179A  konyv B-2
                    Thread.Sleep(100);
                    slu.WriteRegister(0, 1, 0x20, 0x00); //SLU:0, Slot:1, Load 1/CH1/K34 On, U7179A  konyv B-2
                    Thread.Sleep(100);

                }
                // System.Threading.Thread.Sleep(5);
                slu.WriteRegister(0, 1, 0x9, 0x00); //SLU:0, Slot:1, Load 1/CH1/K34 On, U7179A  konyv B-29
               // System.Threading.Thread.Sleep(5);

            }
        }


        [Test]
        public void Slu_K125()
        {
            byte status = 0;
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");
            byte slot = 1;


            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, slot, 0);

                Assert.AreEqual(0x43, type); //0x43 -> E8782A 
                                             //   slu.WriteRegister(0, 0, 0x11, 0x01); //SLU:0, Slot:0, ABUS1 & ROW1: K125 -> reg:0x11, data:0x01
                                             //   slu.WriteRegister(0, 0, 0x09, 0x01); //Aux1 - Row1: K925 -> reg:0x09, data:0x01

                /*
                //--- bus control and protection bypass relay for MRLY240314V02-PRT ---
                //Ez rendven van a MRLY240314V02-PRT panelen ez az első TPIC és a sulyozása rendben van
                slu.WriteRegister(0, 1, 0x05, 0x01); //"PB1" - > K1 -> reg:0x04, data:0x01
                slu.WriteRegister(0, 1, 0x05, 0x02); //"PB2" - > K2 -> reg:0x04, data:0x02
                slu.WriteRegister(0, 1, 0x05, 0x04); //"PB3" - > K3 -> reg:0x04, data:0x04
                slu.WriteRegister(0, 1, 0x05, 0x08); //"PB4" - > K4 -> reg:0x04, data:0x08


                // --- MRLY240314V02-PRT --- 
                //ezzel lehet végig menni az összes relémeghajtón 
                for (int i = 5; i < 54 + 5; i++) 
                {
                    slu.WriteRegister(0, 0, (byte)i, 0xFF);
                }
                */

                //--- Eredeti E8782A bus control and protection bypass relay ---
                slu.WriteRegister(0, slot, 0x04, 0x01); //"PB1" - > K1 -> reg:0x04, data:0x01
                slu.WriteRegister(0, slot, 0x04, 0x02); //"PB2" - > K2 -> reg:0x04, data:0x02
                slu.WriteRegister(0, slot, 0x04, 0x04); //"PB3" - > K3 -> reg:0x04, data:0x04
                slu.WriteRegister(0, slot, 0x04, 0x08); //"PB4" - > K4 -> reg:0x04, data:0x08
                slu.WriteRegister(0, slot, 0x04, 0x00); //"AB4" - > K1 -> reg:0x04, data:0x01
                slu.WriteRegister(0, slot, 0x04, 0xFF);

                //--- Read Status ---
                status = slu.ReadRegister(0, 0, 0x02); 
                Console.WriteLine(status); //0x80

                //--- Reset and Set Relay ---
                slu.WriteRegister(0, slot, 0x04, 0x0F); // bekapcsoljuk a bypsass reléket
                slu.WriteRegister(0, slot, 0x02, 0x01); // reset, majd itt törésponttal lehet mérni
                slu.WriteRegister(0, slot, 0x04, 0x0F); // és urja bakpcsoljuk a bypass reléket úgy hogy a reset autó törlődik


                //--- all rellays to open ---
                status = slu.ReadRegister(0, slot, 0x02); //olvasunk egy státuszt
                Console.WriteLine(status); //0x80 - nem busy
                slu.WriteRegister(0, slot, 0x04, 0x0F); //bekapcsoljuk a bypass reléket
                slu.WriteRegister(0, slot, 0x02, 0x20); //minden relét nyitunk
                status = slu.ReadRegister(0, slot, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x80 a státusz nem változott


                //--- GndRly ---
                slu.WriteRegister(0, slot, 0x02, 0x02); //bekapcsoljuk GndRly ez a K1103-as relé
                status = slu.ReadRegister(0, slot, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x82 változott vagyis a GndRly látszik a státuszban
                slu.WriteRegister(0, slot, 0x02, 0x00); //töröljük a GndRly-t
                status = slu.ReadRegister(0, slot, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x80-ra default értékre változott

                //--- DAC1Rly ---
                slu.WriteRegister(0, slot, 0x02, 0x04); //bekapcsoljuk DAC1Rly-t ez a K1101
                status = slu.ReadRegister(0, slot, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x84 változott vagyis a DAC1Rly látszik a státuszban
                slu.WriteRegister(0, slot, 0x02, 0x00); //töröljük a DAC1Rly-t
                status = slu.ReadRegister(0, slot, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x80-ra default értékre változott

                //--- DAC2Rly ---
                slu.WriteRegister(0, slot, 0x02, 0x08); //bekapcsoljuk DAC2Rly-t ez a K1102
                status = slu.ReadRegister(0, slot, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x88 változott vagyis a DAC2Rly látszik a státuszban
                slu.WriteRegister(0, slot, 0x02, 0x00); //töröljük a DAC2Rly-t
                status = slu.ReadRegister(0, slot, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x80-ra default értékre változott
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
