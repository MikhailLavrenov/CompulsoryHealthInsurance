using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Licensing
{
    [Serializable]
    public class License
    {
        public string Owner { get; set; }
        public List<Claim> Claims { get; set; }

        public License()
        {
            Claims = new List<Claim>();
        }
    }
}
