using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services
{
    public class FomsRegister
    {
        int month;
        int year;
        List<PERS_LIST> notPairedPersons;
        List<ZL_LIST> notPairedCases;

        public List<BillPair> Bills { get; private set; }


        public FomsRegister()
        {
            notPairedPersons = new List<PERS_LIST>();
            notPairedCases = new List<ZL_LIST>();
            Bills = new List<BillPair>();

        }


        public void Add(BillPair pair)
        {
            if (Bills.Count == 0)
            {
                month = pair.Cases.SCHET.MONTH;
                year = pair.Cases.SCHET.YEAR;
            }
            else if (pair.Cases.SCHET.MONTH != month || pair.Cases.SCHET.YEAR != year)
                throw new ArgumentException("Реестр не может состоять из счетов за разные отчетные периоды");

            Bills.Add(pair);
        }

        public void Add(PERS_LIST persons)
        {
            if (persons == null)
                throw new ArgumentNullException(nameof(persons));

            var cases = notPairedCases.Where(x => BillPair.IsPair(persons, x)).FirstOrDefault();

            if (cases == null)
            {
                notPairedPersons.Add(persons);
                return;
            }

            notPairedCases.Remove(cases);
            var pair = new BillPair(persons, cases);
            Add(pair);
        }

        public void Add(ZL_LIST cases)
        {
            if (cases == null)
                throw new ArgumentNullException(nameof(cases));

            var persons = notPairedPersons.Where(x => BillPair.IsPair(x, cases)).FirstOrDefault();

            if (persons == null)
            {
                notPairedCases.Add(cases);
                return;
            }

            notPairedPersons.Remove(persons);
            var pair = new BillPair(persons, cases);
            Add(pair);
        }
    }
}
