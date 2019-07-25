using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.Infrastructure
    {
    public interface IViewModel
        {
        string FullCaption { get; set; }
        string ShortCaption { get; set; }
        }
    }
