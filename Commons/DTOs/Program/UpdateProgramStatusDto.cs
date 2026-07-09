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

            return this;
        }
    }
}
