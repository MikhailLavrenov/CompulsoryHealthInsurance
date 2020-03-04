using CHI.Models.ServiceAccounting;
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
        /// Получает счет-реестр за один период (отчетный месяц года)
        /// </summary>
        /// <returns></returns>
        public Register GetRegister()
        {
            var fomsRegistersFiles = GetFiles();
            var fomsRegisters = DeserializeCollection<ZL_LIST>(fomsRegistersFiles);

            foreach (var fomsRegistersFile in fomsRegistersFiles)
                fomsRegistersFile.Dispose();

            return ConvertToRegister(fomsRegisters);
        }
        /// <summary>
        /// Получает список периодических осмотров пациентов из xml файлов реестров-счетов. Среди всех файлов выбирает только необходимые.
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
        /// <param name="fileNamesStartsWith">Коллекция начала имен файлов.</param>
        /// <returns>Список потоков файлов.</returns>
        private List<Stream> GetFiles(IEnumerable<string> fileNamesStartsWith = null)
        {
            var files = new List<Stream>();

            foreach (var filePath in filePaths)
                files.AddRange(GetFilesRecursive(filePath, fileNamesStartsWith));

            return files;
        }
        /// <summary>
        ///  Получает список потоков на файлы по заданному пути и имена которых начинаются с опеределенных строк.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <param name="fileNamesStartsWith">Коллекция начала имен файлов.</param>
        /// <returns>Список потоков файлов.</returns>
        private List<Stream> GetFilesRecursive(string path, IEnumerable<string> fileNamesStartsWith = null)
        {
            var result = new List<Stream>();
            var isDirectory = new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory);

            if (isDirectory)
            {
                var entries = Directory.GetFileSystemEntries(path);

                foreach (var entry in entries)
                    result.AddRange(GetFilesRecursive(entry, fileNamesStartsWith));
            }
            else
            {
                var extension = Path.GetExtension(path);

                if (extension.Equals(".xml", comparer)
                    && (fileNamesStartsWith == null || fileNamesStartsWith.Any(x => Path.GetFileName(path).StartsWith(x, comparer))))
                        result.Add(new FileStream(path, FileMode.Open));


                else if (extension.Equals(".zip", comparer))
                    using (var archive = ZipFile.OpenRead(path))
                    {
                        foreach (var entry in archive.Entries)
                            result.AddRange(ArchiveEntryGetFilesRecursive(entry, fileNamesStartsWith));
                    }
            }

            return result;
        }
        /// <summary>
        /// Получает список потоков на файлы в архиве, имена которых начинаются с опеределенных строк.
        /// </summary>
        /// <param name="archiveEntry">Файл внутри zip архива.</param>
        /// <param name="fileNamesStartsWith">Коллекция начала имен файлов.</param>
        /// <returns>Список потоков файлов.</returns>
        private List<Stream> ArchiveEntryGetFilesRecursive(ZipArchiveEntry archiveEntry, IEnumerable<string> fileNamesStartsWith)
        {
            var result = new List<Stream>();

            if (string.IsNullOrEmpty(archiveEntry.Name))
                return result;

            var extension = Path.GetExtension(archiveEntry.Name);

            if (extension.Equals(".xml", comparer) &&
                (fileNamesStartsWith == null || fileNamesStartsWith.Any(x => archiveEntry.Name.StartsWith(x, comparer))))
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
                        result.AddRange(ArchiveEntryGetFilesRecursive(entry, fileNamesStartsWith));
                }
            }

            return result;
        }
        /// <summary>
        /// Конвертирует типы xml реестров-счетов в список PatientExaminations.
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
        /// Конвертирует типы xml реестров-счетов в Register.
        /// </summary>
        private static Register ConvertToRegister(IEnumerable<ZL_LIST> fomsRegisters)
        {
            foreach (var item in fomsRegisters)
                if (fomsRegisters.First().SCHET.MONTH != item.SCHET.MONTH || fomsRegisters.First().SCHET.YEAR != item.SCHET.YEAR)
                    throw new InvalidOperationException("Реестры должны принадлежать одному периоду");

            var register = new Register()
            {
                Month = fomsRegisters.First().SCHET.MONTH,
                Year = fomsRegisters.First().SCHET.YEAR,
                BuildDate = fomsRegisters.First().ZGLV.DATA
            };

            foreach (var fomsRegister in fomsRegisters)
                foreach (var fomsCase in fomsRegister.ZAP)
                {
                    var mCase = new Case()
                    {
                        Place = fomsCase.Z_SL.USL_OK,
                        VisitPurpose = fomsCase.Z_SL.SL.P_CEL,
                        TreatmentPurpose = fomsCase.Z_SL.SL.CEL,
                        Employee = new Employee(fomsCase.Z_SL.SL.IDDOKT, fomsCase.Z_SL.SL.RPVS)
                    };

                    foreach (var fomsServices in fomsCase.Z_SL.SL.USL)
                    {
                        var service = new Service()
                        {
                            Code = fomsServices.CODE_USL,
                            Count = fomsServices.KOL_USL,
                            Employee = new Employee(fomsServices.CODE_MD, fomsServices.PRVS)
                        };

                        mCase.Services.Add(service);
                    }

                    register.Cases.Add(mCase);
                }
                                          
            return register;
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
    }
}

