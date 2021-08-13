using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services
{
    public class BillsRegister
    {
        public int Month { get; }
        public int Year { get; }
        public List<BillPair> Bills { get; }


        public BillsRegister(int month, int year)
        {
            Month = month;
            Year = year;
            Bills = new List<BillPair>();
        }


        public void Add(BillPair pair)
        {
            if (pair.Cases.SCHET.MONTH != Month || pair.Cases.SCHET.YEAR != Year)
                throw new ArgumentException("Реестр не может состоять из счетов за разные отчетные периоды.");

            Bills.Add(pair);
        }

        public static BillsRegister Create(IEnumerable<PERS_LIST> personsBills, IEnumerable<ZL_LIST> casesBills)
        {
            if (personsBills == null || !personsBills.Any())
                throw new ArgumentException("Реестр должен содержать минимум 1 файл пациентов.");

            if (casesBills == null || !casesBills.Any())
                throw new ArgumentException("Реестр должен содержать минимум 1 файл случаев.");

            var register = new BillsRegister(casesBills.First().SCHET.MONTH, casesBills.First().SCHET.YEAR);

            foreach (var personsBill in personsBills)
            {
                var casesBill = casesBills.Where(x => BillPair.IsPair(personsBill, x)).FirstOrDefault();

                if (casesBill == null)
                    throw new InvalidOperationException("Не удалось сопоставить все пары файлов счетов.");

                register.Add(new BillPair(personsBill, casesBill));
            }

            return register;
        }
    }
}
