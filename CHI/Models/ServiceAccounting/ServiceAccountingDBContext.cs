using Microsoft.EntityFrameworkCore;

namespace CHI.Models.ServiceAccounting
{
    public class ServiceAccountingDBContext : DbContext
    {
        public DbSet<Register> Registers { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Medic> Medics { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<ServiceClassifier> ServicesClassifier { get; set; }
        public DbSet<Component> Components { get; set; }

        public ServiceAccountingDBContext()
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=ServiceAccounting.db");
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

            //modelBuilder.Entity<Employee>()
            //    .HasMany<Case>()
            //    .WithOne(x => x.Employee)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Medic>()    
            //    .HasMany<Employee>()    
            //    .WithOne(x => x.Medic)   
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Specialty>()   
            //    .HasMany<Employee>()    
            //    .WithOne(x => x.Specialty)   
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Department>()   
            //    .HasMany<Employee>()  
            //    .WithOne(x => x.Department)   
            //    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
