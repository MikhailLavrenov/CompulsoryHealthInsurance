using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FomsPatientsDB.Models
    {
    public class Credential
        {
        public string Login { get; set; }
        public string Password { get; set; }
        public int RequestsLeft { get; set; }

        public Credential Copy()
            {
            return MemberwiseClone() as Credential;

            }




        }
    }
