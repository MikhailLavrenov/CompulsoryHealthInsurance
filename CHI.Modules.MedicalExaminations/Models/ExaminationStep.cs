using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Modules.MedicalExaminations.Models
{
    public class ExaminationStep
    {
        public ExaminationStepKind ExaminationStepKind { get; set; }
        public ExaminationKind Type { get; set; }
        public int Year { get; set; }
        public DateTime Date { get; set; }
        public HealthGroup HealthGroup { get; set; }
        public Referral Referral { get; set; }
    }
}
