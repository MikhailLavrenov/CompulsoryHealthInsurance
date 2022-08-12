using System.Xml.Serialization;

namespace CHI.Services.DTO.Flk
{
    [XmlRoot(ElementName = "ZAP")]
    public class ZAP
    {

        [XmlElement(ElementName = "N_ZAP")]
        public int NZAP { get; set; }

        [XmlElement(ElementName = "SLUCH")]
        public SLUCH SLUCH { get; set; }
    }
}
