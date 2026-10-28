using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Commons.DTOs.OrganisationAvailability
{
    public class GetAvailableRecurringSlotsQueryDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "page must be greater than 0")]
        public int Page { get; set; } = 1;
        [Range(1, 20, ErrorMessage = "perPage must be between 1 and 20")]
        public int PerPage { get; set; } = 10;
    }

    public class RecurringSlotTimeDto
    {
        public string SlotId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class RecurringSlotDayGroupDto
    {
        public string DayCode { get; set; }
        public List<RecurringSlotTimeDto> Times { get; set; }
    }

    public class RecurringSlotPaginationDto
    {
        public int Page { get; set; }
        public int PerPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class AvailableRecurringSlotsListDto
    {
        public List<RecurringSlotDayGroupDto> Items { get; set; }
        public RecurringSlotPaginationDto Pagination { get; set; }
    }
}
