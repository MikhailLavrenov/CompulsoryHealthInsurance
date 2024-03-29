﻿using System;
using System.Xml.Serialization;

namespace CHI.Services
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
        public long IDCASE { get; set; }
        /// <summary>
        /// Условия оказания
        /// </summary>
        [XmlElement(ElementName = "USL_OK")]
        public int USL_OK { get; set; }
        /// <summary>
        /// Результат диспансеризации
        /// 1	Присвоена I группа здоровья
        /// 2	Присвоена II группа здоровья
        /// 12  Направлен на II этап профилактического медицинского осмотра несовершеннолетних или диспансеризации всех типов, предварительно присвоена II группа здоровья
        /// 3	Присвоена III группа здоровья
        /// 14  Направлен на II этап диспансеризации определенных групп взрослого населения, предварительно присвоена IIIа группа здоровья
        /// 31  Присвоена IIIа группа здоровья	
        /// 15  Направлен на II этап диспансеризации определенных групп взрослого населения, предварительно присвоена IIIб группа здоровья
        /// 32  Присвоена IIIб группа здоровья
        /// </summary>
        [XmlElement(ElementName = "RSLT_D")]
        public int RSLT_D { get; set; }

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
