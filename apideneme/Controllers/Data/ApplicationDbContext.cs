using Microsoft.EntityFrameworkCore;
using apideneme.models2;

namespace apideneme.Controllers.Data
{
    // Veritabanı ile konuşan ana sınıf
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() { }
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Tablolar (DbSet’ler)
       
        public DbSet<Info> Info { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Login> login { get; set; }


        // Model konfigürasyonları ve UUID ayarı
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tüm GUID'leri PostgreSQL'de UUID olarak tanımla
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(Guid))
                    {
                        property.SetColumnType("uuid");
                    }
                }
            }
            

            modelBuilder.Entity<User>()
        .HasOne(u => u.location)
        .WithMany(l => l.User)
        .HasForeignKey(u => u.LocationId)
        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
        .HasOne(u => u.login)
        .WithMany(l => l.User)
        .HasForeignKey(u => u.LoginId)
        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>().OwnsOne(u => u.Registered);
            modelBuilder.Entity<User>().OwnsOne(u => u.Dob);
            modelBuilder.Entity<User>().OwnsOne(u => u.Id);
            modelBuilder.Entity<User>().OwnsOne(u => u.Name);
            modelBuilder.Entity<User>().OwnsOne(u => u.Picture);
            modelBuilder.Entity<Location>().OwnsOne(l => l.Street); 
            modelBuilder.Entity<Location>().OwnsOne(l => l.Coordinates);
            modelBuilder.Entity<Location>().OwnsOne(l => l.Timezone);


        }
    }
}
