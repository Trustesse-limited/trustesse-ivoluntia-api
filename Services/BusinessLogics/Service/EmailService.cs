using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class EmailService: IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        public EmailService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _baseUrl = config["EmailService:BaseUrl"];
        }
        public async Task<ApiResponse<string>> SendEmailASync(EmailModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/Notifications/send-email", content);
            if(response.IsSuccessStatusCode)
            {
                return ApiResponse<string>.Success("OTP successfully sent to registered email.", null);
            }
            return ApiResponse<string>.Failure(500, "Unable to update user account with OTP details.");
        }
    }
}
