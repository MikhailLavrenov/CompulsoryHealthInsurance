using CHI.Models.ServiceAccounting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services
{
    public class FomsXmlRegisterService
    {
        public FomsXmlRegisterService()
        {
        }

        public FomsXmlRegisterService(string filePath)
        {
        }


        /// <summary>
        /// Получает счет-реестр за один период (отчетный месяц года)
        /// </summary>
        /// <returns></returns>
        public Register GetRegister(IEnumerable<string> filePaths)
        {
            var loader = new XmlBillsLoader(filePaths);
            var billParts = loader.Load();
            var builder = new BillsRegisterBulder();
            builder.Add(billParts);
            var billsRegister = builder.Build();

            return ConvertToRegister(billsRegister);
        }

        /// <summary>
        /// Конвертирует типы xml реестров-счетов в Register.
        /// </summary>
        Register ConvertToRegister(BillsRegister billsRegister)
        {

            var firstFileName = billsRegister.Bills.First().Cases.ZGLV.FILENAME;
            var titleIndex = firstFileName.IndexOfAny("0123456789".ToCharArray());

            var register = new Register()
            {
                Month = billsRegister.Month,
                Year = billsRegister.Year,
                BuildDate = billsRegister.Bills.Max(x => x.Cases.ZGLV.DATA),
                Title = firstFileName.Substring(titleIndex),
            };

            foreach (var bill in billsRegister.Bills)
                foreach (var fomsCase in bill.Cases.ZAP)
                {
                    var foundPatient = bill.Persons.PERS.FirstOrDefault(x => x.ID_PAC == fomsCase.PACIENT.ID_PAC);

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
