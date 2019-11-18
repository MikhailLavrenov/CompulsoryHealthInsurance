using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{
    public enum ExaminationHealthGroup
    {
        [Description("Не выбрано")] None = 0,
        [Description("1 группа здоровья")] First = 1,
        [Description("2 группа здоровья")] Second = 2,
        [Description("3А группа здоровья")] ThirdA = 4,
        [Description("3Б группа здоровья")] ThirdB = 5
    }
}
