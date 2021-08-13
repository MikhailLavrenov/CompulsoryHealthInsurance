using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services
{
    public class BillsRegisterBulder
    {
        int month;
        int year;
        List<PERS_LIST> notPairedPersons;
        List<ZL_LIST> notPairedCases;
        BillsRegister billsRegister;


        public BillsRegisterBulder()
        {
            notPairedPersons = new List<PERS_LIST>();
            notPairedCases = new List<ZL_LIST>();
        }

        public void Add(IEnumerable<BillPart> billParts)
        {
            foreach (var billPart in billParts)
            {
                if (billPart is PERS_LIST persons)
                    Add(persons);
                else if (billPart is ZL_LIST cases)
                    Add(cases);
            }
        }

        void Add(PERS_LIST persons)
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

        void Add(ZL_LIST cases)
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

        void Add(BillPair bill)
        {
            if (billsRegister == null)
                billsRegister = new BillsRegister(bill.Cases.SCHET.MONTH, bill.Cases.SCHET.YEAR);

            billsRegister.Add(bill);
        }

        public bool CanBuild()
            => billsRegister != null && notPairedPersons.Any() == false && notPairedCases.Any() == false;

        public BillsRegister Build()
            => CanBuild() ? billsRegister : throw new InvalidOperationException("Невозможно построить реестр, т.к. не сопоставлены все пары файлов либо они отсуствуют.");


    }
}