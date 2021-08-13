using System;
using System.Collections.Generic;

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
    }
}
