using System.ComponentModel;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Шаг прохождения профилактического осмотра
    /// </summary>
    public enum StepKind
    {
        [Description("Не выбрано")] None = 0,
        [Description("Начало 1 этапа")] FirstBegin = 12,
        [Description("Завершен 1 этап")] FirstEnd = 15,
        [Description("Результат 1 этапа")] FirstResult = 16,
        [Description("Направлен на 2 этап")] TransferSecond = 21,
        [Description("Начало 2 этапа")] SecondBegin = 22,
        [Description("Завершен 2 этап")] SecondEnd = 25,
        [Description("Результат 2 этапа")] SecondResult = 40,
        [Description("Отказ от прохождения")] Refuse = 90
    }
}
