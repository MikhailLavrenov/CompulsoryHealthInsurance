using System.Collections.Generic;
using System.Xml.Serialization;

namespace CHI.Services.DTO.Flk
{
    [XmlRoot(ElementName = "FLK_P")]
    public class FLKP
    {
        [XmlElement(ElementName = "SCHET")]
        public SCHET SCHET { get; set; }

        [XmlElement(ElementName = "ZAP")]
        public List<ZAP> ZAP { get; set; }
    }
}
