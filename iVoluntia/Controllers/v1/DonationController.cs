using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs.Donation;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationController : ControllerBase
    {
        private readonly IDonationService _donationService;

        public DonationController(IDonationService donationService)
        {
           _donationService = donationService;
        }
        [HttpPost("donate")]
        public async Task<IActionResult> Donation(DonationDto donationDto)
        {
           
            var response = await _donationService.InitializeDonation(donationDto);
            try
            {
                if (response.StatusCode == StatusCodes.Status200OK)
                {
                    return Ok(response.Data);
                }
                return BadRequest(response.Message);   
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //update donation by donationId
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateDto donationId)
        {
            var response = await _donationService.UpdateDonationAsync(donationId);    
            try
            {
                if (response.StatusCode == StatusCodes.Status200OK)
                {
                    return Ok(response.Data);
                }
                return BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
