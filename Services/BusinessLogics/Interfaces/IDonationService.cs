using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Donation;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface IDonationService
    {
        Task<ApiResponse<PaymentInitializeResponse>> InitializeDonation(DonationDto donationDto);
        Task<ApiResponse<string>> UpdateDonationAsync(UpdateDto donationId);
    }
}
