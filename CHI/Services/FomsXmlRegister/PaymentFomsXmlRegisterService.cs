using CHI.Models.ServiceAccounting;
using CHI.Services.CasesPaymentDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CHI.Services.BillsRegister
{
    public class PaymentFomsXmlRegisterService : FomsXmlRegisterServiceBase
    {
        public PaymentFomsXmlRegisterService(IEnumerable<string> filePaths) : base(filePaths)
        {
        }

        public PaymentFomsXmlRegisterService(string filePath) : base(filePath)
        {
        }


        /// <summary>
        /// Получает счет-реестр за один период (отчетный месяц года)
        /// </summary>
        /// <returns></returns>
        public Register GetRegister()
        {
            var fomsRegistersFiles = GetXmlFiles(new Regex("^(?!L)", RegexOptions.IgnoreCase));
            var fomsRegisters = DeserializeXmlFiles<ZL_LIST>(fomsRegistersFiles);

            foreach (var fomsRegistersFile in fomsRegistersFiles)
                fomsRegistersFile.Dispose();

            return ConvertToRegisterWithPayment(fomsRegisters);

        }

        /// <summary>
        /// Конвертирует типы xml реестров-счетов в Register.
        /// </summary>
        Register ConvertToRegisterWithPayment(IEnumerable<ZL_LIST> fomsRegisters)
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
