using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
using Trustesse.Ivoluntia.Commons.Configurations;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Commons.Validators;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.IRepositories;
using Trustesse.Ivoluntia.Data.Repositories;
using Trustesse.Ivoluntia.Data.Repositories.Implementation;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.Abstractions;
using Trustesse.Ivoluntia.Services.BusinessLogics.Implementations;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;
using Trustesse.Ivoluntia.Services.BusinessLogics.Service;
using Trustesse.Ivoluntia.Services.Implementation;

namespace Trustesse.Ivoluntia.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            services.AddScoped<IDonationService, DonationService>();
            services.AddScoped<IDonationRepository, DonationRepository>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IProgramService, ProgramService>();
            services.AddScoped<IProgramRepository, ProgramRepository>();
            services.AddScoped<IFoundationRepository, FoundationRepository>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddHttpClient<IEmailService, EmailService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ICurrentUserRepository, CurrentUserRepository>();
            services.AddScoped<IFileUploadService, CloudinaryService>();
            services.AddScoped<IFileUploadServiceFactory, FileUploadServiceFactory>();
            services.AddScoped<IVolunteerService, VolunteerService>();
            services.AddScoped<IVolunteerRepository, VolunteerRepository>();
            services.AddScoped<IFavoriteProgramRepository, FavoriteProgramRepository>();
            services.AddScoped<IFavoriteProgramService, FavoriteProgramService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<ISecurityQuestionRepository, SecurityQuestionRepository>();
            services.AddScoped<ITransactionPinRepository, TransactionPinRepository>();
            services.AddScoped<ITransactionPinService, TransactionPinService>();
            services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
            services.AddScoped<IPinVerificationAttemptRepository, PinVerificationAttemptRepository>();
            services.AddScoped<IAuthorizationTokenService, AuthorizationTokenService>();
            services.AddScoped<ISecurityQuestionService, SecurityQuestionService>();
            services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IStateRepository, StateRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IOnboardingProgressRepository, OnboardingProgressRepository>();
            services.AddScoped<IInterestRepository, InterestRepository>();
            services.AddScoped<ISkillRepository, SkillRepository>();
            services.AddScoped<IUserInterestLinkRepository, UserInterestLinkRepository>();
            services.AddScoped<IUserSkillLinkRepository, UserSkillLinkRepository>();
            services.AddScoped<IOtpRepo, OtpRepo>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IOnboardingService, OnboardingService>();
            services.AddScoped<IQualificationService, QualificationService>();
            services.AddScoped<IQualificationRepository, QualificationRepository>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<ICauseService, CauseService>();
            services.AddScoped<ICauseRepository, CauseRepository>();
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<IInterestService, InterestService>();
            services.AddScoped<IUserQualificationService, UserQualificationService>();
            services.AddScoped<IUserQualificationRepository, UserQualificationRepository>();
            services.AddScoped<ITwoFactorAuthenticationService, TwoFactorAuthenticationService>();
            services.AddScoped<IUserMapperService, UserMapperService>();
            services.AddScoped<IOtpEmailSenderService, OtpEmailSenderService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITwoFactorAuthenticationService, TwoFactorAuthenticationService>();
            services.AddScoped<IUserMapperService, UserMapperService>();
            services.AddScoped<IOtpEmailSenderService, OtpEmailSenderService>();

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
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
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
            services.AddDbContext<iVoluntiaDataContext>((spt, options) =>
            {
                options.UseSqlServer(
                   config.GetConnectionString("DefaultConnection")!,
                   sqlServerOptions => sqlServerOptions.MigrationsAssembly("Trustesse.Ivoluntia.Data"));
                options.AddInterceptors(spt.GetRequiredService<AuditSaveChangesInterceptor>());
            });
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();

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
            var jwtOptions = configuration.GetSection("JwtOptions");
            services.Configure<JwtOptions>(jwtOptions);
            services.Configure<TransactionSecurityOptions>(configuration.GetSection("TransactionSecurityOptions"));

            var issuer = jwtOptions["Issuer"];
            var audience = jwtOptions["Audience"];
            var key = jwtOptions["Key"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var protector = context.HttpContext.RequestServices
                       .GetRequiredService<IDataProtectionProvider>()
                       .CreateProtector("JWTProtector");
                        var header = context.Request.Headers.Authorization.ToString(); 
                        if (!string.IsNullOrWhiteSpace(header) && header.StartsWith("Bearer ",StringComparison.OrdinalIgnoreCase))
                        {
                            var encryptedToken = header["Bearer".Length..].Trim();
                            var token = protector.Unprotect(encryptedToken);
                            context.Token = token;
                        }
                        else if(!string.IsNullOrWhiteSpace(header))
                        {
                            var token = protector.Unprotect(header);
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine(context.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<INotificationService, NotificationService>();
           
            //services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<AuditSaveChangesInterceptor>();

            return services;
        }

        public static IServiceCollection AddRateLimt(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.PermitLimit = 4;
                    opt.Window = TimeSpan.FromSeconds(12);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 2;
                });
            });
            return services;
        }
    }
}
