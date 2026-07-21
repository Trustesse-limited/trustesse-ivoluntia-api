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
            if (this.Status != ProgramStatus.Pending.ToString() || this.Status != ProgramStatus.Active.ToString() || this.Status != ProgramStatus.Queried.ToString() || this.Status != ProgramStatus.Ended.ToString())
                throw new Exception("invalid status");
            return this;
        }
    }
}
