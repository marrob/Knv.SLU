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
                for (int unit = 0; unit < 2; unit++)
                {
                    for (int slot = 0; slot <= 21; slot++)
                    {
                        row++;
                        var type = slu.ReadRegister((byte)unit, (byte)slot, 0);
                        string name = "";
                        slu.CardTypes.TryGetValue(type, out name);
                        if(type != 0xFF)
                            Console.WriteLine($"{row}. SLU{unit}, Slot: {slot}, Card Type:{name} - {type:X2} "); 
                    }
                }
                slu.LogSave(LOG_ROOT_DIR, MethodBase.GetCurrentMethod().Name);
            }
            Console.ReadLine();
        }


        static void SluIoWriteSLU0CloseK125()
        {

            using (var slu = new SluIo())
            {
                slu.Open();
                slu.WriteRegister(0, 21, 0x11, 1); //Row1 to ABUS1   
            }
        }
    }


}
