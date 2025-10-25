using System.ComponentModel.DataAnnotations;

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
    }
}
