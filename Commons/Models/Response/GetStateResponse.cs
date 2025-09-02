namespace Trustesse.Ivoluntia.Commons.Models.Response;

public class GetStateResponse
{
    public Guid StateId { get; set; }
    public string StateName { get; set; }
    public Guid CountryId { get; set; }
    public string CountryName { get; set; }
}

public class GetCountryResponse
{
    public Guid CountryId { get; set; }
    public string CountryName { get; set; }
}