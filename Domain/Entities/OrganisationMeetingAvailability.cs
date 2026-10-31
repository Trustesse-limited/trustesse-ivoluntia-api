namespace Trustesse.Ivoluntia.Domain.Entities;

public class OrganisationMeetingAvailability : BaseEntity
{
    public string FoundationId { get; set; }
    public Foundation Foundation { get; set; }
    public string DayCode { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
