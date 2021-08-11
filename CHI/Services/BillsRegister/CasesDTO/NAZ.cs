using System;
using System.Xml.Serialization;

namespace CHI.Services.CasesDTO
{
    /// <summary>
    /// Представляет информацию о назначении
    /// </summary>
    [XmlRoot(ElementName = "NAZ")]
    public class NAZ
    {
        /// <summary>
        /// Вид назначения
        /// 1 – направлен на консультацию в медицинскую организацию по месту прикрепления;
        /// 2 – направлен на консультацию в иную медицинскую организацию;
        /// 3 – направлен на обследование;
        /// 4 – направлен в дневной стационар;
        /// 5 – направлен на госпитализацию;
        /// 6 – направлен в реабилитационное отделение.
        /// </summary>
        [XmlElement(ElementName = "NAZ_R")]
        public int NAZ_R { get; set; }
    }
}
