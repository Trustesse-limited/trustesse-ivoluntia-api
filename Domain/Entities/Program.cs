using System.ComponentModel.DataAnnotations;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Program : BaseEntity
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public string LocationId { get; set; }
        public Location Location { get; set; }
        public long DonationTarget { get; set; }
        public string? BannerImage { get; set; }
        public bool HasDonation { get; set; }
        public bool IsActive { get; set; }
        public int Status { get; set; }
        public string FoundationId { get; set; }
        public Foundation Foundation { get; set; }
        public ICollection<ProgramSkill> ProgramSkills { get; set; } = new List<ProgramSkill>();
        public ICollection<ProgramGoal> ProgramGoals { get; set; } = new List<ProgramGoal>();
        public ICollection<User> Users{ get; set; } = new List<User>();
        public ICollection<ProgramRejectionReason> ProgramRejectionReasons { get; set; } = new List<ProgramRejectionReason>();
        public ICollection<Donation> Donations { get; set; }    
        public ICollection<UserProgram> UsersPrograms { get; set; }      

        public bool HasProgramEnded()
        {
            return EndDate < DateTime.UtcNow;
        }
    }
}
