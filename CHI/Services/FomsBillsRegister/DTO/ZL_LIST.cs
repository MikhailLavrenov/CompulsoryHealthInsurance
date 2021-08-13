using System.Collections.Generic;
using System.Xml.Serialization;

namespace CHI.Services
{
    /// <summary>
    /// Представляет информацию о законченных случаях реестра-счетов.
    /// </summary>
    [XmlRoot(ElementName = "ZL_LIST")]
    public class ZL_LIST: BillPart
    {
        /// <summary>
        /// Счет
        /// </summary>
        [XmlElement(ElementName = "SCHET")]
        public SCHET SCHET { get; set; }
        /// <summary>
        /// Заголовок файла
        /// </summary>
        [XmlElement(ElementName = "ZGLV")]
        public ZGLV ZGLV { get; set; }
        /// <summary>
        /// Записи
        /// </summary>
        [XmlElement(ElementName = "ZAP")]
        public List<ZAP> ZAP { get; set; }
    }
}
