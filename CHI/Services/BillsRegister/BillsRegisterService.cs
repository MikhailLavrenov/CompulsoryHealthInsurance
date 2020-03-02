using CHI.Services.MedicalExaminations;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;

namespace CHI.Services.BillsRegister
{
    /// <summary>
    /// Представляет сервис для работы с xml выгрузкой реестров-счетов по программе ОМС ХК ФОМС
    /// </summary>
    public class BillsRegisterService
    {
        #region Поля
        private static readonly StringComparison comparer = StringComparison.OrdinalIgnoreCase;
        private List<string> filePaths;
        #endregion

        #region Конструкторы
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="filePaths">Коллекиця путей к xml файлам. Могут быть многократно упакованны в zip-архив.</param>
        public BillsRegisterService(ICollection<string> filePaths)
        {
            this.filePaths = filePaths.ToList();
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="filePath">Путь к xml файлу. Может быть многократно упакованн в zip-архив.</param>
        public BillsRegisterService(string filePath)
        {
            filePaths = new List<string>() { filePath };
        }
        #endregion

        #region Методы
        /// <summary>
        /// Получает список профилактических осмотров пациентов из xml файлов реестров-счетов. Среди всех файлов выбирает только необходимые.
        /// </summary>
        /// <param name="examinationsFileNamesStartsWith">Коллекция начала имен файлов с услугами.</param>
        /// <param name="patientsFileNamesStartsWith">Коллекция начала имен файлов с пациентами.</param>
        /// <returns>Список профилактических осмотров пациентовю</returns>
        public List<PatientExaminations> GetPatientsExaminations(IEnumerable<string> examinationsFileNamesStartsWith, IEnumerable<string> patientsFileNamesStartsWith)
        {
            var patientsFiles = GetFiles(patientsFileNamesStartsWith);
            var patientsRegisters = DeserializeCollection<PERS_LIST>(patientsFiles);

            foreach (var file in patientsFiles)
                file.Dispose();

            var examinationsFiles = GetFiles(examinationsFileNamesStartsWith);
            var examinationsRegisters = DeserializeCollection<ZL_LIST>(examinationsFiles);

            foreach (var file in examinationsFiles)
                file.Dispose();

            return ConvertToPatientExaminations(examinationsRegisters, patientsRegisters);
        }
        /// <summary>
        /// Получает список потоков  на файлы из указанных расположений  файла/файлов, начинающихся с заданных имен.
        /// </summary>
        /// <param name="fileNamesStartsWithFilter">Коллекция начала имен файлов.</param>
        /// <returns>Список потоков файлов.</returns>
        private List<Stream> GetFiles(IEnumerable<string> fileNamesStartsWithFilter)
        {
            var files = new List<Stream>();

            foreach (var filePath in filePaths)
                files.AddRange(GetFilesRecursive(filePath, fileNamesStartsWithFilter));

            return files;
        }
        /// <summary>
        ///  Получает список потоков на файлы по заданному пути и имена которых начинаются с опеределенных строк.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <param name="fileNamesStartsWithFilter">Коллекция начала имен файлов.</param>
        /// <returns>Список потоков файлов.</returns>
        private List<Stream> GetFilesRecursive(string path, IEnumerable<string> fileNamesStartsWithFilter)
        {
            var result = new List<Stream>();
            var isDirectory = new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory);

            if (isDirectory)
            {
                var entries = Directory.GetFileSystemEntries(path);

                foreach (var entry in entries)
                    result.AddRange(GetFilesRecursive(entry, fileNamesStartsWithFilter));
            }
            else
            {
                var extension = Path.GetExtension(path);

                if (extension.Equals(".xml", comparer)
                    && fileNamesStartsWithFilter.Any(x => Path.GetFileName(path).StartsWith(x, comparer)))
                    result.Add(new FileStream(path, FileMode.Open));

                else if (extension.Equals(".zip", comparer))
                    using (var archive = ZipFile.OpenRead(path))
                    {
                        foreach (var entry in archive.Entries)
                            result.AddRange(ArchiveEntryGetFilesRecursive(entry, fileNamesStartsWithFilter));
                    }
            }

            return result;
        }
        /// <summary>
        /// Получает список потоков на файлы в архиве, имена которых начинаются с опеределенных строк.
        /// </summary>
        /// <param name="archiveEntry">Файл внутри zip архива.</param>
        /// <param name="fileNamesStartsWithFilter">Коллекция начала имен файлов.</param>
        /// <returns>Список потоков файлов.</returns>
        private List<Stream> ArchiveEntryGetFilesRecursive(ZipArchiveEntry archiveEntry, IEnumerable<string> fileNamesStartsWithFilter)
        {
            var result = new List<Stream>();

            if (string.IsNullOrEmpty(archiveEntry.Name))
                return result;

            var extension = Path.GetExtension(archiveEntry.Name);

            if (extension.Equals(".xml", comparer) && fileNamesStartsWithFilter.Any(x => archiveEntry.Name.StartsWith(x, comparer)))
            {
                var extractedEntry = new MemoryStream();
                archiveEntry.Open().CopyTo(extractedEntry);
                result.Add(extractedEntry);
            }
            else if (extension.Equals(".zip", comparer))
            {
                var extractedEntry = new MemoryStream();
                archiveEntry.Open().CopyTo(extractedEntry);

                using (var archive = new ZipArchive(extractedEntry))
                {
                    foreach (var entry in archive.Entries)
                        result.AddRange(ArchiveEntryGetFilesRecursive(entry, fileNamesStartsWithFilter));
                }
            }

            return result;
        }
        /// <summary>
        /// Конвертирует десериализованные классы xml реестров-счетов в список профилактических осмотров пациентов.
        /// </summary>
        /// <param name="examinationsRegisters">Десериализованные классы услуг xml реестров-счетов.</param>
        /// <param name="patientsRegisters">>Десериализованные классы пациентов xml реестров-счетов.</param>
        /// <returns>Список профилактических осмотров пациентов.</returns>
        private static List<PatientExaminations> ConvertToPatientExaminations(IEnumerable<ZL_LIST> examinationsRegisters, IEnumerable<PERS_LIST> patientsRegisters)
        {
            var result = new List<PatientExaminations>();

            var patients = new List<PERS>();

            foreach (var patientsRegister in patientsRegisters)
                patients.AddRange(patientsRegister?.PERS);

            patients = patients.Distinct().ToList();

            foreach (var examinationsRegister in examinationsRegisters)
            {
                if (examinationsRegister?.SCHET == null || examinationsRegister.ZAP == null)
                    continue;

                var examinationStage = DispToExaminationStage(examinationsRegister.SCHET.DISP);
                var examinationYear = examinationsRegister.SCHET.YEAR;

                if (examinationStage == 0 || examinationYear < 2018)
                    continue;

                foreach (var treatmentCase in examinationsRegister.ZAP)
                {
                    if (treatmentCase?.PACIENT == null || treatmentCase.Z_SL?.SL?.USL == null || treatmentCase.Z_SL.SL.NAZ == null)
                        continue;

                    string insuranceNumber;

                    if (string.IsNullOrEmpty(treatmentCase.PACIENT.SPOLIS))
                        insuranceNumber = $@"{ treatmentCase.PACIENT.NPOLIS}";
                    else
                        insuranceNumber = $@"{treatmentCase.PACIENT.SPOLIS} {treatmentCase.PACIENT.NPOLIS}";

                    if (string.IsNullOrEmpty(insuranceNumber))
                        continue;

                    var examination = new Examination();

                    var foundPatient = patients.FirstOrDefault(x => x.ID_PAC == treatmentCase.PACIENT.ID_PAC);

                    if (foundPatient == default)
                        continue;

                    var examinationKind = DispToExaminationType(examinationsRegister.SCHET.DISP, examinationYear - foundPatient.DR.Year);

                    if (examinationStage == 1)
                        examination.BeginDate = treatmentCase.Z_SL.SL.USL.First(x => x.CODE_USL == "024101").DATE_IN;
                    else
                        examination.BeginDate = treatmentCase.Z_SL.SL.DATE_1;

                    examination.EndDate = treatmentCase.Z_SL.SL.DATE_2;
                    examination.HealthGroup = RSLT_DToHealthGroup(treatmentCase.Z_SL.RSLT_D);
                    examination.Referral = (Referral)(treatmentCase.Z_SL.SL.NAZ.FirstOrDefault()?.NAZ_R ?? 0);

                    //если не заполнено направление
                    if (examination.Referral == 0)
                    {
                        if (examination.HealthGroup == HealthGroup.ThirdA || examination.HealthGroup == HealthGroup.ThirdB)
                            examination.Referral = Referral.LocalClinic;
                        else
                            examination.Referral = Referral.No;
                    }

                    if (examination.HealthGroup == HealthGroup.None)
                        continue;

                    var patientExamination = result.FirstOrDefault(x => x.InsuranceNumber.Equals(insuranceNumber, comparer) && x.Year == examinationYear && x.Kind == examinationKind);

                    if (patientExamination == default)
                        patientExamination = new PatientExaminations(insuranceNumber, examinationYear, examinationKind)
                        {
                            Surname = foundPatient.FAM,
                            Name = foundPatient.IM,
                            Patronymic = foundPatient.OT,
                            Birthdate = foundPatient.DR
                        };

                    if (examinationStage == 1)
                        patientExamination.Stage1 = examination;
                    else if (examinationStage == 2)
                        patientExamination.Stage2 = examination;

                    result.Add(patientExamination);
                }
            }

            return result;
        }
        /// <summary>
        /// Определяет этап профилактического осмотра по его типу.
        /// </summary>
        /// <param name="disp">Тип профилактического осмотра.</param>
        /// <returns>Этап профилактического осмотра.</returns>
        private static int DispToExaminationStage(string disp)
        {
            switch (disp.ToUpper())
            {
                case "ОПВ":
                case "ДВ4":
                    return 1;
                case "ДВ2":
                    return 2;
                default:
                    return 0;
            }
        }
        /// <summary>
        /// Опеределяет вид осмотра по его типу и возрасту пациента.
        /// </summary>
        /// <param name="disp">Тип профилактического осмотра.</param>
        /// <param name="age">Возраст пациента.</param>
        /// <returns>Вид осмотра.</returns>
        private static ExaminationKind DispToExaminationType(string disp, int age)
        {
            switch (disp.ToUpper())
            {
                case "ОПВ":
                    return ExaminationKind.ProfOsmotr;
                case "ДВ2":
                case "ДВ4":
                    return age >= 40 ? ExaminationKind.Dispanserizacia1 : ExaminationKind.Dispanserizacia3;
                default:
                    return ExaminationKind.None;
            }
        }
        /// <summary>
        /// Опеределяет группу здоровья по результату профилактического осмотра.
        /// </summary>
        /// <param name="RSLT_D">Результат профилактического осмотра.</param>
        /// <returns>Группа здоровья</returns>
        private static HealthGroup RSLT_DToHealthGroup(int RSLT_D)
        {
            switch (RSLT_D)
            {
                case 1:
                    return HealthGroup.First;
                case 2:
                case 12:
                    return HealthGroup.Second;
                case 31:
                case 14:
                    return HealthGroup.ThirdA;
                case 32:
                case 15:
                    return HealthGroup.ThirdB;
                default:
                    return HealthGroup.None;
            }
        }
        /// <summary>
        /// Десериализует коллекцию потоков в список указанного типа T.
        /// </summary>
        /// <typeparam name="T">Тип Т в который десериализуется поток.</typeparam>
        /// <param name="files">Коллекция потоков.</param>
        /// <returns>Список экземпляров типа Т.</returns>
        private static List<T> DeserializeCollection<T>(IEnumerable<Stream> files) where T : class
        {
            var result = new List<T>();

            foreach (var file in files)
            {
                file.Seek(0, SeekOrigin.Begin);

                var formatter = new XmlSerializer(typeof(T));
                var obj = formatter.Deserialize(file);

                result.Add((T)obj);
            }

            return result;
        }
        #endregion

