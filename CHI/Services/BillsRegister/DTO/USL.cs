using System;
using System.Xml.Serialization;

namespace CHI.Services
{
    /// <summary>
    /// Представляет информацию об оказанной услуге
    /// </summary>
    [XmlRoot(ElementName = "USL")]
    public class USL
    {
        /// <summary>
        /// Код услуги
        /// </summary>
        [XmlElement(ElementName = "CODE_USL")]
        public int CODE_USL { get; set; }
        /// <summary>
        /// Количество услуг
        /// </summary>
        [XmlElement(ElementName = "KOL_USL")]
        public double KOL_USL { get; set; }
        /// <summary>
        /// Специальность медработника, выполнившего услугу
        /// </summary>
        [XmlElement(ElementName = "PRVS")]
        public int PRVS { get; set; }
        /// <summary>
        /// Код медработника, оказавшего услугу
        /// </summary>
        [XmlElement(ElementName = "CODE_MD")]
        public string CODE_MD { get; set; }
        /// <summary>
        /// Дата начала оказания услуги
        /// </summary>
        [XmlElement(ElementName = "DATE_IN")]
        public DateTime DATE_IN { get; set; }
        /// <summary>
        /// Дата окончания оказания услуги
        /// </summary>
        [XmlElement(ElementName = "DATE_OUT")]
        public DateTime DATE_OUT { get; set; }
        /// <summary>
        /// Медицинские работники, выполнившие услугу
        /// </summary>
        [XmlElement(ElementName = "MR_USL_N")]
        public MR_USL_N MR_USL_N { get; set; }
    }
}
