namespace Trustesse.Ivoluntia.Commons.DTOs.Program
{
    public class UpdateProgramStatusDto
    {
        public string ProgramId { get; set; }
        public string Status { get; set; }
        public string? QueriedComment { get; set; }
    }
}
