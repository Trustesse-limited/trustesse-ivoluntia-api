using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Commons.DTOs.Foundation
{
    public class UpdateOrganizationStatusDto
    {     
        public string Status { get; set; }  
        public string? Reason { get; set; } 
        
        public UpdateOrganizationStatusDto Validate(string OrganizationId)
        {
            if (this == null)
                throw new Exception("invalid request");
            if (this.Status == OrganizationStatusUpdateEnums.Declined.ToString() || this.Status == OrganizationStatusUpdateEnums.Blocked.ToString() && this.Reason == null)
                throw new Exception("reason is required for the change of status");
            if (this.Status != OrganizationStatusUpdateEnums.Approved.ToString() && this.Status != OrganizationStatusUpdateEnums.Blocked.ToString() && this.Status != OrganizationStatusUpdateEnums.Declined.ToString())
                throw new Exception("invalid status now");
            if (OrganizationId == null)
                throw new Exception("organization id is required");
            return this;    
        }
    }
}
