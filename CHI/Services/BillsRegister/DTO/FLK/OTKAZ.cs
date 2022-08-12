using System.Xml.Serialization;

namespace CHI.Services.DTO.Flk
{
    [XmlRoot(ElementName = "OTKAZ")]
    public class OTKAZ
    {
        [XmlElement(ElementName = "I_TYPE")]
        public string ITYPE { get; set; }
    }
}
