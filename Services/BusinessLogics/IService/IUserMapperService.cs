using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IUserMapperService
    {
        Task<User> UserMapper(SignUpDto signUpDto);
    }
}
