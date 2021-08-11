using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CHI.Services.CasesDTO
{
    /// <summary>
    /// Представляет информацию о случае обращения за мед. помощью
    /// </summary>
    [XmlRoot(ElementName = "SL")]
    public class SL
    {
        /// <summary>
        /// Идентификатор случая
        /// </summary>
        [XmlElement(ElementName = "SL_ID")]
        public string SL_ID { get; set; }
        /// <summary>
        /// Код лечащего врача/врача, закрывшего талон
        /// </summary>
        [XmlElement(ElementName = "IDDOKT")]
        public string IDDOKT { get; set; }
        /// <summary>
        /// Специальность лечащего врача/врача, закрывшего талон
        /// </summary>
        [XmlElement(ElementName = "PRVS")]
        public int PRVS { get; set; }
        /// <summary>
        /// Цель посещения
        /// </summary>
        [XmlElement(ElementName = "P_CEL")]
        public double P_CEL { get; set; }
        /// <summary>
        /// Цель обращения
        /// </summary>
        [XmlElement(ElementName = "CEL")]
        public int CEL { get; set; }
        /// <summary>
        /// Колчество койко-дней
        /// </summary>
        [XmlElement(ElementName = "KD")]
        public int KD { get; set; }
        /// <summary>
        /// Дата начала лечения
        /// </summary>
        [XmlElement(ElementName = "DATE_1")]
        public DateTime DATE_1 { get; set; }
        /// <summary>
        /// Дата окончания лечения
        /// </summary>
        [XmlElement(ElementName = "DATE_2")]
        public DateTime DATE_2 { get; set; }
        /// <summary>
        /// Список назначений
        /// </summary>
        [XmlElement(ElementName = "NAZ")]
        public List<NAZ> NAZ { get; set; }
        /// <summary>
        /// Список оказанных услуг
        /// </summary>
        [XmlElement(ElementName = "USL")]
        public List<USL> USL { get; set; }
    }
}
