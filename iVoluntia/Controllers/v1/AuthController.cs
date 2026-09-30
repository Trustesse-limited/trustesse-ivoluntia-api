using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Collections.Generic;
//using System.Web.Http;
using Trustesse.Ivoluntia.API.Extensions;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/v1/[Controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestModel request)
            => BuildHttpResponse<LoginResponseModel>(await _authenticationService.LoginAsync(request.Validate()));

        [HttpPost("volunteer-signup")]
        public async Task<IActionResult> CreateVolunteer([FromBody] SignUpDto signUpDto)
            =>BuildHttpResponse<string>(await _authenticationService.CreateVolunteer(signUpDto.Validate()));    
        
        [HttpPost("organization-signup")]
        public async Task<IActionResult> CreateOrganization([FromBody] SignUpDto signUpDto)
            => BuildHttpResponse<string>(await _authenticationService.CreateOrganization(signUpDto.Validate()));

        [Authorize]
        [HttpPost("resetchangepassword")]
        public async Task<IActionResult> ResetChangePassword()
            => BuildHttpResponse<string>(await _authenticationService.ResetPasswordAsync());
        
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
            => BuildHttpResponse<string>(await _authenticationService.ChangePasswordAsync(changePasswordDto.Validate()));

        [HttpPost("resetforgotpassword")]
        public async Task<IActionResult> ResetForgotPassword(string email)
            => BuildHttpResponse<string>(await _authenticationService.ResetForgotPassword(email));

        //[HttpPost("resetpassword")]
        //public async Task<IActionResult> ResetPassword([FromQuery] string email)
        //    => BuildHttpResponse<string>(await _authenticationService.ResetPasswordAsync(email));
        
        //[HttpPost("changepassword")]
        //public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        //    => BuildHttpResponse<string>(await _authenticationService.ChangePasswordAsync(changePasswordDto.Validate())); 
       
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
            => BuildHttpResponse<string>(await _authenticationService.ForgetPasswordAsync(forgotPasswordDto.Validate()));     
        
        [Authorize]
        [HttpPost("2fa-setup")]
        public async Task<IActionResult> TwoFactorAuthenticationSetUp()
            => BuildHttpResponse<string>(await _authenticationService.TwoFactorAuthenticationSetUp());

        [EnableRateLimiting("fixed")]
        [HttpPost("2fa-verify")]
        public async Task<IActionResult> VerifyTwoFactorAuthentication(VerifyTwoFactorAuthenticationRequestDto verifyTwoFactorAuthenticationRequestDto)
            => BuildHttpResponse<LoginResponseModel>(await _authenticationService.VerifyTwoFactorAuthentication(verifyTwoFactorAuthenticationRequestDto.Validate()));
        
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestModel refreshTokenRequestModel)
            => BuildHttpResponse<RefreshTokenResponseModel>(await _authenticationService.RefreshTokenAsync(refreshTokenRequestModel.Validate()));

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
            => BuildHttpResponse<string>(await _authenticationService.Logout());
    }
}
 