using CHI.Models.ServiceAccounting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services
{
    public class PaymentFomsXmlRegisterService
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

        /// <summary>
        /// Конвертирует типы xml реестров-счетов в Register.
        /// </summary>
        Register GetRegisterInternal(BillsRegister billsRegister)
        {
            foreach (var item in fomsRegisters)
                if (fomsRegisters.First().SCHET.MONTH != item.SCHET.MONTH || fomsRegisters.First().SCHET.YEAR != item.SCHET.YEAR)
                    throw new InvalidOperationException("Реестры должны принадлежать одному периоду");

            var titleIndex = fomsRegisters.First().ZGLV.FILENAME.IndexOfAny("0123456789".ToCharArray());

            var register = new Register()
            {
                Month = fomsRegisters.First().SCHET.MONTH,
                Year = fomsRegisters.First().SCHET.YEAR,
                BuildDate = fomsRegisters.First().ZGLV.DATA,
                Title = fomsRegisters.First().ZGLV.FILENAME.Substring(titleIndex),
                Cases = new List<Case>()

            };

            foreach (var fomsRegister in fomsRegisters)
                foreach (var fomsCase in fomsRegister.ZAP)
                {
                    var mCase = new Case()
                    {
                        IdCase = fomsCase.Z_SL.SL.SL_ID,
                        PaidStatus = (PaidKind)fomsCase.Z_SL.OPLATA,
                        AmountPaid = fomsCase.Z_SL.SUMP,
                        AmountUnpaid = fomsCase.Z_SL.SANK_IT,
                    };

                    register.Cases.Add(mCase);

                }

            register.CasesCount = register.Cases.Count;

            return register;
        }

    }
}
