using CHI.Models;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;

namespace CHI.Services
{
    public class AppDBContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Register> Registers { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Medic> Medics { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<ServiceClassifierItem> ServiceClassifierItems { get; set; }
        public DbSet<ServiceClassifier> ServiceClassifiers { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<Indicator> Indicators { get; set; }
        public DbSet<CaseFilter> CaseFilters { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (Settings.Instance.UseSQLServer)
                optionsBuilder.UseSqlServer(@$"Server={Settings.Instance.SQLServerName};Database={Settings.Instance.SQLServerDBName};Trusted_Connection=True;");
            else
                optionsBuilder.UseSqlite(@$"Data Source=Database.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Register>()
                .HasMany(x => x.Cases)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Case>()
                .HasMany(x => x.Services)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Department>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Childs)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlanningPermision>()
                .HasKey(bc => new { bc.UserId, bc.DepartmentId });

            modelBuilder.Entity<PlanningPermision>()
                .HasOne(x => x.User)
                .WithMany(x => x.PlanningPermisions)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlanningPermision>()
                .HasOne(x => x.Department)
                .WithMany(x => x.PlanningPermisions)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Component>()
                .HasMany(x => x.Childs)
                .WithOne(x => x.Parent)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Component>()
                .HasMany(x => x.CaseFilters)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Component>()
                .HasMany(x => x.Indicators)
                .WithOne(x => x.Component)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Indicator>()
                .HasMany(x => x.Ratios)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceClassifier>()
                .HasMany(x => x.ServiceClassifierItems)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Service>()
                .HasOne(x => x.ClassifierItem)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Employee>()
                .HasMany(x => x.Parameters)
                .WithOne(x => x.Employee)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
