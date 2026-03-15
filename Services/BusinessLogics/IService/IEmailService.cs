using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IEmailService
    {
        Task<ApiResponse<string>> SendEmailASync(EmailModel model);
    }
}
