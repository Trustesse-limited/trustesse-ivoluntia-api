using System.Net;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Data.Repositories;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service;

public class VolunteerService : IVolunteerService   
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    public VolunteerService(IUnitOfWork uow, IMapper mapper, UserManager<User> userManager)
    {
            _uow = uow; 
            _mapper = mapper;   
            _userManager = userManager; 
    }

    public async Task<CustomResponse> CreateVolunteer(AuthInfo model)
    {
        CustomResponse response = null;
        User volunteer = null;
        try
        {
            volunteer = _mapper.Map<User>(model);
            volunteer.UserName = model.Email;
            volunteer.Email = !string.IsNullOrWhiteSpace(model.Email.Trim()) ? model.Email.Trim() : default;
            volunteer.DateCreated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            volunteer.IsActive = false;
            if (model.HasAcceptedTOC == true)
            {
                volunteer.HasAgreedToTermsAndCondition = model.HasAcceptedTOC;
            }
            else
            {
                response = new CustomResponse(400, "Volunteer must accept agree to terms and conditions.");
            }
             //Check if the Volunteer already exist
                var VolunteerExists = await _uow.userRepo.GetByExpressionAsync(x => 
                      x.Email == model.Email);
                if (VolunteerExists is null)
                {
                    var result = await _userManager.CreateAsync(volunteer, model.Password.Trim());
                        if (result.Succeeded)
                        {
                        var otp = GenerateOTP();
                        volunteer.OTP = otp;
                        volunteer.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
                        volunteer.Id = volunteer.Id;
                        result = await _userManager.UpdateAsync(volunteer).ConfigureAwait(false);

                        if (!result.Succeeded)
                        {
                            response = new CustomResponse(500, "Unable to update user account with OTP details.");
                        }
                        else
                        {
                            var updateProgressTable = await _uow.onboardingProgressRepo.GetByIdAsync(volunteer.Id);
                            if (updateProgressTable != null)
                            {
                                var obj = new OnboardingProgress();
                                {
                                    obj.LastCompletedPage = 1;
                                    obj.HasCompletedOnboarding = false;
                                }
                                await _uow.onboardingProgressRepo.UpdateAsync(obj);
                                if (await _uow.CompleteAsync() > 0)
                                {
                                    response = new CustomResponse(200, "OnboardingProgress has been updated successfully.");
                                }
                            }
                        }
                  

                        response = new CustomResponse((int)HttpStatusCode.OK, "Volunteer account creation successful and OTP sent to your mail", null);
                    }
                    else
                    {
                        List<IdentityError> errorList = result.Errors.ToList();
                        object errors = string.Join(", ", errorList.FirstOrDefault().Description);
                        response = new CustomResponse((int)HttpStatusCode.InternalServerError, (string)errors, null);
                    }
                    
                }
                else
                {
                    response = new CustomResponse((int)HttpStatusCode.Conflict, $"Volunteer with Email -> {model.Email}  already exist.", null);
                }
        }
        catch (Exception ex)
        {
          
        }

        return response;
    }
    
    private string GenerateOTP()
    {
        try
        {
            byte[] seed = Guid.NewGuid().ToByteArray();
            Random _random = new Random(BitConverter.ToInt32(seed, 0));
            int _rand = _random.Next(1000, 10000);

            return _rand.ToString();
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}