using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Licensing
{
    [Serializable]
    public class Claim
    {
        public string Destination { get; set; }
        public int Priority { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }

    }
}
