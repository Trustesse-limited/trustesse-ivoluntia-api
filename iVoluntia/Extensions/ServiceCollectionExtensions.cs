using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Trustesse.Ivoluntia.Commons.Configurations;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;
using Trustesse.Ivoluntia.Services.BusinessLogics.Service;

namespace Trustesse.Ivoluntia.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            services.AddScoped<INotificationService, NotificationService>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "iVoluntia API",
                    Version = "v1"
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. 
                                Enter 'Bearer' [space] and then your token in the text input below.
                                Example: 'Bearer ey12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            return services;
        }
        public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration config)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policyBuilder =>
                {
                    policyBuilder.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader();
                });
                options.AddPolicy("Filter", policyBuilder =>
                {
                    policyBuilder.WithOrigins(config.GetSection("CORS:AllowedOrigins").Value!.Split(','))
                                .WithMethods(config.GetSection("CORS:AllowedMethods").Value!.Split(','))
                                .WithHeaders(config.GetSection("CORS:AllowedHeaders").Value!.Split(','))
                                .AllowCredentials();
                });
            });

            return services;
        }
        public static IServiceCollection AddCustomDatabase(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<iVoluntiaDataContext>(options =>
                options.UseMySql(
                    config.GetConnectionString("DefaultConnection")!,
                    ServerVersion.AutoDetect(config.GetConnectionString("DefaultConnection")!),
                    mySqlOptions => mySqlOptions.MigrationsAssembly("Trustesse.Ivoluntia.Data")
                )
            );

            return services;
        }

        public static IServiceCollection AddCustomIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            var identityConfig = new IdentityConfiguration();
            configuration.GetSection("IdentityOptions").Bind(identityConfig);
            services.AddIdentity<User, Role>()
                    .AddEntityFrameworkStores<iVoluntiaDataContext>()
                    .AddDefaultTokenProviders();

            services.Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(options =>
             {
                 // Lockout settings
                 options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityConfig.Lockout.DefaultLockoutTimeSpanMinutes);
                 options.Lockout.MaxFailedAccessAttempts = identityConfig.Lockout.MaxFailedAccessAttempts;
                 options.Lockout.AllowedForNewUsers = identityConfig.Lockout.AllowedForNewUsers;

                 // Password settings
                 options.Password.RequireDigit = identityConfig.Password.RequireDigit;
                 options.Password.RequiredLength = identityConfig.Password.RequiredLength;
                 options.Password.RequireNonAlphanumeric = identityConfig.Password.RequireNonAlphanumeric;
                 options.Password.RequireUppercase = identityConfig.Password.RequireUppercase;
                 options.Password.RequireLowercase = identityConfig.Password.RequireLowercase;

                 // Sign-in settings
                 options.SignIn.RequireConfirmedEmail = identityConfig.SignIn.RequireConfirmedEmail;
                 options.SignIn.RequireConfirmedPhoneNumber = identityConfig.SignIn.RequireConfirmedPhoneNumber;

                 // User settings
                 options.User.RequireUniqueEmail = identityConfig.User.RequireUniqueEmail;
             });

            return services;
        }

        public static IServiceCollection RegisterJwtServices(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtOptions = configuration.GetSection(nameof(JwtOptions));
            services.Configure<JwtOptions>(jwtOptions);
            var jwtIssuer = jwtOptions[nameof(JwtOptions.Issuer)];
            var jwtAudience = jwtOptions[nameof(JwtOptions.Audience)];
            var jwtSecretKey = jwtOptions[nameof(JwtOptions.Key)];

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
                    };
                });

            return services;
        }
    }
}
