using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IOtpEmailSenderService
    {
        Task<GlobalRequestReponse<string>> OtpSender(string email, string otp, string notificatioType);
    }
}
