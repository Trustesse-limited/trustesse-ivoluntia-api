using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.DTOs.Volunteer;
using Trustesse.Ivoluntia.Domain.Entities;

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
            config.NewConfig<Foundation, OrganizationDto>();


            services.AddSingleton(config);
            services.AddScoped<IMapper, Mapper>();
        }
    }

}
