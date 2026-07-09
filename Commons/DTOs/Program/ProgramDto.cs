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
        [Required]
        public string CreatorEmail { get; set; }

        public long DonationTarget { get; set; }

        public string? BannerImage { get; set; }

        public List<string> SkillIds { get; set; } = new();

        public List<CreateProgramGoalDTO> ProgramGoals { get; set; } = new();

        public CreateProgramDto Validate()
        {
            if (this == null)
                throw new Exception("Invalid Request");
            if (string.IsNullOrWhiteSpace(Title))
                throw new Exception("Title should not be null");
            if (string.IsNullOrWhiteSpace(Description))
                throw new Exception("Description should not be null");
            if (StartDate == default)
                throw new Exception("StartDate should not be null");
            if (EndDate == default)
                throw new Exception("EndDate should not be null");
            if (EndDate < StartDate)
                throw new Exception("EndDate cannot be earlier than StartDate");
            if (string.IsNullOrWhiteSpace(LocationId))
                throw new Exception("LocationId should not be null");
            if (string.IsNullOrWhiteSpace(FoundationId))
                throw new Exception("FoundationId should not be null");
            if (string.IsNullOrWhiteSpace(CreatorEmail))
                throw new Exception("CreatorEmail should not be null");

            return this;
        }
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

        public UpdateProgramDTO Validate()
        {
            if (this == null)
                throw new Exception("Invalid Request");
            if (string.IsNullOrWhiteSpace(Id))
                throw new Exception("Id should not be null");

            return this;
        }
    }

    public class CreateProgramGoalDTO
    {
        public string Goal { get; set; }

    }

    public class UpdateProgramGoalDTO : CreateProgramGoalDTO
    {
        public string Id { get; set; }
        public bool IsAchieved { get; set; }
    }

    public class ProgramGoalDTO : UpdateProgramGoalDTO
    {

    }
    public class ProgramSkillDTO
    {
        public SkillDto Skill { get; set; }
    }

    public class SkillDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class AddFavoriteProgramRequest
    {
        [Required]
        public string ProgramId { get; set; }

        public AddFavoriteProgramRequest Validate()
        {
            if (ProgramId == null)
                throw new Exception("ProgramId should not be null");
            if (this == null)
                throw new Exception("Invalid Request");

            return this;
        }
    }

    public class FavoriteProgramDto : AddFavoriteProgramRequest
    {
        public string UserId { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
