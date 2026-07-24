using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class FavoriteProgram : BaseEntity
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public string ProgramId { get; set; }
        public Program Program { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
