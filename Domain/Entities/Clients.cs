using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Clients
    {
        [Key]
        public int Id { get; set; }
        public string? ServiceName { get; set; }
        public string? IPaddress { get; set; }
    }
}
