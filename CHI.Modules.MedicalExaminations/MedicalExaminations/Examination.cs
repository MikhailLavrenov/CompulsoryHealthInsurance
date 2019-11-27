using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{    
    public class Examination
    {
        //public ExaminationKind Kind { get; set; }
        //public int Stage { get; set; }
        //public int Year { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }        
        public ExaminationHealthGroup HealthGroup { get; set; }
        public ExaminationReferral Referral { get; set; }
    }
}
