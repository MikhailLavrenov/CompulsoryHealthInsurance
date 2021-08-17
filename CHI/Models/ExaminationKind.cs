using System.ComponentModel;

namespace CHI.Models
{
    /// <summary>
    /// Вид осмотра
    /// </summary>
    public enum ExaminationKind
    {
        [Description("Не выбрано")] None = 0,
        [Description("Диспансеризация раз в 3 года")] Dispanserizacia3 = 1,
        [Description("Профилактический осмотр")] ProfOsmotr = 3,
        [Description("Диспансеризация раз в год")] Dispanserizacia1 = 4,
    }
}
