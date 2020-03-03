using System;
using System.Xml.Serialization;

namespace CHI.Services.BillsRegister
{
    /// <summary>
    /// Представляет информацию о счете.
    /// </summary>
    [XmlRoot(ElementName = "SCHET")]
    public class SCHET
    {
        /// <summary>
        /// Год реестра-счетов
        /// </summary>
        [XmlElement(ElementName = "YEAR")]
        public int YEAR { get; set; }
        /// <summary>
        /// Год реестра-счетов
        /// </summary>
        [XmlElement(ElementName = "MONTH")]
        public int MONTH { get; set; }
        /// <summary>
        /// Код медицинской организации.
        /// </summary>
        [XmlElement(ElementName = "CODE_MO")]
        public string CODE_MO { get; set; }
        /// <summary>
        /// Тип диспансеризации
        /// ДВ2 Второй этап диспансеризации определенных групп взрослого населения с периодичностью 1 раз в 3 года
        /// ОПВ Профилактические медицинские осмотры взрослого населения
        /// ДВ4 Первый этап диспансеризации определенных групп взрослого населения с периодичностью 1 раз в год
        /// </summary>
        [XmlElement(ElementName = "DISP")]
        public string DISP { get; set; }
    }
}
