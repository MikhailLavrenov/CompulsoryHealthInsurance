using CHI.Services.MedicalExaminations;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CHI.Services
{
    public class ExaminationsFomsXmlRegisterService : FomsXmlRegisterServiceBase
    {
        public ExaminationsFomsXmlRegisterService(IEnumerable<string> filePaths)
            : base(filePaths)
        {
        }
        public ExaminationsFomsXmlRegisterService(string filePath)
            : base(filePath)
        {
        }


        /// <summary>
        /// Получает список периодических осмотров пациентов из xml файлов реестров-счетов. Среди всех файлов выбирает только необходимые.
        /// </summary>
        /// <param name="examinationsFileNamesStartsWith">Коллекция начала имен файлов с услугами.</param>
        /// <param name="patientsFileNamesStartsWith">Коллекция начала имен файлов с пациентами.</param>
        /// <returns>Список профилактических осмотров пациентовю</returns>
        public List<PatientExaminations> GetPatientsExaminations(Regex fileNameMatchPattern)
        {
            var patientsFiles = GetXmlFiles(fileNameMatchPattern);
            var patientsRegisters = DeserializeXmlFiles<PERS_LIST>(patientsFiles);

            foreach (var file in patientsFiles)
                file.Dispose();

            var examinationsFiles = GetXmlFiles(fileNameMatchPattern);
            var examinationsRegisters = DeserializeXmlFiles<ZL_LIST>(examinationsFiles);

            foreach (var file in examinationsFiles)
                file.Dispose();

            return ConvertToPatientExaminations(examinationsRegisters, patientsRegisters);
        }

        /// <summary>
        /// Конвертирует типы xml реестров-счетов в список PatientExaminations.
        /// </summary>
        /// <param name="examinationsRegisters">Десериализованные классы услуг xml реестров-счетов.</param>
        /// <param name="patientsRegisters">>Десериализованные классы пациентов xml реестров-счетов.</param>
        /// <returns>Список профилактических осмотров пациентов.</returns>
        List<PatientExaminations> ConvertToPatientExaminations(IEnumerable<ZL_LIST> examinationsRegisters, IEnumerable<PERS_LIST> patientsRegisters)
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
                        examination.BeginDate = treatmentCase.Z_SL.SL.USL.First(x => x.CODE_USL == 024101).DATE_IN;
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
        int DispToExaminationStage(string disp)
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
        ExaminationKind DispToExaminationType(string disp, int age)
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
        HealthGroup RSLT_DToHealthGroup(int RSLT_D)
        {
            switch (RSLT_D)
            {
                case 1:
                    return HealthGroup.First;
                case 2:
                case 12:
                    return HealthGroup.Second;
                case 31:
                case 33:
                case 14:
                    return HealthGroup.ThirdA;
                case 32:
                case 34:
                case 15:
                    return HealthGroup.ThirdB;
                default:
                    return HealthGroup.None;
            }
        }

    }
}
