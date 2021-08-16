using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CHI.Services
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
        /// <summary>
        /// Имя связанного файла
        /// </summary>
        [XmlElement(ElementName = "FILENAME1")]
        public string FILENAME1 { get; set; }
    }
}
