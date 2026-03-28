
namespace Knv.SLU.Discovery
{
    using BitwiseSystems;
    using NUnit.Framework;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;

    [TestFixture]
    internal class MRLY240314_E878XA_UnitTest
    {
        const byte SLOT = 21; //0..21

        const byte TPIC_COUNT_E8782A = 53; //E8782A-ban 53 tpic van, a 6. címtől kezdve
        const byte TPIC_COUNT_E8783A = 57; //E8783A-ban 57 tpic van, a 6. címtől kezdve
        const byte TPIC_FIRST_ADDR = 0x06; //0x04-től kezdődnek a tpic az IRHATÓ és OLVASHTÓ regiszterek


        [Test]
        public void SluCount()
        {
            var devname = QuickUsb.FindModules().ToList<string>();
            Assert.IsTrue(devname.Contains("QUSB-0"));
        }

        [Obsolete("Ezt nem futtathatod mert Az ADC regisztereget az ADC folyamatosan írja")]
        [Test]
        public void IoTest()
        { 
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, SluCtl.ADDR_TYPE_REG);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A

                for (int i = 0; i < 100; i++)
                {
                    // --- Beírom a Teszértékeket a teszt terültre ---
                    slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B1_REG, data: 0xA1);
                    slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B2_REG, data: 0xA2);
                    slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B3_REG, data: 0xA3);
                    slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B4_REG, data: 0xA4);
                    slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B5_REG, data: 0xA5);
                    slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B6_REG, data: 0xA6);
                    slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B7_REG, data: 0xA7);
                    slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B8_REG, data: 0xA8);

                    byte b1 = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B1_REG);
                    Assert.AreEqual(0xA1, b1);
                    byte b2 = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B2_REG);
                    Assert.AreEqual(0xA2, b2);
                    byte b3 = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B3_REG);
                    Assert.AreEqual(0xA3, b3);
                    byte b4 = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B4_REG);
                    Assert.AreEqual(0xA4, b4);
                    byte b5 = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B5_REG);
                    Assert.AreEqual(0xA5, b5);
                    byte b6 = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B6_REG);
                    Assert.AreEqual(0xA6, b6);
                    byte b7 = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B7_REG);
                    Assert.AreEqual(0xA7, b7);
                    byte b8 = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_ADC_RESULT_B8_REG);
                    Assert.AreEqual(0xA8, b8);
                }
            }
        }


        [Test]
        public void Debounce_Speed_UnitTest()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, SluCtl.ADDR_TYPE_REG);
                Assert.IsTrue(type == 0x43); //0x43 -> E8782A, csak a E8782A-val müködik

                for (int i = 0; i < 1000; i++)
                {
                    //--- ON ---
                    // Inst1 -> Abus1
                    slu.WriteRegister(slu: 0, slot: SLOT, register: 0x0E, data: 0x01);
                    // Row1 -> Abus1
                    slu.WriteRegister(slu: 0, slot: SLOT, register: 0x11, data: 0x01);
                    // Abus1 Bypass
                    slu.WriteRegister(slu: 0, slot: SLOT, register: 0x04, data: 0x01);
                    DelayNanoseconds(500_000); //500us
                    //--- OFF ---
                    // Inst1 -> Abus1
                    slu.WriteRegister(slu: 0, slot: SLOT, register: 0x0E, data: 0x00);
                    // Row1 -> Abus1
                    slu.WriteRegister(slu: 0, slot: SLOT, register: 0x11, data: 0x00);
                    // Abus1 Bypass
                    slu.WriteRegister(slu: 0, slot: SLOT, register: 0x04, data: 0x00);
                    DelayNanoseconds(500_000); //500us
                }
            }
        }

        static void DelayNanoseconds(long nanoseconds)
        {
            if (!Stopwatch.IsHighResolution)
                throw new NotSupportedException("Nincs nagy felbontású időzítő ezen a rendszeren.");

            // Stopwatch.Frequency = tick / másodperc
            double ticksPerNanosecond = (double)Stopwatch.Frequency / 1_000_000_000.0;
            long targetTicks = (long)(nanoseconds * ticksPerNanosecond);

            long start = Stopwatch.GetTimestamp();
            while (Stopwatch.GetTimestamp() - start < targetTicks) { }
        }


        [Test]
        public void Reg_WriteProtection_DefaultValue()
        {
            byte regvalue;
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, SluCtl.ADDR_TYPE_REG);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A
                                                             
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_TYPE_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_TYPE_REG);
                Assert.IsTrue(regvalue == 0x43 || regvalue == 0x47);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_CFG_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_CFG_REG);
                Assert.AreEqual(0x0F, regvalue);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_READONLY_1_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_READONLY_1_REG);
                Assert.AreEqual(0x55, regvalue);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_READONLY_2_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_READONLY_2_REG);
                Assert.AreEqual(0xAA, regvalue);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B1_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B1_REG);
                Assert.IsTrue(regvalue!= 0xCC);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B2_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B2_REG);
                Assert.IsTrue(regvalue != 0xCC);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B3_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B3_REG);
                Assert.IsTrue(regvalue != 0xCC);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B4_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B4_REG);
                Assert.IsTrue(regvalue != 0xCC);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B5_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B5_REG);
                Assert.IsTrue(regvalue != 0xCC);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B6_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B6_REG);
                Assert.IsTrue(regvalue != 0xCC);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B7_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B7_REG);
                Assert.IsTrue(regvalue != 0xCC);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B8_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B8_REG);
                Assert.IsTrue(regvalue != 0xCC);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_VERSION_B1_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_VERSION_B1_REG);
                Assert.IsTrue(regvalue != 0xCC);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_VERSION_B2_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_VERSION_B2_REG);
                Assert.IsTrue(regvalue != 0xCC);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_VERSION_B3_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_VERSION_B3_REG);
                Assert.IsTrue(regvalue != 0xCC);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_VERSION_B4_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_VERSION_B4_REG);
                Assert.IsTrue(regvalue != 0xCC);

                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_INV_TYPE_REG, data: 0xCC);
                regvalue = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_INV_TYPE_REG);
                Assert.IsTrue(regvalue != 0xCC);
            }
        }

        [Test]
        public void UidTest()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
               
                int type = slu.ReadRegister(slu: 0, slot: SLOT, register: 0);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A

                for (int i = 0; i < 100; i++)
                {
                    byte[] uidBytes = new byte[8];

                    uidBytes[0] = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B1_REG);
                    uidBytes[1] = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B2_REG);
                    uidBytes[2] = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B3_REG);
                    uidBytes[3] = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B4_REG);
                    uidBytes[4] = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B5_REG);
                    uidBytes[5] = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B6_REG);
                    uidBytes[6] = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B7_REG);
                    uidBytes[7] = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_UID_B8_REG);
                    string uid = Encoding.ASCII.GetString(uidBytes).Trim('\0');
                    Assert.IsTrue(uid == "0YQR8T7" || uid == "0YQITKU");

                    uid = slu.GetUid(unit: 0, slot: SLOT);
                    Assert.IsTrue(uid == "0YQR8T7" || uid == "0YQITKU");
                }
            }
        }

        [Test]
        public void SupportTest()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(slu: 0, slot: SLOT, register: 0);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A
                bool result = slu.IsMRLY240314Card(unit: 0, slot: SLOT);
                Assert.IsTrue(result);
            }
        }


        [Test]
        public void AdcTest()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(slu: 0, slot: SLOT, register: 0);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A
                double result = slu.MeasureResistance(unit: 0, slot: SLOT);
            }
        }

        [Test]
        public void StatusTest()
        {
            byte status = 0;
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(slu: 0, slot: SLOT, register: 0);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A  

                status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG); //Status regiszter olvasása
                Assert.AreEqual(0x80, status); //0x80 -> nem busy

                //--- Dac2Rly & Dac1Rly & GndRly bekapcsolása ---
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG, 0x0E);
                status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG);
                Assert.AreEqual(0x8E, status);

                //A STATUS-ba a ettöl több bit nem  írtható be, a több Reseteli a státuszt és a reléket is.
                //Kikapcsolnak a Disconnect relék 
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG, 0xDE); //Not Reset & Not Open All Relay (OAR)
                status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG);
                Assert.AreEqual(0xCE, status);

                //A reset
                //Bekapcsolnak a Disconnect relék 
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG, 0x01); //Not Reset & Not Open All Relay (OAR)
                status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG);
                Assert.AreEqual(0x80, status);
            }
        }

        [Test]
        public void Dac2Dac2GndRelay()
        {
            byte status = 0;
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A

                //--- GndRly ---
                slu.WriteRegister(0, SLOT, SluCtl.ADDR_STATUS_REG, 0x02); //bekapcsoljuk GndRly ez a K1103-as relé
                status = slu.ReadRegister(0, SLOT, SluCtl.ADDR_STATUS_REG);//olvasunk egy státuszt
                Console.WriteLine(status); //0x82 változott vagyis a GndRly látszik a státuszban
                slu.WriteRegister(0, SLOT, SluCtl.ADDR_STATUS_REG, 0x00); //töröljük a GndRly-t
                status = slu.ReadRegister(0, SLOT, SluCtl.ADDR_STATUS_REG);//olvasunk egy státuszt
                Console.WriteLine(status); //0x80-ra default értékre változott

                //--- DAC1Rly ---
                slu.WriteRegister(0, SLOT, SluCtl.ADDR_STATUS_REG, 0x04); //bekapcsoljuk DAC1Rly-t ez a K1101
                status = slu.ReadRegister(0, SLOT, SluCtl.ADDR_STATUS_REG);//olvasunk egy státuszt
                Console.WriteLine(status); //0x84 változott vagyis a DAC1Rly látszik a státuszban
                slu.WriteRegister(0, SLOT, SluCtl.ADDR_STATUS_REG, 0x00); //töröljük a DAC1Rly-t
                status = slu.ReadRegister(0, SLOT, SluCtl.ADDR_STATUS_REG);//olvasunk egy státuszt
                Console.WriteLine(status); //0x80-ra default értékre változott

                //--- DAC2Rly ---
                slu.WriteRegister(0, SLOT, SluCtl.ADDR_STATUS_REG, 0x08); //bekapcsoljuk DAC2Rly-t ez a K1102
                status = slu.ReadRegister(0, SLOT, SluCtl.ADDR_STATUS_REG);//olvasunk egy státuszt
                Console.WriteLine(status); //0x88 változott vagyis a DAC2Rly látszik a státuszban
                slu.WriteRegister(0, SLOT, SluCtl.ADDR_STATUS_REG, 0x00); //töröljük a DAC2Rly-t
                status = slu.ReadRegister(0, SLOT, SluCtl.ADDR_STATUS_REG);//olvasunk egy státuszt
                Console.WriteLine(status); //0x80-ra default értékre változott
            }
        }

        [Test]
        public void AllRealyOpen()
        {
            byte status = 0;
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, 0);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A

                //--- all rellays to open ---
                status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG);  //olvasunk egy státuszt
                Console.WriteLine(status); //0x80 - nem busy
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_PROTECTION_REG, 0x0F);//bekapcsoljuk a bypass reléket
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG, 0x20);    //minden relét nyitunk
                status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG);  //OAE olvasunk egy státuszt
                Console.WriteLine(status); //0x80 a státusz nem változott
            }
        }

        [Test]
        public void TpicAreaReadWriteTest()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, register: SluCtl.ADDR_TYPE_REG);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A

                byte tpicCount = 0;
                if (type == 0x43)
                {
                    tpicCount = TPIC_COUNT_E8782A;
                }
                else if (type == 0x47)
                {
                    tpicCount = TPIC_COUNT_E8783A;
                }
 
                bool isPass = true;
                for (int j = 1; j < 100; j++)
                {
                    isPass = true;
                    for (byte i = 0; i < tpicCount; i++)
                    {
                        byte writeData = 0x81; //Ne húzz meg sok relét, csak ha nagyon jó tápod van
                        slu.WriteRegister(slu: 0, slot: SLOT, register: (byte)(TPIC_FIRST_ADDR + i), data: writeData);
                        byte readData = slu.ReadRegister(slu: 0, slot: SLOT, register: (byte)(TPIC_FIRST_ADDR + i));

                        if (writeData != readData)
                            isPass = false;
                    }

                    slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG, 0x01);    //minden relét nyitunk

                    Assert.IsTrue(isPass);
                }

                // --- Minden relé nyitva, ha regiszterek értéke 0 ---
                for (byte i = 0; i < tpicCount; i++)
                {
                    byte readData = slu.ReadRegister(slu: 0, slot: SLOT, register: (byte)(TPIC_FIRST_ADDR + i));
                    if (0 != readData)
                        isPass = false;
                }

                byte readDatax = slu.ReadRegister(slu: 0, slot: SLOT, register: 5);

                Assert.IsTrue(isPass);
            }
        }

        [Test]
        public void ResetTest()
        {
            byte status = 0;
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, register: SluCtl.ADDR_TYPE_REG);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A
                
                byte tpicCount = 0;
                byte lastTpicAddress = 0;
                if (type == 0x43)
                {
                    lastTpicAddress = 0x3A; 
                    tpicCount = TPIC_COUNT_E8782A;
                }
                else if (type == 0x47)
                {
                    lastTpicAddress = 0x3E;
                    tpicCount = TPIC_COUNT_E8783A;
                }
                //--- A TPIC utáni terültre nem szabad íri, és amit odairok meg kell, hogy maradjon, itt nem kattanhat relé --
                slu.WriteRegister(slu: 0, slot: SLOT, register: (byte)(lastTpicAddress + 1), data: 0xCC);

                byte tpicValue = 0x01; // ne huzz meg sok relét, mert a táp kevés lehet
                for (byte i = 0; i < tpicCount; i++)
                    slu.WriteRegister(slu: 0, slot: SLOT, register: (byte)(TPIC_FIRST_ADDR + i), data: tpicValue); 

                status = slu.ReadRegister(slu: 0, slot: SLOT, register: lastTpicAddress);  
                Assert.AreEqual(0x01, tpicValue);
                
                status = slu.ReadRegister(slu: 0, slot: SLOT, register: (byte)(lastTpicAddress + 1));
                Assert.AreEqual(0xCC, status);

                //--- RESET: Status és minden TPIC alaphelyzetbe ---
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG, 0x01);

                //--- A TPIC terület utáni területetre a RESET nincs hatással ezt elleneörzöm ---
                status = slu.ReadRegister(slu: 0, slot: SLOT, register: (byte)(lastTpicAddress + 1));
                Assert.AreEqual(0xCC, status);

                status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG);  //olvasunk egy státuszt
                Console.WriteLine(status); //0x80 a státusz nem változott
            }
        }

        [Test]
        public void DisconnectControl()
        {
            byte status = 0;

            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(0, SLOT, register: SluCtl.ADDR_TYPE_REG);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A

                //--- alapból be vannak kapcsolva a discconnect relék ---
                // 2026.02.25-10:27
                // Javítás ennek a regiszternek deafult 0x00-nak kell lennie
                status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_PROTECTION_REG);
                Assert.IsTrue(status == 0xF0 || status == 0x00); //Az MRLY240313 visszaadja a Disconnect relé állapotokat, az Eredeti kártyák NEM

                //--- Bekpacsolja a disconnect reléket az engedélyezés után, de azok már bevannak így nem kattan relé ---
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_PROTECTION_REG, data: 0xF0);

                //--- Mivel DCE nincs engdélyezve ezért nem törölhetem a Disconnect relék bekacsolt állapotát, igy nem kattanak a relék ---
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_PROTECTION_REG, data: 0x00);

                //--- Status olvasása ---
                status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG);  //0x80
                Assert.AreEqual(0x80, status);

                //--- DCE Engedélyezi a Disconnect relék vezérlést ---
                //Fix: 2026.02.25-10:27 itt kikapcsolnak a discconect relék!
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG, data: 0x40);

                //--- Status olvasása ---
                status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG);  //0xC0
                Assert.AreEqual(0xC0, status);
                
                //--- Bekapcsolja a disconnect reléket ---
                // itt kattannak a dissconnect relék
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_PROTECTION_REG, data: 0xF0);

                //--- bekapcsolt Disconnect Relék ---
                status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_PROTECTION_REG);  //0x00
                Assert.IsTrue(status == 0xF0 || status == 0x00); //Az MRLY240313 visszaadja a Disconnect relé állapotokat, az Eredeti kártyák NEM

                //--- Kikapcsolja a disconnect reléketet ---
                // itt kattannak a dissconnect relék
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_PROTECTION_REG, data: 0x00);

                //--- OAR - Open all relays ---
                //Bekapcsolódnak a Disconnect relék mivel a DCE alaphelyzetbe áll
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG, data: 0x20);

                //--- Status olvasása ---
                status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG);  //0x80
                Assert.AreEqual(0x80, status);

                //--- Kikapcsolná a disconnect reléket, de OAR miatt itt már nem engedélyezett, itt nem kattanhatnak a relék ---
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_PROTECTION_REG, data: 0x00);

                //--- AB1_R1 ---
                slu.WriteRegister(slu: 0, slot: SLOT, register: 0x11, data: 0x01);

                //--- ABUS1 Bypass K1 ---
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_PROTECTION_REG, data: 0x01);

                //--- Reset ---
                slu.WriteRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_STATUS_REG, data: 0x01);    //minden alaphelyzetbe állítunk
            }
        }

        [Test]
        public void BypassRelays()
        {
            byte status = 0;
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_TYPE_REG);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A

                //--- Eredeti E8782A bus control and protection bypass relay ---
                slu.WriteRegister(slu: 0, slot: SLOT, SluCtl.ADDR_PROTECTION_REG, data: 0x01); //"PB1" - > K1 -> reg:0x04, data:0x01
                slu.WriteRegister(slu: 0, slot: SLOT, SluCtl.ADDR_PROTECTION_REG, data: 0x02); //"PB2" - > K2 -> reg:0x04, data:0x02
                slu.WriteRegister(slu: 0, slot: SLOT, SluCtl.ADDR_PROTECTION_REG, data: 0x04); //"PB3" - > K3 -> reg:0x04, data:0x04
                slu.WriteRegister(slu: 0, slot: SLOT, SluCtl.ADDR_PROTECTION_REG, data: 0x08); //"PB4" - > K4 -> reg:0x04, data:0x08
                slu.WriteRegister(slu: 0, slot: SLOT, SluCtl.ADDR_PROTECTION_REG, data: 0x00); //"AB4" - > K1 -> reg:0x04, data:0x01
                slu.WriteRegister(slu: 0, slot: SLOT, SluCtl.ADDR_PROTECTION_REG, data: 0xFF);

                //--- Read Status ---
                status = slu.ReadRegister(slu: 0, slot: SLOT, SluCtl.ADDR_STATUS_REG); 
                Console.WriteLine(status); //0x80

                //--- Reset and Set Relay ---
                slu.WriteRegister(slu: 0, slot: SLOT, SluCtl.ADDR_PROTECTION_REG, data: 0x0F); // bekapcsoljuk a bypsass reléket
                slu.WriteRegister(slu: 0, slot: SLOT, SluCtl.ADDR_STATUS_REG, data: 0x01); // reset, majd itt törésponttal lehet mérni
                slu.WriteRegister(slu: 0, slot: SLOT, SluCtl.ADDR_PROTECTION_REG, data: 0x0F); // és urja bakpcsoljuk a bypass reléket úgy hogy a reset autó törlődik
            }
        }


        [Test]
        public void DisconnectRelayOnOff()
        {
            using (var slu = new SluCtl("QUSB-0"))
            {
                int type = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_TYPE_REG);
                Assert.IsTrue(type == 0x43 || type == 0x47); //0x43 -> E8782A, 0x47 -> E8783A

                //RESET
                slu.WriteRegister(slu: 0, slot: SLOT, SluCtl.ADDR_STATUS_REG, data: 0x01); // reset, majd itt törésponttal lehet mérni

                byte status = slu.ReadRegister(slu: 0, slot: SLOT, register: SluCtl.ADDR_PROTECTION_REG);
                Assert.AreEqual(0x00, status);

                //Ez a bit NEM DOKUMENTÁLT!!!
                //(ADDR_PROTECTION_REG: 0x00)
                slu.WriteRegister(slu: 0, slot: SLOT, SluCtl.ADDR_STATUS_REG, data: 0x40); // Kikapcsolja a disconnect reléket 
                slu.WriteRegister(slu: 0, slot: SLOT, SluCtl.ADDR_STATUS_REG, data: 0x00); // Bekapcsolja a disconnect reléket
            }
        }
    }
}
