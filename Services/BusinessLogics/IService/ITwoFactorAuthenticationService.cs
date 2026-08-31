using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface ITwoFactorAuthenticationService
    {
        Task<GlobalRequestReponse<string>> TwoFactorAuthenticationByEmail(string email);
    }
}
