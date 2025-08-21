using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.Models.Request
{
    public class CreateStateModel
    {
        public string StateName { get; set; }
        public Guid CountryId { get; set; }
    }
}
