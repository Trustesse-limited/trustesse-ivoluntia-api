using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Commons.DTOs.Program
{
    public class ProgramDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LocationId { get; set; }
        public long DonationTarget { get; set; }
        public string? BannerImage { get; set; }
        public bool HasDonation { get; set; }
        public bool IsActive { get; set; }
        public int Status { get; set; }
        public string FoundationId { get; set; }
        public List<ProgramGoalDTO> ProgramGoals { get; set; } = new();
        public List<ProgramSkillDTO> ProgramSkills { get; set; } = new();
    }


    public class CreateProgramDto
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

        [Required]
        public string FoundationId { get; set; }

        public long DonationTarget { get; set; }

        public string? BannerImage { get; set; }

        public List<string> SkillIds { get; set; } = new();

        public List<CreateProgramGoalDTO> ProgramGoals { get; set; } = new();
    }

    public class UpdateProgramDTO
    {
        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? LocationId { get; set; }
        public long? DonationTarget { get; set; }
        public string? BannerImage { get; set; }
    }

    public class CreateProgramGoalDTO
    {
        public string Goal { get; set; }
        public bool IsAchieved { get; set; } = false;
    }

    public class UpdateProgramGoalDTO : CreateProgramGoalDTO
    {
        public string Id { get; set; }
    }

    public class ProgramGoalDTO
    {
        public string Id { get; set; }
        public string Goal { get; set; } = string.Empty;
        public bool IsAchieved { get; set; }
    }
    public class ProgramSkillDTO
    {
        public string SkillId { get; set; }
        public SkillDto Skill { get; set; }
    }

    public class SkillDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
