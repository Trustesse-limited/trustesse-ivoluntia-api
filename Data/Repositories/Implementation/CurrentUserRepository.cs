using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class CurrentUserRepository: ICurrentUserRepository
    {
        private readonly IHttpContextAccessor _http;
        private ClaimsPrincipal? _user;
        private readonly iVoluntiaDataContext _context;
        public CurrentUserRepository(IHttpContextAccessor http, iVoluntiaDataContext context)
        {
            _http = http;
            _user = _http.HttpContext?.User;
            _context = context;
        }

        public string? Name => _user?.Identity?.Name;

        private string _userId = string.Empty;
        private string _userEmail = string.Empty;
        private string _firstName = string.Empty;



        public string GetUserId() =>
           IsAuthenticated()
           ? _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
             ?? _user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
             ?? Guid.Empty.ToString()
           : _userId;

        public string GetUserEmail() =>
           IsAuthenticated()
           ? _user?.FindFirst(ClaimTypes.Email)?.Value
             ?? _user?.FindFirst(JwtRegisteredClaimNames.Email)?.Value
             ?? string.Empty
           : _userEmail;

        public string GetUserFirstName() =>
          IsAuthenticated()
          ? _user?.FindFirst(ClaimTypes.GivenName)?.Value
            ?? _user?.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value
            ?? string.Empty
          : _firstName;

        public bool IsAuthenticated() =>
           _user?.Identity?.IsAuthenticated is true;


        public async Task<ApiResponse<string>> GetUserFoundationId(string userId)
        {

            var foundationId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.FoundationId)
                .FirstOrDefaultAsync();

            if (foundationId == null)
                return ApiResponse<string>.Failure(StatusCodes.Status404NotFound, "No foundation Id found for user");

            return ApiResponse<string>.Success("user foundation id retrieved successfully", foundationId);
        }
    }
}
