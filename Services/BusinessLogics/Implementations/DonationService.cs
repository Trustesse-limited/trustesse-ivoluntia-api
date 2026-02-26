using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Donation;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Data.Migrations;
using Trustesse.Ivoluntia.Data.Repositories;
using Trustesse.Ivoluntia.Data.Repositories.Implementation;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;
using Trustesse.Ivoluntia.Services.BusinessLogics.Service;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Implementations
{
    public class DonationService: IDonationService
    {
        private readonly IDonationRepository _donationRepository;
        public readonly ICurrentUserService _currentUserService; 
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public DonationService(IDonationRepository donationRepository, IConfiguration configuration, HttpClient client, ICurrentUserService currentUserService, INotificationService notificationService, IEmailService emailService)
        {
            _donationRepository = donationRepository;
            _configuration = configuration;
            //_client = client;
            //_baseUrl = _configuration["PaymentGateway:BaseUrl"];
            //_client = new HttpClient
            //{
            //    // BaseAddress = new Uri(_baseUrl)
            //};
            _client = client;
            _baseUrl = configuration["PaymentGateway:BaseUrl"];
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _emailService = emailService;
        }
        public async Task<ApiResponse<PaymentInitializeResponse>> InitializeDonation(DonationDto donationDto)
        {
            try
            {
                    if (donationDto != null)
                    {
                        Donation donation = new Donation
                        {
                            Id = Guid.NewGuid().ToString(),
                            Amount = donationDto.Amount,
                            ServicePaidFor = PaymentType.Program.ToString(),
                            StatusOfDonation = DonationStatus.Initiated.ToString(),
                            PaymentMethod = donationDto.PaymentMethod,
                            InitializeTimeDate = DateTime.Now,
                            ProgramId = donationDto.ProgramId,
                            DonorMessage = donationDto.Message,
                            UserId = _currentUserService.GetUserId(),
                            DonorEmail = _currentUserService.GetUserEmail(),
                            ReferenceNumber = Guid.NewGuid().ToString()
                        };

                    var dbResponse = await _donationRepository.InitializeDonation(donation);
                    if(dbResponse == false)
                    {
                        InitializeDonationDto initializeDonationDto = new InitializeDonationDto
                        {
                            Amount = donation.Amount,
                            Email = donation.DonorEmail,
                            ServicePaidFor = donation.ServicePaidFor,
                            ServiceId = donation.Id,
                            PaymentMethod = donation.PaymentMethod,
                            Reference = donation.ReferenceNumber,
                            Callback_Url = "https://www.google.com",
                            UserId = donation.UserId
                        };
                        var json = JsonConvert.SerializeObject(initializeDonationDto);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        using var response = await _client.PostAsync("https://localhost:/api/5112/PaystackTransaction/DonationPayment", content);
                        response.EnsureSuccessStatusCode();
                        var initializeResponse = await response.Content.ReadFromJsonAsync<PaymentInitializeResponse>();
                        if (initializeResponse != null)
                        {
                            return ApiResponse<PaymentInitializeResponse>.Success("success", initializeResponse);
                        }
                    }  
                    }
                //using var responsee = await _client.PostAsJsonAsync("PaystackTransaction/DonationPayment", initializeDonationDto);
                return ApiResponse<PaymentInitializeResponse>.Failure(StatusCodes.Status404NotFound, "data not found");
            }
            catch (Exception ex) 
            {
                return ApiResponse<PaymentInitializeResponse>.Failure(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        //update donation with donation id
        public async Task<ApiResponse<string>> UpdateDonationAsync(string donationId)
        {
            var programAdminEmail = await _donationRepository.UpdateDonationAsync(donationId);
            try
            {
                if (programAdminEmail != null)
                {
                    //send email to donor
                    string userFirstName = _currentUserService.GetUserFirstName(); 
                    string userEmail = _currentUserService.GetUserEmail();  
                    Dictionary<string, string> donorHolder = new Dictionary<string, string>();
                    donorHolder.Add("FirstName", userFirstName);
                    var donorMessage = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.Donation.ToString(), NotificationChannelEnum.Email.ToString(), donorHolder);
                    EmailModel donorEmailModel = new EmailModel
                    {
                        Receivers = userEmail.Trim().Split().ToList(),   
                        Subject = "program donation",
                        Message = HttpUtility.HtmlDecode(donorMessage.Data)
                    };
                    var donorEmailResponse = await _emailService.SendEmailASync(donorEmailModel);
                    //send email to program creator
                    string[] programAdminInfo = programAdminEmail.Split(' ');
                    Dictionary<string, string> adminHolder = new Dictionary<string, string>();
                    adminHolder.Add("FirstName", programAdminInfo[1]);
                    var adminMessage = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.DonationMade.ToString(), NotificationChannelEnum.Email.ToString(), adminHolder);
                    EmailModel adminEmailModel = new EmailModel
                    {
                        Receivers = programAdminInfo[0].Trim().Split().ToList(),
                        Subject = "donation made",
                        Message = HttpUtility.HtmlDecode(adminMessage.Data)
                    };
                    var adminEmailResponse = await _emailService.SendEmailASync(adminEmailModel);
                    return ApiResponse<string>.Success("email sent to donor and program admin", "email sent");
                }
                return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "unable to sent email");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Failure(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
