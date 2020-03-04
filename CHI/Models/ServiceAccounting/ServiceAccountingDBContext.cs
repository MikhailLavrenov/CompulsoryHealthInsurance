using CHI.Services.AttachedPatients;
using Microsoft.EntityFrameworkCore;

namespace CHI.Models.ServiceAccounting
{
   public  class ServiceAccountingDBContext : DbContext
    {
        public ServiceAccountingDBContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=ServiceAccounting.db");
        }

        public DbSet<Register> Registers { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Medic> Medics { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
    }
}
