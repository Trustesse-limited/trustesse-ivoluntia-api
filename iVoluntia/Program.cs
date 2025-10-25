using MapsterMapper;
using Trustesse.Ivoluntia.API.Extensions;
using Trustesse.Ivoluntia.API.Middlewares;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Data.Repositories;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddCustomSwagger();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCustomCors(builder.Configuration);
builder.Services.AddCustomDatabase(builder.Configuration);
builder.Services.AddCustomIdentity(builder.Configuration);
builder.Services.RegisterJwtServices(builder.Configuration);
builder.Services.AddScoped<NetworkFilter>();
builder.Services.AddCustomServices();
builder.ConfigureHsts();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMapper, Mapper>();

// Add Mapster mappings
builder.Services.RegisterMappings();

#region Services
//builder.Services.AddScoped<ICountryService, CountryService>();
//builder.Services.AddScoped<IAuthService, AuthService>();
#endregion

#region Repository
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStateRepository, StateRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IOnboardingProgressRepository, OnboardingProgressRepository>();
builder.Services.AddScoped<IInterestRepository, InterestRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<IUserInterestLinkRepository, UserInterestLinkRepository>();
builder.Services.AddScoped<IUserSkillLinkRepository, UserSkillLinkRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
#endregion

var app = builder.Build();

// Seed roles and superadmin
await app.SeedDefaultDataAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

if (Convert.ToBoolean(builder.Configuration.GetSection("CORS:Enabled").Value)) app.UseCors("Filter");
else app.UseCors("AllowAll");

app.UseHsts();
app.UseRouting();
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
