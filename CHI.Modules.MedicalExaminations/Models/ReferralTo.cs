using System.ComponentModel;

namespace CHI.Modules.MedicalExaminations.Models
{
    public enum ReferralTo
    {
        [Description("Нет назначения")] None = 7,
        [Description("Консультацию в МО по месту прикрепления")] LocalClinic = 1,
        [Description("Консультацию в иную МО")] AnotherClinic = 2,
        [Description("Обследование")] Examination = 3,
        [Description("Дневной стационар")] DaytimeHospital = 4,
        [Description("Госпитализацию")] Hospitalization = 5,
        [Description("Реабилитационное отделение")] Rehabilitation = 6,

    }
}
