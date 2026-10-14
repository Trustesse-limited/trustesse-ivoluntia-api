namespace Trustesse.Ivoluntia.Commons.DTOs.Location
{
    public class GetLocationResponseDto
    {
        public string Id { get; set; }
        public string? Address { get; set; }
        public string City { get; set; }
        public string? Zipcode { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
    }
}
