using Prism.Mvvm;
using System;
using System.Xml.Serialization;

namespace CHI.Infrastructure
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
        [XmlElementAttribute(Order = 0)] 
        public string Owner { get => owner; set => SetProperty(ref owner, value); }
        /// <summary>
        /// Код МО ФОМС, с которым разрешено загружать осмотры на портал диспансеризации
        /// </summary>
        [XmlElementAttribute(Order = 1)]
        public string ExaminationsFomsCodeMO { get => examinationsFomsCodeMO; set => SetProperty(ref examinationsFomsCodeMO, value); }
        /// <summary>
        /// Дата осмотра до, с которой разрешено загружать осмотры на портал диспансеризации
        /// </summary>
        [XmlElementAttribute(Order = 2)]
        public DateTime? ExaminationsMaxDate { get => examinationsMaxDate; set => SetProperty(ref examinationsMaxDate, value); }
        /// <summary>
        /// Загрузка осмотров на портал диспансеризации без ограничений
        /// </summary>
        [XmlElementAttribute(Order = 3)]
        public bool ExaminationsUnlimited { get => examinationsUnlimited; set => SetProperty(ref examinationsUnlimited, value); }
    }
}
