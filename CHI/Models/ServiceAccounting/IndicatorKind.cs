using System.ComponentModel;

namespace CHI.Models.ServiceAccounting
{
    public enum IndicatorKind
    {
        [Description("Пусто")] None = 0,
        [Description("Cлучаи")] Cases = 1,
        [Description("Услуги")] Services = 2,
        [Description("УЕТы")] LaborCost = 3,
        [Description("Койко-дни")] BedDays = 4,
        [Description("Стоимость")] Cost = 5,
    }
}
