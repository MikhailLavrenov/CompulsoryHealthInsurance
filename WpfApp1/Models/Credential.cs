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

        private int _RequestsLimit;
        public int RequestsLimit
            { get
                {
                return _RequestsLimit;
                }
            set
                {
                _RequestsLimit = value;
                requestsLeft = value;
                }
            }

        private int requestsLeft;

        private readonly object syncLock = new object();      

        public Credential Copy()
            {
            return MemberwiseClone() as Credential;

            }

        public bool TryReserveRequest()
            {
            lock(syncLock)
                {
                if (requestsLeft != 0)
                    {
                    requestsLeft--;
                    return true;
                    }
                else
                    return false;
                }
            }



        }
    }
