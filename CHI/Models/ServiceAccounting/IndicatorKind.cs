using System.ComponentModel;

namespace CHI.Models.ServiceAccounting
{
    public enum IndicatorKind
    {
        [Description("Пусто")] None = 0,
        [Description("Кол-во случаев")] CasesCount = 1,
        [Description("Кол-во услуг")] ServicesCount = 2,
        [Description("Кол-во УЕТ")] LaborAmount = 3,
        [Description("Кол-во койко-дней")] BedDaysCount = 4,
    }
}
