using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Donation
{
    public class DonationDto
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }  
        public string Message { get; set; } 
        public string PaymentMethod { get; set; }     
        public string ProgramId { get; set; }   
    }
}
