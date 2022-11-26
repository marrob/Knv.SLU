using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitwiseSystems;
using System.Threading;


namespace Knv.SLU
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int row = 0;
            using (var slu = new SluIo())
            {
                slu.Open();
                for (int unit = 0; unit < 2; unit++)
                {
                    for (int slot = 1; slot <= 21; slot++)
                    {
                        row++;
                        var type = slu.ReadRegister((byte)unit, (byte)slot, 0);
                        string name = "";
                        slu.CardTypes.TryGetValue(type, out name);
                        Console.WriteLine($"{row}. SLU{unit}, Slot: {slot}, Card Type:{name} - {type:X2} "); 
                    }
                }
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
