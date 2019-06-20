using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    class CacheDB : DbContext
    {
        public CacheDB()
            : base("DBConnectionString")
        { }

        public DbSet<Patient> Patients { get; set; }
    }
}
