using System.Xml.Serialization;

namespace CHI.Services.DTO.Flk
{
    [XmlRoot(ElementName = "SCHET")]
    public class SCHET
    {

        [XmlElement(ElementName = "CODE")]
        public int CODE { get; set; }


    }
}
