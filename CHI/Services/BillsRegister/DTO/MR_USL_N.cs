using System.Xml.Serialization;

namespace CHI.Services
{
    [XmlRoot(ElementName = "MR_USL_N")]
    public class MR_USL_N
    {
        /// <summary>
        /// Специальность медработника, выполнившего услугу
        /// </summary>
        [XmlElement(ElementName = "PRVS")]
        public int PRVS { get; set; }
        /// <summary>
        /// Код медработника, оказавшего услугу
        /// </summary>
        [XmlElement(ElementName = "CODE_MD")]
        public string CODE_MD { get; set; }
    }
}
