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
        public DbSet<Location> Locations { get; set; }
        public DbSet<OnboardingProgress> OnboardingProgress { get; set; }
        public DbSet<FoundationCategory> FoundationCategories { get; set; }
        public DbSet<Cause> Causes { get; set; }
        public DbSet<ContactInfo> ContactInfos { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationChannel> NotificationChannels { get; set; }
        public DbSet<NotificationChannelSettings> NotificationChannelSettings { get; set; }
        public DbSet<NotificationPriority> NotificationPriorities { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<NotificationTypePriority> NotificationTypePriorities { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<UserInterestLink> UserInterestLinks { get; set; }
        public DbSet<UserSkillLink> UserSkillLinks { get; set; }
        public DbSet<ProgramSkill> ProgramSkills { get; set; }
        public DbSet<ProgramGoal> ProgramGoals { get; set; }
        public DbSet<Program> Programs { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {

                entity.HasOne(u => u.Foundation)
                      .WithMany(f => f.Admins)
                      .HasForeignKey(u => u.FoundationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Location)
                      .WithOne(l => l.User)
                      .HasForeignKey<Location>(l => l.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Skills)
                      .WithMany(s => s.Users)
                      .UsingEntity(j => j.ToTable("UserSkills"));

                entity.HasMany(u => u.Interests)
                      .WithMany(i => i.Users)
                      .UsingEntity(j => j.ToTable("UserInterests"));


                entity.Property(u => u.FirstName)
                      .HasMaxLength(50);

                entity.Property(u => u.LastName)
                      .HasMaxLength(50);

                entity.Property(u => u.Bio)
                      .HasMaxLength(500);
            });

            modelBuilder.Entity<Foundation>(entity =>
            {
                entity.HasOne(u => u.Location)
                     .WithOne(l => l.Foundation)
                     .HasForeignKey<Location>(l => l.FoundationId)
                     .OnDelete(DeleteBehavior.Cascade);

                entity.Property(u => u.Name)
                      .HasMaxLength(80);

                entity.Property(u => u.Mission)
                     .HasMaxLength(2000);

                entity.HasMany(u => u.Causes)
                       .WithMany(i => i.Foundations)
                       .UsingEntity(j => j.ToTable("FoundationCauses"));

                entity.HasOne(f => f.Category)
                      .WithMany(c => c.Foundations)
                      .HasForeignKey(f => f.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<Interest>(entity =>
            {

                entity.Property(u => u.Name)
                      .HasMaxLength(50);

                entity.Property(u => u.Description)
                      .HasMaxLength(500);
            });

            modelBuilder.Entity<Skill>(entity =>
            {

                entity.Property(u => u.Name)
                      .HasMaxLength(50);

                entity.Property(u => u.Description)
                      .HasMaxLength(500);
            });

            modelBuilder.Entity<Location>(entity =>
            {

                entity.Property(u => u.Address)
                      .HasMaxLength(500);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId);
            });

            modelBuilder.Entity<NotificationChannelSettings>(entity =>
            {
                entity.HasOne(s => s.NotificationChannel)
                      .WithMany(c => c.ChannelSettings)
                      .HasForeignKey(s => s.NotificationChannelId);
            });

            modelBuilder.Entity<UserSkillLink>()
                  .HasKey(us => new { us.UserId, us.SkillId });

            modelBuilder.Entity<UserSkillLink>()
                  .HasOne(us => us.User)
                  .WithMany(u => u.UserSkillLinks)
                  .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<UserSkillLink>()
                  .HasOne(us => us.Skill)
                  .WithMany(s => s.UserSkillLinks)
                  .HasForeignKey(us => us.SkillId);

            modelBuilder.Entity<UserInterestLink>()
                  .HasKey(us => new { us.UserId, us.InterestId });

            modelBuilder.Entity<UserInterestLink>()
                  .HasOne(us => us.User)
                  .WithMany(u => u.UserInterestLinks)
                  .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<UserInterestLink>()
                  .HasOne(us => us.Interest)
                  .WithMany(s => s.UserInterestLinks)
                  .HasForeignKey(us => us.InterestId);

            modelBuilder.Entity<ProgramSkill>()
                .HasKey(ps => new { ps.ProgramId, ps.SkillId });


            modelBuilder.Entity<ProgramSkill>()
                .HasOne(ps => ps.Program)
                .WithMany(p => p.ProgramSkills)
                .HasForeignKey(ps => ps.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProgramSkill>()
                .HasOne(ps => ps.Skill)
                .WithMany(s => s.ProgramSkills)
                .HasForeignKey(ps => ps.SkillId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Program>()
                .HasMany(p => p.ProgramGoals)
                .WithOne(pg => pg.Program)
                .HasForeignKey(pg => pg.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Foundation>()
               .HasMany(f => f.Programs)
               .WithOne(p => p.Foundation)
               .HasForeignKey(p => p.FoundationId)
               .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
