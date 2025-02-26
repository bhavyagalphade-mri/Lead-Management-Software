using Lms.Models;
using LMSWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace LMSWebAPI.Data
{
    public class AppDbContext : DbContext
    {
        internal IEnumerable<object> Users;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> users { get; set; } 

        public DbSet<Lead> leads { get; set; }

        public DbSet<LeadLog> lead_log { get; set; }

        public DbSet<LeadAssignment> lead_assignment { get; set; }

        public DbSet<ActivityLog> activity_log { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LeadLog>().Property(l => l.new_status).HasConversion<string>(); //store enum as string in DB

            modelBuilder.Entity<LeadLog>().ToTable("lead_log");

            modelBuilder.Entity<User>().Property(l => l.role)
                .HasConversion(v => v.ToString(),
                v => (UserRole)Enum.Parse(typeof(UserRole), v)
                ); //store enum as string in DB

            modelBuilder.Entity<ActivityLog>()
                .Property(a => a.action_type).HasConversion<string>();
        }

        
    }
}
