using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services.Common
{
    public struct CounterEventArgs
    {
        public int Counter { get; set; }
        public int Total {get; set; }

        public CounterEventArgs(int counter, int total)
        {
            Counter = counter;
            Total = total;
        }

}
}
