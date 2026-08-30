using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Data.Repositories;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class TwoFactorAuthenticationService: ITwoFactorAuthenticationService
    {
        private readonly IOtpService _otp;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        public TwoFactorAuthenticationService(IOtpService otp, IUnitOfWork unitOfWork, UserManager<User> userManager, INotificationService notificationService, IEmailService emailService)
        {
            _otp = otp;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _notificationService = notificationService;
            _emailService = emailService;
        }
        public async Task<GlobalRequestReponse<string>> TwoFactorAuthenticationByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);   
            var otp = await _otp.GenerateOtpAsync(user.Id, OtpPurpose.TwoFactorAuthenticationLogin.ToString(), false, NotificationChannelEnum.Email.ToString());
            user.OTP = otp;
            user.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // emailService 
                var dictionary = new Dictionary<string, string>()
             {
               {"Name",user.Email},
               {"Otp", user.OTP}
             };
                var notificationTemplate = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.TwoFactorAuthentication.ToString(), NotificationChannelEnum.Email.ToString(), dictionary);
                if (notificationTemplate != null)
                {
                    List<string> receivers = new List<string>{ user.Email };
                    string Message = HttpUtility.HtmlDecode(notificationTemplate.Data);
                    var emailModel = EmailModelBuilder.EmailModelObjectBuilder(receivers, NotificationTypeEnum.TwoFactorAuthentication.ToString(), Message);
                    var emailResponse = await _emailService.SendEmailASync(emailModel);
                    if(emailResponse.StatusCode == StatusCodes.Status200OK)
                        return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, "two factor code sent to email", true);
                }
                return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
            }
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
    }
}
