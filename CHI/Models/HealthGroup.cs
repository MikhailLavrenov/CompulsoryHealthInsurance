using System.ComponentModel;

namespace CHI.Models
{
    /// <summary>
    /// Группа здоровья
    /// </summary>
    public enum HealthGroup
    {
        [Description("Не выбрано")] None = 0,
        [Description("1")] First = 1,
        [Description("2")] Second = 2,
        [Description("3А")] ThirdA = 4,
        [Description("3Б")] ThirdB = 5
    }
}
