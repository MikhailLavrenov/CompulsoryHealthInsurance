using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Licensing
{
    public interface ILicenseManager
    {
        List<License> Licenses { get; set; }
    }
}
