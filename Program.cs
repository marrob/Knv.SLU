using BitwiseSystems;
using System;
using System.Linq;
using System.Reflection;

namespace Knv.SLU.Discovery
{
    internal class Program
    {
        static string LOG_ROOT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        static void Main(string[] args)
        {
            SluCtl slu = null;
            try
            {
                var racks = QuickUsb.FindModules().ToList<string>();
                Console.WriteLine($"Attached QuickUSB devices:{string.Join(",", racks)}");

                if (racks.Count == 0)
                {
                    Console.WriteLine("No QuickUSB devices found.");
                    return;
                }

                // Ha több racak van a User egy szám alapján kiválaszthatja melyikkel akar dolgozni
                string devname;
                if (racks.Count == 1)
                {
                    devname = racks[0];
                    Console.WriteLine($"Using detected device: {devname}");
                }
                else
                {
                    Console.WriteLine("Select device to use (enter index or full device name):");
                    for (int i = 0; i < racks.Count; i++)
                        Console.WriteLine($"{i}: {racks[i]}");

                    Console.Write("Selection: ");
                    var input = Console.ReadLine();
                    if (int.TryParse(input, out int idx) && idx >= 0 && idx < racks.Count)
                        devname = racks[idx];
                    else if (!string.IsNullOrWhiteSpace(input) && racks.Contains(input))
                        devname = input;
                    else
                    {
                        Console.WriteLine("Invalid selection, using first rack.");
                        devname = racks[0];
                    }
                }

                Console.WriteLine("Card discovery:");
                using (slu = new SluCtl(devname))
                {
                    int row = 0;
                    for (byte unit = 0; unit < racks.Count; unit++)
                    {
                        for (byte slot = 0; slot <= SluCtl.MAX_CARD_COUNT_IN_RACK; slot++)
                        {
                            if (slu.CardIsPresent(unit, slot))
                            {
                                row++;
                                Console.WriteLine($"{row}. Unit:{unit}, Slot {slot}, Model:{slu.GetCardModel(unit, slot)} 0x{slu.GetCardType(unit, slot):X2} ");
                            }
                        }
                    }

                    if (row == 0)
                        Console.WriteLine($"Cards not found in rack {devname}..."); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                slu.LogSave(LOG_ROOT_DIR, MethodBase.GetCurrentMethod().Name);
                Console.ReadLine();
            }
        }
    }
}
