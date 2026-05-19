using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.DataContext
{
    public class iVoluntiaDataContext : IdentityDbContext<User, Role, string>
    {
        private readonly ICurrentUserRepository _currentUserRepository;
        public iVoluntiaDataContext(DbContextOptions<iVoluntiaDataContext> options, ICurrentUserRepository currentUserRepository) : base(options)
        {
            _currentUserRepository = currentUserRepository;
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
        public DbSet<ProgramRejectionReason> ProgramRejectionReasons { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<UserProgram> userPrograms { get; set; }
        public DbSet<FavoriteProgram> FavoritePrograms { get; set; }
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

                entity.HasQueryFilter(u => !u.IsDeprecated);
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

                entity.HasQueryFilter(f => !f.IsDeprecated);
            });

            modelBuilder.Entity<Interest>(entity =>
            {
                entity.Property(u => u.Name)
                      .HasMaxLength(50);
                entity.Property(u => u.Description)
                      .HasMaxLength(500);
                entity.HasQueryFilter(u => !u.IsDeprecated);
            });
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.Property(u => u.Name)
                      .HasMaxLength(50);
                entity.Property(u => u.Description)
                      .HasMaxLength(500);
                entity.HasQueryFilter(u => !u.IsDeprecated);
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(u => u.Address)
                      .HasMaxLength(500);

                entity.HasOne(l => l.Country)
                      .WithMany()
                      .HasForeignKey(l => l.CountryId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(l => l.State)
                      .WithMany()
                      .HasForeignKey(l => l.StateId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasQueryFilter(u => !u.IsDeprecated);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId);
                entity.HasQueryFilter(u => !u.IsDeprecated);
            });

            modelBuilder.Entity<NotificationChannelSettings>(entity =>
            {
                entity.HasOne(s => s.NotificationChannel)
                      .WithMany(c => c.ChannelSettings)
                      .HasForeignKey(s => s.NotificationChannelId);
                entity.HasQueryFilter(u => !u.IsDeprecated);
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
            modelBuilder.Entity<UserSkillLink>()
                  .HasQueryFilter(us => !us.IsDeprecated);

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
            modelBuilder.Entity<UserInterestLink>()
                  .HasQueryFilter(us => !us.IsDeprecated);

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
            modelBuilder.Entity<ProgramSkill>()
                .HasQueryFilter(ps => !ps.IsDeprecated);

            modelBuilder.Entity<Program>()
                .HasMany(p => p.ProgramGoals)
                .WithOne(pg => pg.Program)
                .HasForeignKey(pg => pg.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Program>()
                .HasMany(p => p.ProgramRejectionReasons)
                .WithOne(pg => pg.Program)
                .HasForeignKey(pg => pg.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Program>()
                .HasOne(p => p.Location)
                .WithMany()
                .HasForeignKey(p => p.LocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Program>()
               .HasMany(p => p.Users)
               .WithMany(u => u.Programs)
               .UsingEntity<UserProgram>();
            modelBuilder.Entity<Program>()
               .HasQueryFilter(p => !p.IsDeprecated & p.FoundationId == _currentUserRepository.GetUserFoundationId());


            modelBuilder.Entity<Foundation>()
               .HasMany(f => f.Programs)
               .WithOne(p => p.Foundation)
               .HasForeignKey(p => p.FoundationId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired(false);

            modelBuilder.Entity<Donation>()
              .HasOne(d => d.Program)
              .WithMany(p => p.Donations)
              .HasForeignKey(d => d.ProgramId)
              .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Donation>()
             .HasOne(d => d.User)
             .WithMany(u => u.Donations)
             .HasForeignKey(d => d.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Donation>()
              .HasQueryFilter(d => !d.IsDeprecated);


            modelBuilder.Entity<FavoriteProgram>(entity =>
            {
                entity.HasOne(fp => fp.User)
                    .WithMany(u => u.FavoritePrograms)
                    .HasForeignKey(fp => fp.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(fp => fp.Program)
                    .WithMany()
                    .HasForeignKey(fp => fp.ProgramId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(fp => new
                {
                    fp.UserId,
                    fp.ProgramId
                })
                .IsUnique();
            });
        }
    }
}
