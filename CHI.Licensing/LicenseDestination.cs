using System.ComponentModel;

namespace CHI.Licensing
{
    public enum LicenseDestination
    {
        [Description("Не выбрано")] None = 0,
        [Description("Загрузка осмотров")] MedicalExamination = 1,

    }
}
