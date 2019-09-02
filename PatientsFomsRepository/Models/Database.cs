using System.Data.Entity;

namespace PatientsFomsRepository.Models
{
    class Database : DbContext
    {
        public Database()
            : base("DBConnectionString")
        { }

        public DbSet<Patient> Patients { get; set; }
    }
}
