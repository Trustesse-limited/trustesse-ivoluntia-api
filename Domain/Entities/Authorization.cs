using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Authorization : BaseEntity
    {
        public byte[] TokenHash { get; set; }
        public byte[] TokenSalt { get; set; }
        public bool IsUsed { get; set; }

        [Required]
        public string InitiatorId { get; set; }
        public byte[] ByteString { get; set; }

        [Required]
        [MaxLength(255)]
        public string Purpose { get; set; }

        public virtual User Initiator { get; set; } = null!;
    }
}
