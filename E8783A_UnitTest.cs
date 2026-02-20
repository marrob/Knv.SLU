
namespace Knv.SLU.Discovery
{
    using System;
    using System.Linq;
    using BitwiseSystems;
    using NUnit.Framework;


    [TestFixture]
    internal class E8783A_UnitTest
    {
        const byte SLOT = 0; //0..21

        [Test]
        public void SluCount()
        {
            var devname = QuickUsb.FindModules().ToList<string>();
            Assert.IsTrue(devname.Contains("QUSB-0"));
        }

        [Test]
        public void Slu_Bypass_Reset()
        {
            byte status = 0;
           
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);

                Assert.AreEqual(0x47, type); //0x43 -> E8783A 

                //--- Eredeti E8783A bus control and protection bypass relay ---
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

            }
        }


        [Test]
        public void ResistanceTest()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x47, type); //0x43 -> E8783A 

                slu.WriteRegister(0, SLOT, 0x02, 0x01); // reset

                //--- ROW1 & ABUS1 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01); // K1
                slu.WriteRegister(0, SLOT, 0x0E, 0x01); // ROW1
                slu.WriteRegister(0, SLOT, 0x02, 0x01); // reset

                //--- ROW16 & ABUS4 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x08); // K4
                slu.WriteRegister(0, SLOT, 0x27, 0x80); // ROW16
                slu.WriteRegister(0, SLOT, 0x02, 0x01); // reset

                //--- ROW64 & ABUS3 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x04); // K3
                slu.WriteRegister(0, SLOT, 0x25, 0x80); // ROW64
                slu.WriteRegister(0, SLOT, 0x02, 0x01); // reset


                //--- AUX1 ---
                slu.WriteRegister(0, SLOT, 0x06, 0x01); // AUX1
                slu.WriteRegister(0, SLOT, 0x02, 0x01); // reset

                //--- AUX16 ---
                slu.WriteRegister(0, SLOT, 0x07, 0x80); // AUX16
                slu.WriteRegister(0, SLOT, 0x02, 0x01); // reset

                //--- AUX64 ---
                slu.WriteRegister(0, SLOT, 0x0D, 0x80); // AUX64
                slu.WriteRegister(0, SLOT, 0x02, 0x01); // reset

            }
        }

        [Test]
        public void Bypass()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x47, type); //0x43 -> E8783A 
  
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
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x47, type); //0x47 -> E8783A 

                //--- K1 - ABUS1 Bypass On ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01);
                slu.WriteRegister(0, SLOT, 0x04, 0x00);

                //--- AUX_1 ez az első relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x06, 0x01);

                //--- AB4_R64 - Az utolsó relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x2D, 0x80);

            }
        }

        [Test]
        public void DisconnectRelayOnOff()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x47, type); //0x47 -> E8783A 

                //Ez a bit NEM DOKUMENTÁLT!!! ezt a 8783A kártya is müködik
                slu.WriteRegister(0, SLOT, 0x02, 0x40); // Kikapcsolja a disconnect reléket 
                slu.WriteRegister(0, SLOT, 0x02, 0x00); // Bekapcsolja a disconnect reléket
            }
        }

        [Test]
        public void DisconnectControl()
        {
            byte status = 0;
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x47, type); //0x47 -> E8783A 

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
                slu.WriteRegister(0, SLOT, 0x0E, 0x01);

                //--- ABUS1 Bypass K1 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01);
            }
        }


        [Test]
        public void OpenAllRealy()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x47, type); //0x47 -> E8783A 

                //--- AUX1 - Ez az első relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x06, 0x01);

                //--- AB1_R1 ---
                slu.WriteRegister(0, SLOT, 0x0E, 0x01);

                //--- AB4_R40 - Az utolsó relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x2D, 0x80);

                //--- ABUS1 Bypass K1 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01);
                
                //--- Open All Relay - AOR ---
                slu.WriteRegister(0, SLOT, 0x02, 0x20);  //OK 25.11.17
            }
        }

        [Test]
        public void Reset()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x47, type); //0x47 -> E8783A 

                //--- AUX1 - Ez az első relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x06, 0x01);

                //--- AB1_R1 ---
                slu.WriteRegister(0, SLOT, 0x0E, 0x01);

                //--- AB4_R40 - Az utolsó relé a láncban ---
                slu.WriteRegister(0, SLOT, 0x2D, 0x80);

                //--- ABUS1 Bypass K1 ---
                slu.WriteRegister(0, SLOT, 0x04, 0x01);
                
                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01); //OK 25.11.17
            }
        }



        [Test]
        public void CAN3_Termination() //Ez rossz
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x47, type); //0x47 -> E8783A 

                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01); //OK 25.11.17


                slu.WriteRegister(0, SLOT, 0x09, 0x30); // AUX29-ROW29,ROW30-AUX30  Goppel-t kapcsolja ROW29/ROW30-ra            
                slu.WriteRegister(0, SLOT, 0x21, 0x10); // ABUS3-ROW29  ROW29/30 kapcsolja az ABUS3/ABUS4-re
                slu.WriteRegister(0, SLOT, 0x29, 0x20); // ABUS4-ROW30

                slu.WriteRegister(0, SLOT, 0x22, 0x02); // ABUS3-ROW34  ABUS3/ABUS4-et kapcsolja lezáró ellenállásra
                slu.WriteRegister(0, SLOT, 0x2A, 0x01); // ABUS4-ROW33
            }
        }

        [Test]
        public void CAN2_Termination() //Ez a jó
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x47, type); //0x47 -> E8783A 

                //--- Reset ---
                slu.WriteRegister(0, SLOT, 0x02, 0x01); //OK 25.11.17

                slu.WriteRegister(0, SLOT, 0x09, 0x03); // AUX25-ROW25,AUX26-ROW26 
                                                        
                slu.WriteRegister(0, SLOT, 0x21, 0x01); // ABUS3-ROW25
                slu.WriteRegister(0, SLOT, 0x29, 0x02); // ABUS4-ROW26

                slu.WriteRegister(0, SLOT, 0x22, 0x02); // ABUS3-ROW34 ABUS3/ABUS4-et kapcsolja lezáró ellenállásra
                slu.WriteRegister(0, SLOT, 0x2A, 0x01); // ABUS4-ROW33
            }
        }
    }
}
