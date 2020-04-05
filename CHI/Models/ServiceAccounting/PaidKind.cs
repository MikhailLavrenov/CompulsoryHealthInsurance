using System.ComponentModel;

namespace CHI.Models.ServiceAccounting
{
    public enum PaidKind
    {
        [Description("Пусто")] None = 0,
        [Description("Полная")] Full = 1,
        [Description("Отказ")] Refuse = 2,
        [Description("Частично")] Partly = 3,
    }
}