        #region Классы для десериализации случаев реестров-счетов
        /// <summary>
        /// Представляет информацию о законченных случаях реестра-счетов.
        /// </summary>
        [XmlRoot(ElementName = "ZL_LIST")]
        public class ZL_LIST
        {
            /// <summary>
            /// Счет
            /// </summary>
            [XmlElement(ElementName = "SCHET")]
            public SCHET SCHET { get; set; }
            /// <summary>
            /// Записи
            /// </summary>
            [XmlElement(ElementName = "ZAP")]
            public List<ZAP> ZAP { get; set; }
        }
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
        /// <summary>
        /// Представляет информацию о записи в реестре-счетов
        /// </summary>
        [XmlRoot(ElementName = "ZAP")]
        public class ZAP
        {
            /// <summary>
            /// Сведения о пациенте
            /// </summary>
            [XmlElement(ElementName = "PACIENT")]
            public PACIENT PACIENT { get; set; }
            /// <summary>
            /// Сведения о законченном случае
            /// </summary>
            [XmlElement(ElementName = "Z_SL")]
            public Z_SL Z_SL { get; set; }
        }
        /// <summary>
        /// Представляет информацию о пациенте
        /// </summary>
        [XmlRoot(ElementName = "PACIENT")]
        public class PACIENT
        {
            /// <summary>
            /// Guid пациента
            /// </summary>
            [XmlElement(ElementName = "ID_PAC")]
            public Guid ID_PAC { get; set; }
            /// <summary>
            /// Серия полиса ОМС
            /// </summary>
            [XmlElement(ElementName = "SPOLIS")]
            public string SPOLIS { get; set; }
            /// <summary>
            /// Номер полиса ОМС
            /// </summary>
            [XmlElement(ElementName = "NPOLIS")]
            public string NPOLIS { get; set; }
        }
        /// <summary>
        /// Представляет информацию о законченном случае мед. помощи
        /// </summary>
        [XmlRoot(ElementName = "Z_SL")]
        public class Z_SL
        {
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
            /// Случай обращения за мед. помощью
            /// </summary>
            [XmlElement(ElementName = "SL")]
            public SL SL { get; set; }
        }
        /// <summary>
        /// Представляет информацию о случае обращения за мед. помощью
        /// </summary>
        [XmlRoot(ElementName = "SL")]
        public class SL
        {
            /// <summary>
            /// Цель обращения
            /// </summary>
            public string CEL { get; set; }
            /// <summary>
            /// Дата начала лечения
            /// </summary>
            [XmlElement(ElementName = "DATE_1")]
            public DateTime DATE_1 { get; set; }
            /// <summary>
            /// Дата окончания лечения
            /// </summary>
            [XmlElement(ElementName = "DATE_2")]
            public DateTime DATE_2 { get; set; }
            /// <summary>
            /// Список назначений
            /// </summary>
            [XmlElement(ElementName = "NAZ")]
            public List<NAZ> NAZ { get; set; }
            /// <summary>
            /// Список оказанных услуг
            /// </summary>
            [XmlElement(ElementName = "USL")]
            public List<USL> USL { get; set; }
        }
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
        /// <summary>
        /// Представляет информацию об оказанной услуге
        /// </summary>
        [XmlRoot(ElementName = "USL")]
        public class USL
        {
            /// <summary>
            /// Дата начала оказания услуги
            /// </summary>
            [XmlElement(ElementName = "DATE_IN")]
            public DateTime DATE_IN { get; set; }
            /// <summary>
            /// Код услуги
            /// </summary>
            [XmlElement(ElementName = "CODE_USL")]
            public string CODE_USL { get; set; }
        }
        #endregion

