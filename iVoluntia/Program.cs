using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Trustesse.Ivoluntia.API.Middlewares;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});


builder.Services.AddSwaggerGen(options =>
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
                In = ParameterLocation.Header,

            },
            new List<string>()
        }
    });

});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
    });
    options.AddPolicy("Filter", policyBuilder =>
    {
        policyBuilder.WithOrigins(builder.Configuration.GetSection("CORS:AllowedOrigins").Value!.Split(','))
                     .WithMethods(builder.Configuration.GetSection("CORS:AllowedMethods").Value!.Split(','))
                     .WithHeaders(builder.Configuration.GetSection("CORS:AllowedHeaders").Value!.Split(','))
                     .AllowCredentials();

    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<iVoluntiaDataContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")!),
        mySqlOptions => mySqlOptions.MigrationsAssembly("Trustesse.Ivoluntia.Data")
    )
);

// configure User Manager and Role Manager 
builder.Services.AddIdentity<User, Role>()
       .AddEntityFrameworkStores<iVoluntiaDataContext>()
       .AddDefaultTokenProviders();


builder.Services.AddScoped<NetworkFilter>();
var app = builder.Build();

//roles and superadmin user seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<Role>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    await RoleSeeder.SeedRolesAsync(roleManager);
    await RoleSeeder.SeedSuperAdminUserAsync(userManager, roleManager);
}



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
