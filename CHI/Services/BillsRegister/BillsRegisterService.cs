using CHI.Models.ServiceAccounting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services
{
    /// <summary>
    /// Получает счет-реестр за один период (отчетный месяц года)
    /// </summary>
    public class BillsRegisterService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePaths">Пути к xml файлам реестров-счетов. (может быть папками, xml файлами и/или zip архивами)</param>
        /// <returns></returns>
        public Register GetRegister(IEnumerable<string> filePaths)
        {
            var xmlLoader = new XmlBillsLoader();
            xmlLoader.Load(filePaths);

            var billsRegister = BillsRegister.Create(xmlLoader.PersonsBills, xmlLoader.CasesBills);

            var register = new Register()
            {
                Month = billsRegister.Month,
                Year = billsRegister.Year,
                BuildDate = billsRegister.Bills.Max(x => x.Cases.ZGLV.DATA),
                Title = GetTitle(billsRegister.Bills.First().Cases.ZGLV.FILENAME),
            };

            foreach (var bill in billsRegister.Bills)
            {
                //добавлен DistinctBy потому-что в реестрах МЭК пациент может дублироваться хотя в реестре ЛПУ этого не было
                var billPersons = bill.Persons.PERS.DistinctBy(x => x.ID_PAC).ToDictionary(x => x.ID_PAC, x => x);

                foreach (var zap in bill.Cases.ZAP)
                {
                    var pers = billPersons[zap.PACIENT.ID_PAC];

                    var mCase = MapToCase(bill.BillRegisterCode, pers, zap);

                    register.Cases.Add(mCase);
                }
            }

            register.CasesCount = register.Cases.Count;

            return register;
        }

        public List<FlkCaseEntry> GetFlkCases(IEnumerable<string> filePaths)
        {
            var xmlLoader = new XmlBillsLoader();
            xmlLoader.Load(filePaths);

            List<FlkCaseEntry> flkCaseEntries = new();

            foreach (var flkp in xmlLoader.FlkpList)
                foreach (var zap in flkp.ZAP)
                    flkCaseEntries.Add(new FlkCaseEntry(flkp.SCHET.CODE, zap.SLUCH.IDCASE, zap.SLUCH.NHISTORY, zap.SLUCH.OTKAZ?.Any() ?? false));

            return flkCaseEntries;
        }

        static Case MapToCase(int billRegisterCode, PERS billPerson, ZAP billCase)
        {
            var mCase = new Case()
            {
                CloseCaseCode = billCase.Z_SL.IDCASE,
                IdCase = billCase.Z_SL.SL.SL_ID,
                MedicalHistoryNumber = billCase.Z_SL.SL.NHISTORY,
                BillRegisterCode = billRegisterCode,
                Place = billCase.Z_SL.USL_OK,
                VisitPurpose = billCase.Z_SL.SL.P_CEL,
                TreatmentPurpose = billCase.Z_SL.SL.CEL,
                DateEnd = billCase.Z_SL.SL.DATE_2,
                AgeKind = GetAgeKind(billCase.Z_SL.SL.DATE_2, billPerson.DR),
                BedDays = billCase.Z_SL.SL.KD,
                PaidStatus = (PaidKind)billCase.Z_SL.OPLATA,
                AmountPaid = billCase.Z_SL.SUMP,
                AmountUnpaid = billCase.Z_SL.SANK_IT,
                Employee = Employee.CreateUnknown(billCase.Z_SL.SL.IDDOKT, billCase.Z_SL.SL.PRVS),
                Services = new List<Service>()
            };

            foreach (var fomsServices in billCase.Z_SL.SL.USL)
            {
                var service = new Service()
                {
                    Code = fomsServices.CODE_USL,
                    Count = fomsServices.KOL_USL,
                    Date = fomsServices.DATE_OUT,
                    Employee = Employee.CreateUnknown(fomsServices.MR_USL_N?.CODE_MD ?? fomsServices.CODE_MD, fomsServices.MR_USL_N?.PRVS ?? fomsServices.PRVS)
                };

                mCase.Services.Add(service);
            }

            return mCase;
        }

        static AgeKind GetAgeKind(DateTime onDate, DateTime birthday)
        {
            var ageYears = (DateTime.MinValue + (onDate - birthday)).Year - 1;
            return ageYears < 18 ? AgeKind.Сhildren : AgeKind.Adults;
        }

        static string GetTitle(string anyFileName)
        {
            var index = anyFileName.IndexOfAny("0123456789".ToCharArray());
            return anyFileName.Substring(index);
        }
    }
}
