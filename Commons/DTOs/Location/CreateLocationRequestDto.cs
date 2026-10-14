using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Commons.DTOs.Location
{
    public class CreateLocationRequestDto
    {
        public string? Address { get; set; }

        [Required]
        public string City { get; set; }

        public string? Zipcode { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string State { get; set; }

        public CreateLocationRequestDto Validate()
        {
            if (this == null)
                throw new Exception("Invalid Request");
            if (string.IsNullOrWhiteSpace(City))
                throw new Exception("City should not be null");
            if (string.IsNullOrWhiteSpace(Country))
                throw new Exception("Country should not be null");
            if (string.IsNullOrWhiteSpace(State))
                throw new Exception("State should not be null");

            return this;
        }
    }
}
