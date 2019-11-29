using CHI.Services.MedicalExaminations;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;

namespace CHI.Services.BillsRegister
{
    public class BillsRegisterService
    {
        #region Поля
        private static readonly StringComparison comparer = StringComparison.OrdinalIgnoreCase;
        private List<string> filePaths;
        #endregion

        #region Конструкторы
        public BillsRegisterService(ICollection<string> filePaths)
        {
            this.filePaths = filePaths.ToList();
        }
        public BillsRegisterService(string filePath)
        {
            filePaths = new List<string>() { filePath };
        }
        #endregion

        #region Методы
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

            return GetPatientsExaminations(examinationsRegisters, patientsRegisters);
        }
        private List<Stream> GetFiles(IEnumerable<string> fileNamesStartsWithFilter)
        {
            var files = new List<Stream>();

            foreach (var filePath in filePaths)
                files.AddRange(GetFilesRecursive(filePath, fileNamesStartsWithFilter));

            return files;
        }
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
        private List<PatientExaminations> GetPatientsExaminations(IEnumerable<ZL_LIST> examinationsRegisters, IEnumerable<PERS_LIST> patientsRegisters)
        {
            var result = new List<PatientExaminations>();

            var patients = new List<(Guid, int)>();

            foreach (var patientsRegister in patientsRegisters)
                foreach (var patient in patientsRegister.PERS)
                    patients.Add((patient.ID_PAC, patient.DR.Year));

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

                    var insuranceNumber = $@"{treatmentCase.PACIENT.SPOLIS}{treatmentCase.PACIENT.NPOLIS}";

                    if (string.IsNullOrEmpty(insuranceNumber))
                        continue;

                    var examination = new Examination();

                    var foundPatient = patients.FirstOrDefault(x => x.Item1 == treatmentCase.PACIENT.ID_PAC);

                    if (foundPatient == default)
                        continue;

                    var examinationKind = DispToExaminationType(examinationsRegister.SCHET.DISP, examinationYear - foundPatient.Item2);

                    if (examinationStage == 1)
                        examination.BeginDate = treatmentCase.Z_SL.SL.USL.First(x => x.CODE_USL == "024101").DATE_IN;
                    else
                        examination.BeginDate = treatmentCase.Z_SL.SL.DATE_1;

                    examination.EndDate = treatmentCase.Z_SL.SL.DATE_2;
                    examination.HealthGroup = RSLT_DToHealthGroup(treatmentCase.Z_SL.RSLT_D);
                    examination.Referral = (ExaminationReferral)(treatmentCase.Z_SL.SL.NAZ.FirstOrDefault()?.NAZ_R ?? 0);

                    if (examination.HealthGroup == ExaminationHealthGroup.None)
                        continue;

                    var patientExamination = result.FirstOrDefault(x => x.InsuranceNumber.Equals(insuranceNumber, comparer) && x.Year == examinationYear && x.Kind == examinationKind);

                    if (patientExamination == default)
                        patientExamination = new PatientExaminations(insuranceNumber, examinationYear, examinationKind);

                    if (examinationStage == 1)
                        patientExamination.Stage1 = examination;
                    else if (examinationStage == 2)
                        patientExamination.Stage2 = examination;

                    result.Add(patientExamination);
                }
            }

            return result;
        }
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
        private static ExaminationHealthGroup RSLT_DToHealthGroup(int RSLT_D)
        {
            switch (RSLT_D)
            {
                case 1:
                    return ExaminationHealthGroup.First;
                case 2:
                case 12:
                    return ExaminationHealthGroup.Second;
                case 31:
                case 14:
                    return ExaminationHealthGroup.ThirdA;
                case 32:
                case 15:
                    return ExaminationHealthGroup.ThirdB;
                default:
                    return ExaminationHealthGroup.None;
            }
        }
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
        [XmlRoot(ElementName = "ZL_LIST")]
        public class ZL_LIST
        {
            //Счёт
            [XmlElement(ElementName = "SCHET")]
            public SCHET SCHET { get; set; }
            //Записи
            [XmlElement(ElementName = "ZAP")]
            public List<ZAP> ZAP { get; set; }
        }

        [XmlRoot(ElementName = "SCHET")]
        public class SCHET
        {
            //год
            [XmlElement(ElementName = "YEAR")]
            public int YEAR { get; set; }
            //Реестровый номер медицинской организации
            [XmlElement(ElementName = "CODE_MO")]
            public string CODE_MO { get; set; }
            //Тип диспансеризации
            //ДВ2	Второй этап диспансеризации определенных групп взрослого населения с периодичностью 1 раз в 3 года
            //ОПВ	Профилактические медицинские осмотры взрослого населения
            //ДВ4	Первый этап диспансеризации определенных групп взрослого населения с периодичностью 1 раз в год
            [XmlElement(ElementName = "DISP")]
            public string DISP { get; set; }
        }

        [XmlRoot(ElementName = "ZAP")]
        public class ZAP
        {
            //Сведения о пациенте
            [XmlElement(ElementName = "PACIENT")]
            public PACIENT PACIENT { get; set; }
            //Сведения о законченном случае
            [XmlElement(ElementName = "Z_SL")]
            public Z_SL Z_SL { get; set; }
        }

        [XmlRoot(ElementName = "PACIENT")]
        public class PACIENT
        {
            //guid пациента
            [XmlElement(ElementName = "ID_PAC")]
            public Guid ID_PAC { get; set; }
            //Серия документа, подтверждающего факт страхования по ОМС
            [XmlElement(ElementName = "SPOLIS")]
            public string SPOLIS { get; set; }
            //Номер документа, подтверждающего факт страхования по ОМС
            [XmlElement(ElementName = "NPOLIS")]
            public string NPOLIS { get; set; }
        }

        [XmlRoot(ElementName = "Z_SL")]
        public class Z_SL
        {
            //результат диспансеризации
            //1	Присвоена I группа здоровья
            //2	Присвоена II группа здоровья
            //12 Направлен на II этап профилактического медицинского осмотра несовершеннолетних или диспансеризации всех типов, предварительно присвоена II группа здоровья
            //3	Присвоена III группа здоровья
            //14 Направлен на II этап диспансеризации определенных групп взрослого населения, предварительно присвоена IIIа группа здоровья
            //31 Присвоена IIIа группа здоровья	
            //15 Направлен на II этап диспансеризации определенных групп взрослого населения, предварительно присвоена IIIб группа здоровья
            //32 Присвоена IIIб группа здоровья
            [XmlElement(ElementName = "RSLT_D")]
            public int RSLT_D { get; set; }
            //сведения о случае
            [XmlElement(ElementName = "SL")]
            public SL SL { get; set; }
        }

        [XmlRoot(ElementName = "SL")]
        public class SL
        {
            //Цель обращения
            public string CEL { get; set; }
            [XmlElement(ElementName = "DATE_1")]
            //Дата начала лечения
            public DateTime DATE_1 { get; set; }
            //Дата оконча-ния лечения
            [XmlElement(ElementName = "DATE_2")]
            public DateTime DATE_2 { get; set; }
            //Назначения
            [XmlElement(ElementName = "NAZ")]
            public List<NAZ> NAZ { get; set; }
            //Услуги
            [XmlElement(ElementName = "USL")]
            public List<USL> USL { get; set; }
        }

        [XmlRoot(ElementName = "NAZ")]
        public class NAZ
        {
            //Вид назначения
            //1 – направлен на консультацию в медицинскую организацию по месту прикрепления;
            //2 – направлен на консультацию в иную медицинскую организацию;
            //3 – направлен на обследование;
            //4 – направлен в дневной стационар;
            //5 – направлен на госпитализацию;
            //6 – направлен в реабилитационное отделение.
            [XmlElement(ElementName = "NAZ_R")]
            public int NAZ_R { get; set; }
        }

        [XmlRoot(ElementName = "USL")]
        public class USL
        {
            //Дата начала оказания услуги
            [XmlElement(ElementName = "DATE_IN")]
            public DateTime DATE_IN { get; set; }
            //Код услуги
            [XmlElement(ElementName = "CODE_USL")]
            public string CODE_USL { get; set; }
        }
        #endregion

        #region Классы для десериализации пациентов реестров-счетов
        [XmlRoot(ElementName = "PERS_LIST")]
        public class PERS_LIST
        {
            //сведения о пациенте
            [XmlElement(ElementName = "PERS")]
            public List<PERS> PERS { get; set; }
        }

        [XmlRoot(ElementName = "PERS")]
        public class PERS
        {
            //guid пациента
            [XmlElement(ElementName = "ID_PAC")]
            public Guid ID_PAC { get; set; }
            //Дата рождения
            [XmlElement(ElementName = "DR")]
            public DateTime DR { get; set; }
        }
        #endregion
    }
}

