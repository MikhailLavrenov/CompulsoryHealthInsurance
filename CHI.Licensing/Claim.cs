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
        public string Description { get; set; }
        public Type TargetType { get; set; }
        public ClaimKey Key { get; set; }
        public string Value { get; set; }       
    }
}