        #region Классы для десериализации пациентов реестров-счетов
        /// <summary>
        /// Представляет информацию о пациентах реестра-счетов.
        /// </summary>
        [XmlRoot(ElementName = "PERS_LIST")]
        public class PERS_LIST
        {
            /// <summary>
            /// Список сведений о пациентах
            /// </summary>
            [XmlElement(ElementName = "PERS")]
            public List<PERS> PERS { get; set; }
        }
        /// <summary>
        /// Представляет сведения о пациенте
        /// </summary>
        [XmlRoot(ElementName = "PERS")]
        public class PERS
        {
            /// <summary>
            /// Guid пациента
            /// </summary>
            [XmlElement(ElementName = "ID_PAC")]
            public Guid ID_PAC { get; set; }
            /// <summary>
            /// Фамилия
            /// </summary>
            [XmlElement(ElementName = "FAM")]
            public string FAM { get; set; }
            /// <summary>
            /// Имя
            /// </summary>
            [XmlElement(ElementName = "IM")]
            public string IM { get; set; }
            /// <summary>
            /// Отчество
            /// </summary>
            [XmlElement(ElementName = "OT")]
            public string OT { get; set; }
            /// <summary>
            /// Дата рождения
            /// </summary>
            [XmlElement(ElementName = "DR")]
            public DateTime DR { get; set; }
        }
        #endregion
    }
}

