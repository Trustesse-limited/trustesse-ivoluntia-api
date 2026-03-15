using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Donation
{
    public class InitializeDonationDto
    {
        public string Amount { get; set; }
        public string Email { get; set; } 
        public string Reference { get; set; }
        public string ServicePaidFor { get; set; }
        public string ServiceId { get; set; }
        public string DonationId { get; set; }
        public string PaymentMethod { get; set; }
        public string Callback_Url { get; set; }  
        public string UserId { get; set; } 
    }
}
