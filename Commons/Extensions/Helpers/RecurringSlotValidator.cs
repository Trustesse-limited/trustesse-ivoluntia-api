using System.Globalization;
using Trustesse.Ivoluntia.Commons.DTOs.OrganisationAvailability;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Commons.Extensions.Helpers
{
    public static class RecurringSlotValidator
    {
        public record SlotUnit(string DayCode, TimeSpan StartTime, TimeSpan EndTime);

        private static readonly TimeSpan UnitLength = TimeSpan.FromMinutes(30);

        public static (List<SlotUnit> Units, List<ErrorItem> Errors) ValidateAndSplit(List<DaySlotRequestDto> daySlots)
        {
            var errors = new List<ErrorItem>();
            var units = new List<SlotUnit>();

            foreach (var day in daySlots ?? new List<DaySlotRequestDto>())
            {
                var rawDayCode = day.DayCode ?? string.Empty;
                var isRecognisedDayCode = Enum.GetNames<DayCodeEnum>().Any(name => string.Equals(name, rawDayCode, StringComparison.OrdinalIgnoreCase));
                if (!isRecognisedDayCode)
                {
                    errors.Add(new ErrorItem { Key = rawDayCode, ErrorMessages = new List<string> { $"'{rawDayCode}' is not a recognised day code." } });
                    continue;
                }
                var dayCode = Enum.Parse<DayCodeEnum>(rawDayCode, true);

                foreach (var time in day.Times ?? new List<TimeSlotRequestDto>())
                {
                    var slotKey = $"{dayCode}:{time.StartTime}-{time.EndTime}";

                    if (!TimeSpan.TryParseExact(time.StartTime, "hh\\:mm", CultureInfo.InvariantCulture, out var start) ||
                        !TimeSpan.TryParseExact(time.EndTime, "hh\\:mm", CultureInfo.InvariantCulture, out var end))
                    {
                        errors.Add(new ErrorItem { Key = slotKey, ErrorMessages = new List<string> { "startTime and endTime must be in HH:mm format." } });
                        continue;
                    }

                    if (start >= end)
                    {
                        errors.Add(new ErrorItem { Key = slotKey, ErrorMessages = new List<string> { "startTime must be before endTime." } });
                        continue;
                    }

                    if ((end - start).Ticks % UnitLength.Ticks != 0)
                    {
                        errors.Add(new ErrorItem { Key = slotKey, ErrorMessages = new List<string> { "The slot must span a whole multiple of 30 minutes." } });
                        continue;
                    }

                    for (var unitStart = start; unitStart < end; unitStart += UnitLength)
                    {
                        units.Add(new SlotUnit(dayCode.ToString(), unitStart, unitStart + UnitLength));
                    }
                }
            }

            if (errors.Count > 0)
                return (new List<SlotUnit>(), errors);

            var duplicateErrors = units
                .GroupBy(u => (u.DayCode, u.StartTime, u.EndTime))
                .Where(g => g.Count() > 1)
                .Select(g => new ErrorItem
                {
                    Key = $"{g.Key.DayCode}:{g.Key.StartTime:hh\\:mm}-{g.Key.EndTime:hh\\:mm}",
                    ErrorMessages = new List<string> { "This slot is repeated, or overlaps another slot, for this day." }
                })
                .ToList();

            if (duplicateErrors.Count > 0)
                return (new List<SlotUnit>(), duplicateErrors);

            return (units, errors);
        }
    }
}
