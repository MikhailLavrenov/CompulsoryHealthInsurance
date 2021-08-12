using System;

namespace CHI.Services
{
    public class BillPair
    {
        public PERS_LIST Persons { get; private set; }
        public ZL_LIST Cases { get; private set; }


        public BillPair(PERS_LIST persons, ZL_LIST cases)
        {
            if (!IsPair(persons, cases))
                new ArgumentException("Элементы счета не являются парными.");

            Persons = persons;
            Cases = cases;
        }


        public static bool IsPair(PERS_LIST persons, ZL_LIST cases)
        {
            if (persons == null || cases == null)
                return false;

            return persons.ZGLV.FILENAME1.Equals(cases.ZGLV.FILENAME, StringComparison.OrdinalIgnoreCase);
        }
    }
}
