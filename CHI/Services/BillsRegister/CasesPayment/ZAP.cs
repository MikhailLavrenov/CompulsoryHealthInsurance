using System.Xml.Serialization;

namespace CHI.Services.BillsRegister.CasesPayment
{
    /// <summary>
    /// Представляет информацию о записи в реестре-счетов
    /// </summary>
    [XmlRoot(ElementName = "ZAP")]
    public class ZAP
    {
        /// <summary>
        /// Сведения о законченном случае
        /// </summary>
        [XmlElement(ElementName = "Z_SL")]
        public Z_SL Z_SL { get; set; }
    }
}
