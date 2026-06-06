using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Implementations
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly ICurrentUserRepository _currentUserRepository;
        public CurrentUserService(ICurrentUserRepository currentUserRepository)
        {
            _currentUserRepository = currentUserRepository;
        }
        public string GetUserId() =>
           _currentUserRepository.GetUserId();

        public string GetUserEmail() =>
         _currentUserRepository.GetUserEmail(); 

        public string GetUserFirstName() =>
         _currentUserRepository.GetUserFirstName();

        public string GetUserFoundationId()
        {
            var response = _currentUserRepository.GetUserFoundationId();
            return response;
        }
    }
}
