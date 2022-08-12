using System.Collections.Generic;
using System.Xml.Serialization;

namespace CHI.Services.DTO.Flk
{
    [XmlRoot(ElementName = "SLUCH")]
    public class SLUCH
    {

        [XmlElement(ElementName = "IDCASE")]
        public long IDCASE { get; set; }

        [XmlElement(ElementName = "NHISTORY")]
        public string NHISTORY { get; set; }

        [XmlElement(ElementName = "OTKAZ")]
        public List<OTKAZ> OTKAZ { get; set; }
    }
}
