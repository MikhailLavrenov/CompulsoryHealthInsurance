using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class Register
    {
        public int Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime BuildDate { get; set; }
        public string Title { get; set; }
        public List<Case> Cases { get; set; }

        public Register()
        {
            Cases = new List<Case>();
        }
    }
}
