using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Modules.MedicalExaminations.Models
{    
    public class Examination
    {
        public ExaminationKind Type { get; set; }
        public int Stage { get; set; }
        public int Year { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }        
        public HealthGroup HealthGroup { get; set; }
        public Referral Referral { get; set; }
    }
}
