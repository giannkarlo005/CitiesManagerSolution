using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using CitiesManager.Core.Identity;
using CitiesManager.Core.Models;

namespace CitiesManager.Infrastructure.DatabaseContext
{
    public class ApplicationDbContext: IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public virtual DbSet<City> Cities { get; set; }

        public ApplicationDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) 
        { 
        }

        public ApplicationDbContext()
        { 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<City>().HasData(new City()
            {
                CityID = Guid.Parse("6E608BA2-078E-4E16-B567-A0776FB0951D"),
                CityName = "New York"
            });
            modelBuilder.Entity<City>().HasData(new City()
            {
                CityID = Guid.Parse("94EBA773-02E9-4DF4-A6E9-89569C28DCD8"),
                CityName = "London"
            });
        }
    }
}
