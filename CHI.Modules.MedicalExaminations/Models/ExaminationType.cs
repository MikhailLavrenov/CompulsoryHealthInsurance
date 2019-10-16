using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Modules.MedicalExaminations.Models
{
    public enum ExaminationType
    {
        [Description("Не выбрано")] None = 0,
        [Description ("Диспансеризация 1 раз в 3 года")] Dispanserizacia3=1,
        //[Description("Диспансеризация 1 раз в 2 года")] Dispanserizacia2 = 2,
        [Description("Профилактический осмотр")] ProfOsmotr = 3,
        [Description("Диспансеризация 1 раз в год")] Dispanserizacia1 = 4,
    }
}
