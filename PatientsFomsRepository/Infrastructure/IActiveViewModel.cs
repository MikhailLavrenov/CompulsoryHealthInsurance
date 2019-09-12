using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.Infrastructure
{
    public interface IActiveViewModel
    {
        string Status { get; set; }
        string Header { get; set; }
    }
}
