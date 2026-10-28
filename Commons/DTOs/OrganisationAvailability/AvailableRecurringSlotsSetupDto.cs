namespace Trustesse.Ivoluntia.Commons.DTOs.OrganisationAvailability
{
    public class AvailableRecurringSlotsSetupRequestDto
    {
        public string OrganisationId { get; set; }
        public List<DaySlotRequestDto> Slots { get; set; }
    }

    public class DaySlotRequestDto
    {
        public string DayCode { get; set; }
        public List<TimeSlotRequestDto> Times { get; set; }
    }

    public class TimeSlotRequestDto
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
