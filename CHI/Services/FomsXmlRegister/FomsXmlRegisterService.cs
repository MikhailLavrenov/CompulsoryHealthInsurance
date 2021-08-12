using CHI.Models.ServiceAccounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CHI.Services
{
    public class FomsXmlRegisterService : FomsXmlRegisterServiceBase
    {
        public FomsXmlRegisterService(IEnumerable<string> filePaths) : base(filePaths)
        {
        }

        public FomsXmlRegisterService(string filePath) : base(filePath)
        {
        }


        /// <summary>
        /// Получает счет-реестр за один период (отчетный месяц года)
        /// </summary>
        /// <returns></returns>
        public Register GetRegister()
        {
            var patientsFiles = GetXmlFiles(new Regex("^L", RegexOptions.IgnoreCase));
            var patientsRegisters = DeserializeXmlFiles<PERS_LIST>(patientsFiles);

            foreach (var file in patientsFiles)
                file.Dispose();

            var casesFiles = GetXmlFiles(new Regex("^(?!L)", RegexOptions.IgnoreCase));
            var casesRegisters = DeserializeXmlFiles<ZL_LIST>(casesFiles);

            foreach (var file in casesFiles)
                file.Dispose();

            return ConvertToRegister(casesRegisters, patientsRegisters);
        }

        /// <summary>
        /// Конвертирует типы xml реестров-счетов в Register.
        /// </summary>
        Register ConvertToRegister(IEnumerable<ZL_LIST> casesRegisters, IEnumerable<PERS_LIST> patientsRegisters)
        {
            foreach (var item in casesRegisters)
                if (casesRegisters.First().SCHET.MONTH != item.SCHET.MONTH || casesRegisters.First().SCHET.YEAR != item.SCHET.YEAR)
                    throw new InvalidOperationException("Реестры должны принадлежать одному периоду");

            var patients = new List<PERS>();

            foreach (var patientsRegister in patientsRegisters)
                patients.AddRange(patientsRegister?.PERS);

            patients = patients.Distinct().ToList();

            var titleIndex = casesRegisters.First().ZGLV.FILENAME.IndexOfAny("0123456789".ToCharArray());

            var register = new Register()
            {
                Month = casesRegisters.First().SCHET.MONTH,
                Year = casesRegisters.First().SCHET.YEAR,
                BuildDate = casesRegisters.Select(x => x.ZGLV.DATA).Max(),
                Title = casesRegisters.First().ZGLV.FILENAME.Substring(titleIndex),
                Cases = new List<Case>()

            };

            foreach (var fomsRegister in casesRegisters)
                foreach (var fomsCase in fomsRegister.ZAP)
                {
                    var foundPatient = patients.FirstOrDefault(x => x.ID_PAC == fomsCase.PACIENT.ID_PAC);

                    if (foundPatient == default)
                        throw new InvalidOperationException($"В xml реестре не найдена информация о пациенте ID_PAC={fomsCase.PACIENT.ID_PAC}, которому были оказаны услуги.");

                    var ageYears = (DateTime.MinValue + (fomsCase.Z_SL.SL.DATE_2 - foundPatient.DR)).Year - 1;

                    var mCase = new Case()
                    {
                        IdCase = fomsCase.Z_SL.SL.SL_ID,
                        Place = fomsCase.Z_SL.USL_OK,
                        VisitPurpose = fomsCase.Z_SL.SL.P_CEL,
                        TreatmentPurpose = fomsCase.Z_SL.SL.CEL,
                        DateEnd = fomsCase.Z_SL.SL.DATE_2,
                        AgeKind = ageYears < 18 ? AgeKind.Сhildren : AgeKind.Adults,
                        BedDays = fomsCase.Z_SL.SL.KD,
                        PaidStatus = (PaidKind)fomsCase.Z_SL.OPLATA,
                        AmountPaid = fomsCase.Z_SL.SUMP,
                        AmountUnpaid = fomsCase.Z_SL.SANK_IT,
                        Employee = Employee.CreateUnknown(fomsCase.Z_SL.SL.IDDOKT, fomsCase.Z_SL.SL.PRVS),
                        Services = new List<Service>()
                    };

                    foreach (var fomsServices in fomsCase.Z_SL.SL.USL)
                    {
                        var service = new Service()
                        {
                            Code = fomsServices.CODE_USL,
                            Count = fomsServices.KOL_USL,
                            Date = fomsServices.DATE_OUT,
                            Employee = Employee.CreateUnknown(fomsServices.CODE_MD, fomsServices.PRVS)
                        };

                        mCase.Services.Add(service);
                    }

                    register.Cases.Add(mCase);

                }

            register.CasesCount = register.Cases.Count;

            return register;
        }
    }
}
