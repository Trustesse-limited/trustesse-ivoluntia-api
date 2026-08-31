using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class UserMapperService: IUserMapperService
    {
        private readonly IMapper _mapper;
        private readonly IOtpService _otp;
        public UserMapperService(IMapper mapper, IOtpService otp)
        {
            _mapper = mapper;
            _otp = otp;
        }
        public async Task<User> UserMapper(SignUpDto signUpDto)
        {
            var user = _mapper.Map<User>(signUpDto);
            user.UserName = signUpDto.Email;
            user.Email = signUpDto.Email.Trim();
            user.DateCreated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            user.IsActive = false;
            user.HasAgreedToTermsAndCondition = signUpDto.HasAgreedToTermsAndCondition;
            var otp = await _otp.GenerateOtpAsync(user.Id, OtpPurpose.Signup.ToString(), false, NotificationChannelEnum.Email.ToString());
            user.OTP = otp;
            user.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
            return user;    
        }
    }
}
