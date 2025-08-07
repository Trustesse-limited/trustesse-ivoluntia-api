using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Domain.Entities;


namespace Trustesse.Ivoluntia.Data.DataContext
{
    public class iVoluntiaDataContext : IdentityDbContext<User, Role, string>
    {
        public iVoluntiaDataContext(DbContextOptions<iVoluntiaDataContext> options) : base(options)
        {
        }

        public DbSet<Foundation> Foundations { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<Skill> Skills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Foundation)
                .WithMany(f => f.Admins)
                .HasForeignKey(u => u.FoundationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Skills)
                .WithMany(s => s.Users)
                .UsingEntity(j => j.ToTable("UserSkills"));

            modelBuilder.Entity<User>()
                .HasMany(u => u.Interests)
                .WithMany(i => i.Users)
                .UsingEntity(j => j.ToTable("UserInterests"));

        }
    }



}
