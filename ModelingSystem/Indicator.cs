using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingSystem
{
    public class Indicator
    {
        public int NumInterruptMessages { get; set; }
        public int NumSpareChannel { get; set; }

        public override string ToString()
        {
            return String.Format("Число прерванных сообщений: {0,7}, Количество включений запасного канала: {1,7}",
                NumInterruptMessages, NumSpareChannel);
        }
    }
}
