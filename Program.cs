using System;
using System.Reflection;

namespace Knv.SLU.Discovery
{
    internal class Program
    {
        static string LOG_ROOT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        static void Main(string[] args)
        {
            SluIo slu = null;
            try
            {
                using (slu = new SluIo())
                {
                    slu.Open();
                    int row = 0;
                    for (byte unit = 0; unit < SluIo.GetAttachedNameOfUnits().Count; unit++)
                    {
                        for (byte slot = 0; slot <= 21; slot++)
                        {
                            if (slu.CardIsPresent(unit, slot))
                            {
                                row++;
                                Console.WriteLine($"{row}. Unit:{unit}, Slot {slot}, Model:{slu.GetCardModel(unit, slot)} 0x{slu.GetCardType(unit, slot):X2} ");
                            }
                        }
                    }

                    if (row == 0)
                        Console.WriteLine("Cards not found..."); 
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
