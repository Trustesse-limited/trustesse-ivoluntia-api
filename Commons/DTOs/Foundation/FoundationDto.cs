using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Commons.DTOs.Foundation
{
    public class FoundationDto
    {

    }


    public class CreateFoundationDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string CategoryId { get; set; }

        [Required]
        public string Mission { get; set; }

        [Required]
        public string Logo { get; set; }

        public string Website { get; set; }

        [Required]
        public string LocationId { get; set; }

        public string Email { get; set; }
        public DateTime YearEstablished { get; set; }
        public bool IsActive { get; set; }
        public bool HasAgreedToDisclaimer { get; set; }
    }

    public class UpdateFoundationDTO
    {

    }

}
