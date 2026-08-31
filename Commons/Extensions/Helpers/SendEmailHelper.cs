//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web;
//using Trustesse.Ivoluntia.Commons.Models.Request;
//using Trustesse.Ivoluntia.Commons.Models.Response;
//using Trustesse.Ivoluntia.Domain.Enums;

//namespace Trustesse.Ivoluntia.Commons.Extensions.Helpers
//{
//    public class SendEmailHelper
//    {
//        private readonly INotificationService _notify;
//        private readonly IEmailService _email;
//        public SendEmailHelper()
//        {
            
//        }
//        public static async Task<GlobalRequestReponse<string>> EmailSender(string email, string otp)
//        {
//            var dictionary = new Dictionary<string, string>()
//             {
//               {"Name",volunteer.Email},
//               {"Otp", volunteer.OTP}
//             };
//            var notificationTemplate = await _notify.ComposeNotificationAsync(NotificationTypeEnum.OtpRequest.ToString(), NotificationChannelEnum.Email.ToString(), dictionary);
//            if (notificationTemplate != null)
//            {
//                var message = new EmailModel
//                {
//                    Receivers = new List<string> { volunteer.Email },
//                    Subject = "OTP",
//                    Message = HttpUtility.HtmlDecode(notificationTemplate.Data)
//                };
//                var emailResponse = await _email.SendEmailASync(message);
//            }
//            return ResponseHelper.BuildResponse("account created and otp sent", StatusCodes.Status200OK, "created successfully", true);
//        }
//    }
//}
