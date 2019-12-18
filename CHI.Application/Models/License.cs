using Prism.Mvvm;
using System;
using System.Text;

namespace CHI.Application
{
    /// <summary>
    /// Представляет информацию о лицензии
    /// </summary>
    [Serializable]
    public class License : BindableBase
    {
        private string owner;
        private string examinationsFomsCodeMO;
        private DateTime? examinationsMaxDate;
        private bool examinationsUnlimited;

        /// <summary>
        /// Владелец лицензии
        /// </summary>
        public string Owner { get => owner; set => SetProperty(ref owner, value); }
        /// <summary>
        /// Код МО ФОМС, с которым разрешено загружать осмотры на портал диспансеризации
        /// </summary>
        public string ExaminationsFomsCodeMO { get => examinationsFomsCodeMO; set => SetProperty(ref examinationsFomsCodeMO, value); }
        /// <summary>
        /// Дата осмотра до, с которой разрешено загружать осмотры на портал диспансеризации
        /// </summary>
        public DateTime? ExaminationsMaxDate { get => examinationsMaxDate; set => SetProperty(ref examinationsMaxDate, value); }
        /// <summary>
        /// Загрузка осмотров на портал диспансеризации без ограничений
        /// </summary>
        public bool ExaminationsUnlimited { get => examinationsUnlimited; set => SetProperty(ref examinationsUnlimited, value); }
    }
}
