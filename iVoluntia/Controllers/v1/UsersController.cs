using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthService _authService;
        public UsersController(UserManager<IdentityUser> userManager, IAuthService  authService)
        {
            _userManager = userManager;
            _authService = authService;
        }
        // This code defines a UsersController for managing sample user accounts in an ASP.NET Core application.
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber
            });
        }
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] IdentityUser user)
        {
            if (user == null)
            {
                return BadRequest("User data cannot be null.");
            }
            var result = await _userManager.CreateAsync(user, "DefaultPassword123!");
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, user);

        }
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] IdentityUser updatedUser)
        {
            if (string.IsNullOrEmpty(userId) || updatedUser == null)
            {
                return BadRequest("User ID and updated user data cannot be null or empty.");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            user.UserName = updatedUser.UserName;
            user.Email = updatedUser.Email;
            user.PhoneNumber = updatedUser.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok(user);
        }
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return NoContent();
        }
        
        [HttpPost("CreateVolunteer")]
        public async Task<IActionResult> CreateVolunteer([FromBody] VolunteerSignUpDto request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _authService.CreateVolunteer(request);

            if (result.ResponseCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }


    }
}
