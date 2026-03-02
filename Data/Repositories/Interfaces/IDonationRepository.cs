using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Donation;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Interfaces
{
    public interface IDonationRepository
    {
        Task<bool> InitializeDonation(Donation donation);
        Task<string> UpdateDonationAsync(UpdateDto donationId);
    }
}
