using CHI.Services.AttachedPatients;
using Microsoft.EntityFrameworkCore;

namespace CHI.Models
{
    class AttachedPatientsDBContext : DbContext
    {
        public AttachedPatientsDBContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=AttachedPAtients.db");
        }

        public DbSet<Patient> Patients { get; set; }
    }
}
