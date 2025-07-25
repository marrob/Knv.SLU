
namespace Knv.SLU
{
    using System;
    using System.Linq;
    using System.Threading;
    using BitwiseSystems;
    using NUnit.Framework;


    [TestFixture]
    internal class MatirxCard_UnitTest
    {
        const byte SLOT = 0; //0..21

        [Test]
        public void SluCount()
        {
            var devname = QuickUsb.FindModules().ToList<string>();
            Assert.IsTrue(devname.Contains("QUSB-0"));
        }


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
            
            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);

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
                slu.WriteRegister(0, SLOT, 0x04, 0x01); //"PB1" - > K1 -> reg:0x04, data:0x01
                slu.WriteRegister(0, SLOT, 0x04, 0x02); //"PB2" - > K2 -> reg:0x04, data:0x02
                slu.WriteRegister(0, SLOT, 0x04, 0x04); //"PB3" - > K3 -> reg:0x04, data:0x04
                slu.WriteRegister(0, SLOT, 0x04, 0x08); //"PB4" - > K4 -> reg:0x04, data:0x08
                slu.WriteRegister(0, SLOT, 0x04, 0x00); //"AB4" - > K1 -> reg:0x04, data:0x01
                slu.WriteRegister(0, SLOT, 0x04, 0xFF);

                //--- Read Status ---
                status = slu.ReadRegister(0, 0, 0x02); 
                Console.WriteLine(status); //0x80

                //--- Reset and Set Relay ---
                slu.WriteRegister(0, SLOT, 0x04, 0x0F); // bekapcsoljuk a bypsass reléket
                slu.WriteRegister(0, SLOT, 0x02, 0x01); // reset, majd itt törésponttal lehet mérni
                slu.WriteRegister(0, SLOT, 0x04, 0x0F); // és urja bakpcsoljuk a bypass reléket úgy hogy a reset autó törlődik


