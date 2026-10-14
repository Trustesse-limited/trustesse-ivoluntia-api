using System.Linq;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Commons.DTOs.Program
{
    public class UpdateProgramStatusDto
    {
        public string ProgramId { get; set; }
        public string Status { get; set; }
        public string? QueriedComment { get; set; }

        public UpdateProgramStatusDto Validate()
        {
            if (this == null)
                throw new Exception("Invalid Request");
            if (string.IsNullOrWhiteSpace(ProgramId))
                throw new Exception("ProgramId should not be null");
            if (string.IsNullOrWhiteSpace(Status))
                throw new Exception("Status should not be null");

            var validStatuses = new[]
            {
                ProgramStatus.Pending.ToString(),
                ProgramStatus.Active.ToString(),
                ProgramStatus.Queried.ToString(),
                ProgramStatus.Ended.ToString(),
                ProgramStatus.Forfeited.ToString()
            };

            if (!validStatuses.Contains(this.Status))
                throw new Exception("invalid status");
            return this;
        }
    }
}
