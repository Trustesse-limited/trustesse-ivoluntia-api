using System.ComponentModel.DataAnnotations.Schema;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Location : BaseEntity
    {
        public string CountryId { get; set; }
        public string StateId { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public string Address { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public string FoundationId { get; set; }
        public Foundation Foundation { get; set; }
        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }

        [ForeignKey("StateId")]
        public virtual State State { get; set; }
    }

}
