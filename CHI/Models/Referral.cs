using System.ComponentModel;

namespace CHI.Models
{
    /// <summary>
    /// Направление
    /// </summary>
    public enum Referral
    {
        [Description("Не выбрано")] None = 0,
        [Description("Консультацию в МО по месту прикрепления")] LocalClinic = 1,
        [Description("Консультацию в иную МО")] AnotherClinic = 2,
        [Description("Обследование")] Examination = 3,
        [Description("Дневной стационар")] DaytimeHospital = 4,
        [Description("Госпитализацию")] Hospitalization = 5,
        [Description("Реабилитационное отделение")] Rehabilitation = 6,
        [Description("Нет назначения")] No = 7,

    }
}
