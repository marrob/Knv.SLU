
namespace Knv.SLU.Discovery
{
    using BitwiseSystems;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;

    [TestFixture]
    internal class MRLY240314_E8782A_UnitTest
    {
        const byte SLOT = 0; //0..21

        const int ADDR_ADC_RESULT_B1_REG = 0xE6;
        const int ADDR_ADC_RESULT_B2_REG = 0xE7;
        const int ADDR_ADC_RESULT_B3_REG = 0xE8;
        const int ADDR_ADC_RESULT_B4_REG = 0xE9;
        const int ADDR_ADC_RESULT_B5_REG = 0xEA;
        const int ADDR_ADC_RESULT_B6_REG = 0xEB;
        const int ADDR_ADC_RESULT_B7_REG = 0xEC;
        const int ADDR_ADC_RESULT_B8_REG = 0xED;

        const int ADDR_UID_B1_REG = 0xF3;
        const int ADDR_UID_B2_REG = 0xF4;
        const int ADDR_UID_B3_REG = 0xF5;
        const int ADDR_UID_B4_REG = 0xF6;
        const int ADDR_UID_B5_REG = 0xF7;
        const int ADDR_UID_B6_REG = 0xF8;
        const int ADDR_UID_B7_REG = 0xF9;
        const int ADDR_UID_B8_REG = 0xFA;



        [Test]
        public void SluCount()
        {
            var devname = QuickUsb.FindModules().ToList<string>();
            Assert.IsTrue(devname.Contains("QUSB-0"));
        }

        [Test]
        public void IoTest()
        { 
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                for (int i = 0; i < 100; i++)
                {
                    // --- Beírom a Teszértékeket a teszt terültre ---
                    slu.WriteRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B1_REG, data: 0xA1);
                    slu.WriteRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B2_REG, data: 0xA2);
                    slu.WriteRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B3_REG, data: 0xA3);
                    slu.WriteRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B4_REG, data: 0xA4);
                    slu.WriteRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B5_REG, data: 0xA5);
                    slu.WriteRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B6_REG, data: 0xA6);
                    slu.WriteRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B7_REG, data: 0xA7);
                    slu.WriteRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B8_REG, data: 0xA8);

                    byte b1 = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B1_REG);
                    Assert.AreEqual(0xA1, b1);
                    byte b2 = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B2_REG);
                    Assert.AreEqual(0xA2, b2);
                    byte b3 = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B3_REG);
                    Assert.AreEqual(0xA3, b3);
                    byte b4 = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B4_REG);
                    Assert.AreEqual(0xA4, b4);
                    byte b5 = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B5_REG);
                    Assert.AreEqual(0xA5, b5);
                    byte b6 = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B6_REG);
                    Assert.AreEqual(0xA6, b6);
                    byte b7 = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B7_REG);
                    Assert.AreEqual(0xA7, b7);
                    byte b8 = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_ADC_RESULT_B8_REG);
                    Assert.AreEqual(0xA8, b8);
                }
            }
        }

        [Test]
        public void UidTest()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                byte[] uidBytes = new byte[8];

                uidBytes[0] = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_UID_B1_REG);
                uidBytes[1] = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_UID_B2_REG);
                uidBytes[2] = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_UID_B3_REG);
                uidBytes[3] = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_UID_B4_REG);
                uidBytes[4] = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_UID_B5_REG);
                uidBytes[5] = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_UID_B6_REG);
                uidBytes[6] = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_UID_B7_REG);
                uidBytes[7] = slu.ReadRegister(unit: 0, slot: SLOT, register: ADDR_UID_B8_REG);
                string uid = Encoding.ASCII.GetString(uidBytes).Trim('\0');
                Assert.AreEqual("0YQR8T7", uid); //E8782A gyári UID-je")
            }
        }

        [Test]
        public void Slu_K125()
        {
            byte status = 0;
           
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);

                Assert.AreEqual(0x43, type); //0x43 -> E8782A 
                                             //   slu.WriteRegister(0, 0, 0x11, 0x01); //SLU:0, Slot:0, ABUS1 & ROW1: K125 -> reg:0x11, data:0x01
                                             //   slu.WriteRegister(0, 0, 0x09, 0x01); //Aux1 - Row1: K925 -> reg:0x09, data:0x01

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
            int type = 0;
            byte regval = 0;
            using (var slu = new SluCtl("QUSB-0"))
            {
                for (int i = 0; i < 10; i++)
                {
                    type = slu.ReadRegister(0, SLOT, 0);
                    regval = slu.ReadRegister(0, SLOT, 0xF7);
                    regval = slu.ReadRegister(0, SLOT, 0xF8);
                    regval = slu.ReadRegister(0, SLOT, 0xF9);
                    regval = slu.ReadRegister(0, SLOT, 0xFA);

                    //Assert.AreEqual(0x43, type); //0x43 -> E8782A 

                    //--- A Disconnect relék írása hatástalan mivel mindig meg vannak húzva ---

                    slu.WriteRegister(0, SLOT, 0x04, 0xF0);

                    regval = slu.ReadRegister(0, SLOT, 0x04); //csak a MRLY240314 sorzat támogatja
                    regval = slu.ReadRegister(0, SLOT, 0x04); //csak a MRLY240314 sorzat támogatja

                    regval = slu.ReadRegister(0, SLOT, 0x04); //csak a MRLY240314 sorzat támogatja

                    slu.WriteRegister(0, SLOT, 0x04, 0x00);
                    regval = slu.ReadRegister(0, SLOT, 0x04); //csak a MRLY240314 sorzat támogatja

                    //--- Bypass relék kapcsolhatóak ---
                    slu.WriteRegister(0, SLOT, 0x04, 0x0F);
                    slu.WriteRegister(0, SLOT, 0x04, 0x00);

                    //--- K1 - ABUS1 Bypass On ---
                    slu.WriteRegister(0, SLOT, 0x04, 0x01);
                }

            }
        }

        [Test]
        public void TestPoints()
        {
            using (var slu = new SluCtl("QUSB-0"))
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
            using (var slu = new SluCtl("QUSB-0"))
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

            using (var slu = new SluCtl("QUSB-0"))
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
            using (var slu = new SluCtl("QUSB-0"))
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
            using (var slu = new SluCtl("QUSB-0"))
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

    }
}
