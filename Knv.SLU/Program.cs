using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitwiseSystems;
using System.Threading;
using System.Reflection;

namespace Knv.SLU
{
    internal class Program
    {
        static string LOG_ROOT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        static void Main(string[] args)
        {
            using (var slu = new SluIo())
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
                slu.LogSave(LOG_ROOT_DIR, MethodBase.GetCurrentMethod().Name);
            }

            Console.ReadLine();
        }
    }
}
