using System.Xml.Serialization;

namespace CHI.Services.CasesPaymentDTO
{
    /// <summary>
    /// Представляет информацию о законченном случае мед. помощи
    /// </summary>
    [XmlRoot(ElementName = "Z_SL")]
    public class Z_SL
    {
        /// <summary>
        /// Номер записи в реестре законченных случаев
        /// </summary>
        [XmlElement(ElementName = "IDCASE")]
        public int IDCASE { get; set; }

        /// <summary>
        /// Тип оплаты:
        /// 0 – не принято решение об оплате;
        /// 1 – полная;
        /// 2 – полный отказ;
        /// 3 – частичный отказ;
        /// </summary>
        [XmlElement(ElementName = "OPLATA")]
        public int OPLATA { get; set; }

        /// <summary>
        /// Сумма, принятая к оплате 
        /// </summary>
        [XmlElement(ElementName = "SUMP")]
        public double SUMP { get; set; }

        /// <summary>
        /// Сумма санкций
        /// </summary>
        [XmlElement(ElementName = "SANK_IT")]
        public double SANK_IT { get; set; }

        /// <summary>
        /// Случай обращения за мед. помощью
        /// </summary>
        [XmlElement(ElementName = "SL")]
        public SL SL { get; set; }
    }
}
