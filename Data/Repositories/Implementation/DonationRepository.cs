using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Donation;
using Trustesse.Ivoluntia.Commons.uitilities;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class DonationRepository: IDonationRepository
    {
        private readonly iVoluntiaDataContext _iVoluntiaDataContext;
        public DonationRepository(iVoluntiaDataContext iVoluntiaDataContext)
        {
            _iVoluntiaDataContext = iVoluntiaDataContext; 
        }
        public async Task<bool> InitializeDonation(Donation donation)
        {
            try
            {
               if(donation != null) 
                {  
                    await _iVoluntiaDataContext.Donations.AddAsync(donation);
                    await _iVoluntiaDataContext.SaveChangesAsync();
                    return true;
                }
                return false;
            } catch(Exception ex)
            {
                return false;
            }
        }
        public async Task<string> UpdateDonationAsync(UpdateDto donationId)
        {
            var donation = await _iVoluntiaDataContext.Donations.Include(x => x.Program).FirstOrDefaultAsync(x => x.Id == donationId.Id);
            try
            {
                if (donation != null)
                {
                   
                    donation.StatusOfDonation = DonationStatus.Received.ToString();
                    donation.SettlementTimeDate = DateTime.Now; 
                    donation.DateUpdated = DateTime.Now;
                    _iVoluntiaDataContext.Donations.Update(donation);
                    await _iVoluntiaDataContext.SaveChangesAsync();
                    return $"{donation.Program.CreatedBy} {donation.DonorEmail}";//program admin email
                }
                return "no data found";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
