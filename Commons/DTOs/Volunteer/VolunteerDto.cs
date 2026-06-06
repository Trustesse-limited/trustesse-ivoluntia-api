using System.ComponentModel.DataAnnotations;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Commons.DTOs.Volunteer
{
    public class VolunteerDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LocationId { get; set; }
        public string? UserImage { get; set; }
        public string? Bio { get; set; }
        public byte? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsDeprecated { get; set; }
        public string? FoundationId { get; set; }
        public string? ProgramId { get; set; }
        public ICollection<Skill?> Skills { get; set; }
        public ICollection<Interest?> Interests { get; set; }
    }

    public class VolunteerQueryDto
    {
        [Required]
        public string FoundationId { get; set; }

        public bool? IsActive { get; set; }
    }

}
