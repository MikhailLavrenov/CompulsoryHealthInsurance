using CHI.Models.ServiceAccounting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services
{
    public class FomsXmlRegisterService
    {
        /// <summary>
        /// Получает счет-реестр за один период (отчетный месяц года)
        /// </summary>
        /// <param name="filePaths">Путь к xml файлам реестров-счетов. (может быть папками, xml файлами и/или zip архивами)</param>
        /// <returns></returns>
        public Register GetRegister(IEnumerable<string> filePaths)
        {
            var xmlLoader = new XmlBillsLoader();
            xmlLoader.Load(filePaths);
            var billsRegister = BillsRegister.Create(xmlLoader.PersonsBills, xmlLoader.CasesBills);

            return GetRegisterInternal(billsRegister);
        }

        Register GetRegisterInternal(BillsRegister billsRegister)
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
            {
                var billPersons = bill.Persons.PERS.ToDictionary(x => x.ID_PAC, x => x);

                foreach (var billCase in bill.Cases.ZAP)
                {
                    var foundPatient = billPersons[billCase.PACIENT.ID_PAC];

                    var ageYears = (DateTime.MinValue + (billCase.Z_SL.SL.DATE_2 - foundPatient.DR)).Year - 1;

                    var mCase = new Case()
                    {
                        IdCase = billCase.Z_SL.SL.SL_ID,
                        Place = billCase.Z_SL.USL_OK,
                        VisitPurpose = billCase.Z_SL.SL.P_CEL,
                        TreatmentPurpose = billCase.Z_SL.SL.CEL,
                        DateEnd = billCase.Z_SL.SL.DATE_2,
                        AgeKind = ageYears < 18 ? AgeKind.Сhildren : AgeKind.Adults,
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
                            Employee = Employee.CreateUnknown(fomsServices.CODE_MD, fomsServices.PRVS)
                        };

                        mCase.Services.Add(service);
                    }

                    register.Cases.Add(mCase);
                }
            }

            register.CasesCount = register.Cases.Count;

            return register;
        }
    }
}
