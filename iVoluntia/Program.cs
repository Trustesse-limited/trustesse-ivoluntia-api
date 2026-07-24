using CloudinaryDotNet;
using MapsterMapper;
using Microsoft.Extensions.Options;
using Trustesse.Ivoluntia.API.Extensions;
using Trustesse.Ivoluntia.API.Middlewares;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Data.Repositories;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services;
using Trustesse.Ivoluntia.Services.Helpers;

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
builder.Services.AddScoped<IMapper, Mapper>();

// Add Mapster mappings
builder.Services.RegisterMappings();


//add cloudinary settings
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));


builder.Services.AddSingleton(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
    return new Cloudinary(account);
});


#region Services

#endregion

#region Repository

#endregion

var app = builder.Build();

// Seed roles and superadmin
await app.SeedDefaultDataAsync();


    app.UseSwagger();
    app.UseSwaggerUI();


app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

if (Convert.ToBoolean(builder.Configuration.GetSection("CORS:Enabled").Value)) app.UseCors("Filter");
else app.UseCors("AllowAll");

app.UseHsts();
app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
