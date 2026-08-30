using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class OtpEmailSenderService: IOtpEmailSenderService
    {
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        public OtpEmailSenderService(INotificationService notificationService, IEmailService emailService)
        {
            _notificationService = notificationService;
            _emailService = emailService;
        }
        public  async Task<GlobalRequestReponse<string>> OtpSender(string email, string otp, string notificatioType)
        {
            var dictionary = new Dictionary<string, string>()
             {
               {"Name",email},
               {"Otp", otp}
             };
            var notificationTemplate = await _notificationService.ComposeNotificationAsync(notificatioType, NotificationChannelEnum.Email.ToString(), dictionary);
            if (notificationTemplate != null)
            {
                var message = new EmailModel
                {
                    Receivers = new List<string> { email },
                    Subject = "OTP",
                    Message = HttpUtility.HtmlDecode(notificationTemplate.Data)
                };
                var emailResponse = await _emailService.SendEmailASync(message);
            }
            return ResponseHelper.BuildResponse("otp sent", StatusCodes.Status200OK, "successfully", true);
        }
    }
}
