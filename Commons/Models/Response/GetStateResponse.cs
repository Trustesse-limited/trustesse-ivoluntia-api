namespace Trustesse.Ivoluntia.Commons.Models.Response;

public class GetStateResponse
{
    public string StateId { get; set; }
    public string StateName { get; set; }
    public string CountryId { get; set; }
    public string CountryName { get; set; }
}

public class GetCountryResponse
{
    public string Id { get; set; }
    public string CountryName { get; set; }
    public string CountryCode { get; set; }
}