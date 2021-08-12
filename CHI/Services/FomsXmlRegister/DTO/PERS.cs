using System;
using System.Xml.Serialization;

namespace CHI.Services
{
    /// <summary>
    /// Представляет сведения о пациенте
    /// </summary>
    [XmlRoot(ElementName = "PERS")]
    public class PERS
    {
        /// <summary>
        /// Guid пациента
        /// </summary>
        [XmlElement(ElementName = "ID_PAC")]
        public string ID_PAC { get; set; }
        /// <summary>
        /// Фамилия
        /// </summary>
        [XmlElement(ElementName = "FAM")]
        public string FAM { get; set; }
        /// <summary>
        /// Имя
        /// </summary>
        [XmlElement(ElementName = "IM")]
        public string IM { get; set; }
        /// <summary>
        /// Отчество
        /// </summary>
        [XmlElement(ElementName = "OT")]
        public string OT { get; set; }
        /// <summary>
        /// Дата рождения
        /// </summary>
        [XmlElement(ElementName = "DR")]
        public DateTime DR { get; set; }
    }
}
