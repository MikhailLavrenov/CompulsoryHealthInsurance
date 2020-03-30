using System.ComponentModel;

namespace CHI.Models.ServiceAccounting
{
    public enum CaseFilterKind
    {
        [Description("Пусто")] None = 0,
        [Description("Цель обращения")] TreatmentPurpose = 1,
        [Description("Цель посещения")] VisitPurpose = 2, 
        [Description("Содержит услугу")] ContainsService = 3,
        [Description("Не содержит услугу")] NotContainsService = 4,
        [Description("Итог")] Total = 5,
    }
}
