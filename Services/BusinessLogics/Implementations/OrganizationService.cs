using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.DTOs.GenericResponse;
using Trustesse.Ivoluntia.Commons.DTOs.GlobalRequest;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Implementations
{
    public class OrganizationService: IOrganizationService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notify;
        private readonly IEmailService _email;
        public OrganizationService(IMapper mapper, IUnitOfWork unitOfWork, IAuthenticationService authenticationService, ICurrentUserService currentUserService, INotificationService notify, IEmailService email)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _currentUserService = currentUserService;
            _notify = notify;
            _email = email;
        }

        public async Task<GlobalRequestReponse<List<OrganizationResponseDto>>> GetOrganization(PagedRequestDTO pagedRequestDTO)
        {
            if (pagedRequestDTO.All == false && pagedRequestDTO.Status != null)
            {
                var organization = await _unitOfWork.OrganizationRepository.GetAsync(f => f.Status == pagedRequestDTO.Status, null, pagedRequestDTO.Page, pagedRequestDTO.PageSize);
                var organizationResponseDto = _mapper.Map<List<OrganizationResponseDto>>(organization);
                if (organizationResponseDto.Count == 0)
                    return ResponseHelper.BuildResponse("organizations not found", StatusCodes.Status404NotFound, organizationResponseDto, false);
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, organizationResponseDto, true); 
            }
            else if (pagedRequestDTO.OrderByColumn != null && pagedRequestDTO.OrderBy != null && pagedRequestDTO.SearchQuery != null)
            {
                var organizations = await _unitOfWork.OrganizationRepository.SearchAndOrder(null, pagedRequestDTO.Page, pagedRequestDTO.PageSize, pagedRequestDTO.SearchQuery, pagedRequestDTO.OrderByColumn, pagedRequestDTO.OrderBy).ToListAsync();
                var organizationResponseDto = _mapper.Map<List<OrganizationResponseDto>>(organizations);
                if (organizationResponseDto.Count == 0)
                    return ResponseHelper.BuildResponse("organizations not found", StatusCodes.Status404NotFound, organizationResponseDto, false);
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, organizationResponseDto, true);
            }
            else if (pagedRequestDTO.All == false && pagedRequestDTO.Status == null)
            {
                var organization = await _unitOfWork.OrganizationRepository.GetAsync(null, null, pagedRequestDTO.Page, pagedRequestDTO.PageSize);
                var organizationResponseDto = _mapper.Map<List<OrganizationResponseDto>>(organization);
                if (organizationResponseDto.Count == 0)
                    return ResponseHelper.BuildResponse("organizations not found", StatusCodes.Status404NotFound, organizationResponseDto, false);
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, organizationResponseDto, true);
            }
            else
            {
                //no pagination 
                var organization = await _unitOfWork.OrganizationRepository.GetAllAsync();
                var organizationResponseDto = _mapper.Map<List<OrganizationResponseDto>>(organization);
                if(organizationResponseDto.Count == 0)
                    return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, organizationResponseDto, true);
                return ResponseHelper.BuildResponse("organizations not found", StatusCodes.Status404NotFound, organizationResponseDto, false);
            }                      
        }
        public async Task<GlobalRequestReponse<OrganizationResponseDto>> GetOrganizationByID(string id)
        {
             var response = await _unitOfWork.OrganizationRepository.GetByExpressionAsync(f => f.Id == id);
             var organizationResponseDto = _mapper.Map<OrganizationResponseDto>(response);
             if (response != null)
                 return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, organizationResponseDto, true);
             return ResponseHelper.BuildResponse("organizations not found", StatusCodes.Status404NotFound, organizationResponseDto, false);  
        }

        public async Task<GlobalRequestReponse<string>> OrganizationStatusUpdate(UpdateOrganizationStatusDto updateOrganizationStatusDto, string organizationId)
        {
            var foundation = await _unitOfWork.OrganizationRepository.GetByExpressionAsync(f => f.Id == organizationId);
            if (foundation == null)
                return ResponseHelper.BuildResponse("not found", StatusCodes.Status404NotFound, "organization not found", false);
            if(_currentUserService.GetUserFoundationId == null)
                return ResponseHelper.BuildResponse("user not in organization", StatusCodes.Status400BadRequest, "user must belong to an organization", false);
            if(updateOrganizationStatusDto.Status == OrganizationStatusUpdateEnums.Approved.ToString())
            {
                foundation.Status = updateOrganizationStatusDto.Status;
                _unitOfWork.OrganizationRepository.Update(foundation);
                await _unitOfWork.CompleteAsync();
                var dictionary = new Dictionary<string, string>()
                {
                {"Name",foundation.Name},
                {"Status",updateOrganizationStatusDto.Status}
                };
                var notificationTemplate = await _notify.ComposeNotificationAsync(NotificationTypeEnum.OrganizationStatusUpdate.ToString(), NotificationChannelEnum.Email.ToString(), dictionary);
                if (notificationTemplate != null)
                {
                    var message = new EmailModel
                    {
                        Receivers = new List<string> {foundation.Email},
                        Subject = "Status Update",
                        Message = HttpUtility.HtmlDecode(notificationTemplate.Data)
                    };
                    var emailResponse = await _email.SendEmailASync(message);
                }
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, updateOrganizationStatusDto.Status, true);
            }
            else if (updateOrganizationStatusDto.Status == OrganizationStatusUpdateEnums.Declined.ToString() || updateOrganizationStatusDto.Status == OrganizationStatusUpdateEnums.Blocked.ToString())
            {
                foundation.Status = updateOrganizationStatusDto.Status;
                _unitOfWork.OrganizationRepository.Update(foundation);

                await _unitOfWork.CompleteAsync();
                var dictionary = new Dictionary<string, string>()
                {
                {"Name",foundation.Name},
                {"Status",updateOrganizationStatusDto.Status},
                {"Reason", updateOrganizationStatusDto.Reason}
                };
                var notificationTemplate = await _notify.ComposeNotificationAsync(NotificationTypeEnum.OrganizationBlockStatusUpdate.ToString(), NotificationChannelEnum.Email.ToString(), dictionary);
                if (notificationTemplate != null)
                {
                    var message = new EmailModel
                    {
                        Receivers = new List<string> { foundation.Email },
                        Subject = "Status Update",
                        Message = HttpUtility.HtmlDecode(notificationTemplate.Data)
                    };
                    var emailResponse = await _email.SendEmailASync(message);
                }
                var organizationDeclineStatus = new OrganizationDeclineStatus
                {
                    Id = Guid.NewGuid().ToString(),
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    IsDeprecated = false,
                    ModifiedBy = _currentUserService.GetUserEmail(),
                    ModifiedDate = DateTime.Now,
                    Reason = updateOrganizationStatusDto.Reason,
                    Status = updateOrganizationStatusDto.Status,
                    FoundationId = organizationId
                }; 
                await _unitOfWork.organizationDeclineStatusRepository.AddAsync(organizationDeclineStatus);
                await _unitOfWork.CompleteAsync();  
                return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, updateOrganizationStatusDto.Status, true);
            }
            return ResponseHelper.BuildResponse("something went wrong", StatusCodes.Status400BadRequest, updateOrganizationStatusDto.Status, false);
        }
    }
}
