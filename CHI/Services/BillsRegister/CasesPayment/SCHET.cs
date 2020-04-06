using System;
using System.Xml.Serialization;

namespace CHI.Services.BillsRegister.CasesPayment
{
    /// <summary>
    /// Представляет информацию о счете.
    /// </summary>
    [XmlRoot(ElementName = "SCHET")]
    public class SCHET
    {
        /// <summary>
        /// Год реестра-счетов
        /// </summary>
        [XmlElement(ElementName = "YEAR")]
        public int YEAR { get; set; }
        /// <summary>
        /// Год реестра-счетов
        /// </summary>
        [XmlElement(ElementName = "MONTH")]
        public int MONTH { get; set; }
        /// <summary>
        /// Код медицинской организации.
        /// </summary>
        [XmlElement(ElementName = "CODE_MO")]
        public string CODE_MO { get; set; }
    }
}
