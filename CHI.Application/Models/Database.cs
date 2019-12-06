using CHI.Services.AttachedPatients;
using System.Data.Entity;

namespace CHI.Application.Models
{
    class Database : DbContext
    {
        public Database()
            : base("DBConnectionString")
        { }

        public DbSet<Patient> Patients { get; set; }
    }
}
