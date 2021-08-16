using CHI.Services.MedicalExaminations;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services
{
    public class ExaminationsFomsXmlRegisterService
    {
        /// <summary>
        /// Получает список периодических осмотров пациентов из xml файлов реестров-счетов. Среди всех файлов выбирает только необходимые.
        /// </summary>
        /// <param name="filePaths">Путь к xml файлам реестров-счетов. (может быть папками, xml файлами и/или zip архивами)</param>
        /// <returns></returns>
        public List<PatientExaminations> GetPatientExaminationsList(IEnumerable<string> filePaths)
        {
            var xmlLoader = new XmlBillsLoader();
            xmlLoader.Load(filePaths);
            var billsRegister = BillsRegister.Create(xmlLoader.PersonsBills, xmlLoader.CasesBills);

            return GetPatientExaminationsListInternal(billsRegister);
        }

        List<PatientExaminations> GetPatientExaminationsListInternal(BillsRegister billsRegister)
        {
            var result = new Dictionary<string, PatientExaminations>();

            foreach (var bill in billsRegister.Bills)
            {
                int examinationStage;

                if (!TryGetExaminationStage(bill.Cases.SCHET.DISP, out examinationStage))
                    continue;

                var billPersons = bill.Persons.PERS.ToDictionary(x => x.ID_PAC, x => x);

                foreach (var billCase in bill.Cases.ZAP)
                {
                    var examination = new Examination();

                    if (examinationStage == 1)
                        examination.BeginDate = billCase.Z_SL.SL.USL.First(x => x.CODE_USL == 24101).DATE_IN;
                    else
                        examination.BeginDate = billCase.Z_SL.SL.DATE_1;

                    examination.EndDate = billCase.Z_SL.SL.DATE_2;

                    if (TryGetHealthGroup(billCase.Z_SL.RSLT_D, out var healthGroup))
                        continue;

                    examination.HealthGroup = healthGroup;

                    var naz_r = billCase.Z_SL.SL.NAZ.FirstOrDefault()?.NAZ_R ?? 0;

                    examination.Referral = GetRefferal(healthGroup, naz_r);

                    string insuranceNumber;

                    if (string.IsNullOrEmpty(billCase.PACIENT.SPOLIS))
                        insuranceNumber = billCase.PACIENT.NPOLIS;
                    else
                        insuranceNumber = $"{billCase.PACIENT.SPOLIS} {billCase.PACIENT.NPOLIS}";

                    var examinationYear = billCase.Z_SL.SL.DATE_2.Year;

                    var patient = billPersons[billCase.PACIENT.ID_PAC];

                    var examinationKind = GetExaminationType(bill.Cases.SCHET.DISP, examinationYear - patient.DR.Year);

                    PatientExaminations patientExamination;

                    var key = $"{insuranceNumber}{examinationYear}{examinationKind}".ToUpper();
                    if (!result.TryGetValue(key, out patientExamination))
                    {
                        patientExamination = new PatientExaminations()
                        {
                            InsuranceNumber = insuranceNumber,
                            Year = examinationYear,
                            Kind = examinationKind,
                            Surname = patient.FAM,
                            Name = patient.IM,
                            Patronymic = patient.OT,
                            Birthdate = patient.DR
                        };

                        result.Add(key, patientExamination);
                    }

                    patientExamination.AddStage(examinationStage, examination);
                }
            }

            return result.Values.ToList();
        }

        bool TryGetExaminationStage(string DISP, out int stage)
        {
            switch (DISP.ToUpper())
            {
                case "ОПВ":
                case "ДВ4":
                    stage = 1;
                    return true;
                case "ДВ2":
                    stage = 2;
                    return true;
                default:
                    stage = 0;
                    return false;
            }
        }

        ExaminationKind GetExaminationType(string DISP, int ageOnConsultationYear)
        {
            switch (DISP.ToUpper())
            {
                case "ОПВ":
                    return ExaminationKind.ProfOsmotr;
                case "ДВ2":
                case "ДВ4":
                    return ageOnConsultationYear >= 40 ? ExaminationKind.Dispanserizacia1 : ExaminationKind.Dispanserizacia3;
                default:
                    return ExaminationKind.None;
            }
        }

        bool TryGetHealthGroup(int RSLT_D, out HealthGroup healthGroup)
        {
            switch (RSLT_D)
            {
                case 1:
                    healthGroup = HealthGroup.First;
                    return true;
                case 2:
                case 12:
                    healthGroup = HealthGroup.Second;
                    return true;
                case 31:
                case 33:
                case 14:
                    healthGroup = HealthGroup.ThirdA;
                    return true;
                case 32:
                case 34:
                case 15:
                    healthGroup = HealthGroup.ThirdB;
                    return true;
                default:
                    healthGroup = HealthGroup.None;
                    return false;
            }
        }

        Referral GetRefferal(HealthGroup healthGroup, int NAZ_R = 0)
        {
            //В ТАП реестра-счетов может быть не заполнено назначение, поэтому используется бизнес правило
            if (NAZ_R == 0)
                switch (healthGroup)
                {
                    case HealthGroup.ThirdA:
                    case HealthGroup.ThirdB:
                        return Referral.LocalClinic;
                    default:
                        return Referral.No;
                }

            return (Referral)NAZ_R;
        }
    }
}
