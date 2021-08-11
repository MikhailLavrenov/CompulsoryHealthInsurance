using System;
using System.Xml.Serialization;

namespace CHI.Services.CasesPaymentDTO
{
    /// <summary>
    /// Представляет заголовок файла
    /// </summary>
    [XmlRoot(ElementName = "ZGLV")]
    public class ZGLV
    {
        /// <summary>
        /// Дата формирования файла
        /// </summary>
        [XmlElement(ElementName = "DATA")]
        public DateTime DATA { get; set; }
        /// <summary>
        /// Имя файла
        /// </summary>
        [XmlElement(ElementName = "FILENAME")]
        public string FILENAME { get; set; }
    }
}
