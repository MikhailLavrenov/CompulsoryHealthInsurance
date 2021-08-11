using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CHI.Services.CasesPaymentDTO
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
    }
}
