using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{
    public class SrzInfo
    {
        public int SrzPatientId { get; set; }
        public bool ExistInPlan { get; set; }
        public bool FilledByAnotherClinic { get; set; }
    }
}
