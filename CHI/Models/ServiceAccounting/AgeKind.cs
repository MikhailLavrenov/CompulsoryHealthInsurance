using System.ComponentModel;

namespace CHI.Models.ServiceAccounting
{
    public enum AgeKind
    {
        [Description("Любой")] Any = 0,
        [Description("Дети")] Сhildren = 1,
        [Description("Взрослые")] Adults = 2,
    }
}
