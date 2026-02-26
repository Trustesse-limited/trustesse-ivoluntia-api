using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Donation
{
    public class DonationDto
    {
        public string Amount { get; set; }  
        public string Message { get; set; } 
        public string PaymentMethod { get; set; }     
        public string ProgramId { get; set; }      
    }
}