                //--- all rellays to open ---
                status = slu.ReadRegister(0, SLOT, 0x02); //olvasunk egy státuszt
                Console.WriteLine(status); //0x80 - nem busy
                slu.WriteRegister(0, SLOT, 0x04, 0x0F); //bekapcsoljuk a bypass reléket
                slu.WriteRegister(0, SLOT, 0x02, 0x20); //minden relét nyitunk
                status = slu.ReadRegister(0, SLOT, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x80 a státusz nem változott


                //--- GndRly ---
                slu.WriteRegister(0, SLOT, 0x02, 0x02); //bekapcsoljuk GndRly ez a K1103-as relé
                status = slu.ReadRegister(0, SLOT, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x82 változott vagyis a GndRly látszik a státuszban
                slu.WriteRegister(0, SLOT, 0x02, 0x00); //töröljük a GndRly-t
                status = slu.ReadRegister(0, SLOT, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x80-ra default értékre változott

                //--- DAC1Rly ---
                slu.WriteRegister(0, SLOT, 0x02, 0x04); //bekapcsoljuk DAC1Rly-t ez a K1101
                status = slu.ReadRegister(0, SLOT, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x84 változott vagyis a DAC1Rly látszik a státuszban
                slu.WriteRegister(0, SLOT, 0x02, 0x00); //töröljük a DAC1Rly-t
                status = slu.ReadRegister(0, SLOT, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x80-ra default értékre változott

                //--- DAC2Rly ---
                slu.WriteRegister(0, SLOT, 0x02, 0x08); //bekapcsoljuk DAC2Rly-t ez a K1102
                status = slu.ReadRegister(0, SLOT, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x88 változott vagyis a DAC2Rly látszik a státuszban
                slu.WriteRegister(0, SLOT, 0x02, 0x00); //töröljük a DAC2Rly-t
                status = slu.ReadRegister(0, SLOT, 0x02);//olvasunk egy státuszt
                Console.WriteLine(status); //0x80-ra default értékre változott
            }
        }


        [Test]
        public void Bypass()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 
  
                //--- A Disconnect relék írása hatástalan mivel mindig meg vannak húzva ---
                slu.WriteRegister(0, SLOT, 0x04, 0xF0);
                slu.WriteRegister(0, SLOT, 0x04, 0x00);

                //--- Bypass relék kapcsolhatóak ---
                slu.WriteRegister(0, SLOT, 0x04, 0x0F);
                slu.WriteRegister(0, SLOT, 0x04, 0x00);

                //--- K1 - ABUS1 Bypass On ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01);

            }

        }

        [Test]
        public void TestPoints()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- K1 - ABUS1 Bypass On ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01);
                slu.WriteRegister(0, SLOT, 0x04, 0x00);

                //--- UUT_I1 - Ez az első relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x06, 0x01);

                //--- AB4_R40 - Az utolsó relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x2D, 0x80);


            }
        }

        [Test]
        public void DisconnectRelayOnOff()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //Ez a bit NEM DOKUMENTÁLT!!!
                slu.WriteRegister(0, SLOT, 0x02, 0x40); // Kikapcsolja a disconnect reléket 
                slu.WriteRegister(0, SLOT, 0x02, 0x00); // Bekapcsolja a disconnect reléket
            }
        }

        [Test]
        public void DisconnectControl()
        {
            byte status = 0;
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- alapból be vannak kapcsolva a discconnect relék

                //--- Bekpacsolja a disconnect reléket az engedélyezés után, de azok már bevannak így nem kattan relé ---
                slu.WriteRegister(0, SLOT, 0x04, 0xF0);

                //--- Status olvasása ---
                status = slu.ReadRegister(0, SLOT, 0x02);  //0x80

                //--- DCE Engedélyezi a Disconnect relék vezérlést ---
                slu.WriteRegister(0, SLOT, 0x02, 0x40);

                //--- Status olvasása ---
                status = slu.ReadRegister(0, SLOT, 0x02);  //0xC0

                //--- Kikapcsolja a disconnect reléket ---
                slu.WriteRegister(0, SLOT, 0x04, 0x00);

                //--- OAR - Open all relays ---
                //Bekapcsolódnak a Disconnect relék mivel a DCE alaphelyzetbe áll
                slu.WriteRegister(0, SLOT, 0x02, 0x20);

                //--- Status olvasása ---
                status = slu.ReadRegister(0, SLOT, 0x02);  //0x80

                //--- Kikapcsolná a disconnect reléket, de OAR miatt itt már nem engedélyezett ---
                slu.WriteRegister(0, SLOT, 0x04, 0x00);

                //--- AB1_R1 ---
                slu.WriteRegister(0, SLOT, 0x11, 0x01);

                //--- ABUS1 Bypass K1 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01);
            }
        }




        [Test]
        public void OpenAllRealy()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- UUT_I1 - Ez az első relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x06, 0x01);

                //--- AB1_R1 ---
                slu.WriteRegister(0, SLOT, 0x11, 0x01);

                //--- AB4_R40 - Az utolsó relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x2D, 0x80);

                //--- ABUS1 Bypass K1 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01);
                
                //--- Open All Relay - AOR ---
                slu.WriteRegister(0, SLOT, 0x02, 0x20); 
            }
        }

        [Test]
        public void Reset()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- UUT_I1 - Ez az első relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x06, 0x01);

                //--- AB1_R1 ---
                slu.WriteRegister(0, SLOT, 0x11, 0x01);

                //--- AB4_R40 - Az utolsó relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x2D, 0x80);

                //--- ABUS1 Bypass K1 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01);
                
                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01); 
            }
        }


        [Test]
        public void DAC()
        {
            byte status = 0;
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 


                //--- UUT_I1 - Ez az első relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x06, 0x01);

                //--- Status olvasása ---
                status = slu.ReadRegister(0, SLOT, 0x02);  //0x80

                //--- DAC1 Relé bekapcsolása ---
                slu.WriteRegister(0, SLOT, 0x02, 0x04);

                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01);

                //--- GND Relé bekapcsolása ---
                slu.WriteRegister(0, SLOT, 0x02, 0x02);

                //--- DAC2 Relé bekapcsolása ---
                slu.WriteRegister(0, SLOT, 0x02, 0x08);
            }
        }

        /// <summary>
        /// L3_M4_HI
        /// INST1 - AB4 - ROW8
        /// </summary>
        [Test]
        public void L3_M4_HI()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01);

                //--- AB4_I1 ---
                slu.WriteRegister(0, SLOT, 0x26, 0x01);

                //--- K4 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x08);

                //--- AB4_R8 ---
                slu.WriteRegister(0, SLOT, 0x29, 0x80);

            }
        }

        /// <summary>
        /// L3_M4_LO
        /// INST2 - AB3 - ROW10
        /// </summary>
        [Test]
        public void L3_M4_LO()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 
                                             
                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01);

                //--- AB3_I2 ---
                slu.WriteRegister(0, SLOT, 0x1E, 0x02);

                //--- K3 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x04);

                //--- AB3_R10 ---
                slu.WriteRegister(0, SLOT, 0x22, 0x02);
            }
        }

        /// <summary>
        /// M1_L2_HI
        /// INST1 - AB2 - ROW1
        /// </summary>
        [Test]
        public void M1_L2_HI()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01);

                //--- AB2_I1 ---
                slu.WriteRegister(0, SLOT, 0x16, 0x01);

                //--- K2 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x02);

                //--- AB2_R1 ---
                slu.WriteRegister(0, SLOT, 0x19, 0x01);
            }
        }


        /// <summary>
        /// M1_L2_LO
        /// INST2 - AB1 - ROW2
        /// </summary>
        [Test]
        public void M1_L2_LO()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01);

                //--- AB1_I2 ---
                slu.WriteRegister(0, SLOT, 0x0E, 0x02);

                //--- K1 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01);

                //--- AB1_R2 ---
                slu.WriteRegister(0, SLOT, 0x11, 0x02);
            }
        }


        /// <summary>
        /// M1_M2_HI
        /// INST1 - AB2 - ROW3
        /// </summary>
        [Test]
        public void M1_M2_HI()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01);

                //--- AB2_I1 ---
                slu.WriteRegister(0, SLOT, 0x16, 0x01);

                //--- K2 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x02);

                //--- AB2_R3 ---
                slu.WriteRegister(0, SLOT, 0x19, 0x04);
            }
        }

        /// <summary>
        /// M1_M2_LO
        /// INST2 - AB1 - ROW2
        /// </summary>
        [Test]
        public void M1_M2_LO()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01);

                //--- AB1_I2 ---
                slu.WriteRegister(0, SLOT, 0x0E, 0x02);

                //--- K1 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01);

                //--- AB1_R2 ---
                slu.WriteRegister(0, SLOT, 0x11, 0x02);
            }
        }

        /// <summary>
        /// M3_L3_HI
        /// INST1 - AB2 - ROW8
        /// </summary>
        [Test]
        public void M3_L3_HI()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01);

                //--- AB2_I1 ---
                slu.WriteRegister(0, SLOT, 0x16, 0x01);

                //--- K2 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x02);

                //--- AB2_R8 ---
                slu.WriteRegister(0, SLOT, 0x19, 0x80);
            }
        }

        /// <summary>
        /// M3_L3_LO
        /// INST2 - AB1 - ROW9
        /// </summary>
        [Test]
        public void M3_L3_LO()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01);

                //--- AB1_I2 ---
                slu.WriteRegister(0, SLOT, 0x0E, 0x02);

                //--- K1 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01);

                //--- AB1_R9 ---
                slu.WriteRegister(0, SLOT, 0x12, 0x01);
            }
        }

        /// <summary>
        /// Offset
        /// INST1 - AB2 - ROW1 - AB1 - INST2
        /// </summary>
        [Test]
        public void Offset()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01);

                //--- AB2_I1 ---
                slu.WriteRegister(0, SLOT, 0x16, 0x01);

                //--- K2 és K1 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x03);

                //--- AB2_R1 ---
                slu.WriteRegister(0, SLOT, 0x19, 0x01);

                //--- AB1_R1 ---
                slu.WriteRegister(0, SLOT, 0x11, 0x01);

                //--- AB1_R2 ---
                slu.WriteRegister(0, SLOT, 0x0E, 0x02);
            }
        }

        [Test]
        public void Aux34()
        {
            var qusb = new QuickUsb();
            qusb.Open("QUSB-0");

            using (var slu = new SluCtl(qusb))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01);

                //--- AUX_R34 ---
                slu.WriteRegister(0, SLOT, 0x0D, 0x02);
            }
        }
    }
}
