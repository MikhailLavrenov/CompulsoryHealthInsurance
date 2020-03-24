using System.ComponentModel;

namespace CHI.Models.ServiceAccounting
{
    public enum ExpressionKind
    {
        [Description("Пусто")] None = 0,
        [Description("Цель обращения")] TreatmentPurpose = 1,
        [Description("Цель посещения")] VisitPurpose = 2, 
        [Description("Код услуги")] ServiceCode = 3,
        [Description("Сумма вложенных")] SumOfNested = 4,

    }
}
