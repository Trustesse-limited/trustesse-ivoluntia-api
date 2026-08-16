using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.PortableExecutable;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.DTOs.Volunteer;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Commons.DTOs.Country;
using Trustesse.Ivoluntia.Commons.DTOs.Skill;
using Trustesse.Ivoluntia.Commons.DTOs.Interest;
using Trustesse.Ivoluntia.Commons.DTOs.Cause;

namespace Trustesse.Ivoluntia.Services
{
    public static class MappingConfig
    {
        public static void RegisterMappings(this IServiceCollection services)
        {
            var config = TypeAdapterConfig.GlobalSettings;

            config.NewConfig<Program, ProgramDto>();
            config.NewConfig<Program, CreateProgramDto>();
            config.NewConfig<Program, UpdateProgramDTO>();
            config.NewConfig<ProgramSkill, ProgramSkillDTO>();
            config.NewConfig<Skill, SkillDto>();
            config.NewConfig<User, VolunteerDto>();
            config.NewConfig<FavoriteProgram, FavoriteProgramDto>();
            config.NewConfig<Foundation, OrganizationResponseDto>();
            config.NewConfig<User, CreateFoundationRequestDto>();
            config.NewConfig<CreateFoundationRequestDto, Foundation>();
            config.NewConfig<CreateFoundationRequestDto,FoundationAdminSignUpDto>();
            config.NewConfig<Cause,FoundationCauses>();
            config.NewConfig<FoundationAdminInfo, User>();
            config.NewConfig<FoundationBioData, Foundation>();
            config.NewConfig<User, OtpDto>();
            config.NewConfig<OtpDto, Otp>();
            config.NewConfig<VolunteerSignUpDto, User>();
            config.NewConfig<SecurityQuestion, SecurityQuestionDto>();
            config.NewConfig<OrganizationAccountNumberVerifyResponseDto, FoundationBankAccountDetail>();
            config.NewConfig<State, GetStateResponse>();
            config.NewConfig<CreateCountryRequestDto, Country>();
            config.NewConfig<Skill, GetSkillResponseDto>();
            config.NewConfig<CreateSkillRequestDto, Skill>();
            config.NewConfig<CreateInterestRequestDto, Interest>();
            config.NewConfig<Interest, GetInterestResponseDto>();
            config.NewConfig<CreateCauseRequestDto, Cause>();
            config.NewConfig<Cause, GetCauseResponseDto>();

            services.AddSingleton(config);
            services.AddScoped<IMapper, Mapper>();
        }
    }
}
