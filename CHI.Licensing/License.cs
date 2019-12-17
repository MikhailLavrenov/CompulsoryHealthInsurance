using Prism.Mvvm;
using System;
using System.Text;

namespace CHI.Licensing
{
    [Serializable]
    public class License : BindableBase
    {
        private string owner;
        private string examinationsFomsCodeMO;
        private DateTime? examinationsMaxDate;
        private bool examinationsUnlimited;

        public string Owner { get => owner; set => SetProperty(ref owner, value); }
        public string ExaminationsFomsCodeMO { get => examinationsFomsCodeMO; set => SetProperty(ref examinationsFomsCodeMO, value); }
        public DateTime? ExaminationsMaxDate { get => examinationsMaxDate; set => SetProperty(ref examinationsMaxDate, value); }
        public bool ExaminationsUnlimited { get => examinationsUnlimited; set => SetProperty(ref examinationsUnlimited, value); }
    }
}
