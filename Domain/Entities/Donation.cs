using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Donation: BaseEntity
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string ReferenceNumber { get; set; } 
        public string ProgramId { get; set; } 
        public Program Program { get; set; }   
        public string? DonorMessage { get; set; } 
        public string ServicePaidFor { get; set; } 
        public string PaymentMethod { get; set; } 
        public string UserId { get; set; }  
        public User User { get; set; }      
        public string DonorEmail { get; set; }
        public string? StatusOfDonation { get; set; }    
        public DateTime? InitializeTimeDate { get; set; }  
        public DateTime? SettlementTimeDate { get; set; }    
    }
}
