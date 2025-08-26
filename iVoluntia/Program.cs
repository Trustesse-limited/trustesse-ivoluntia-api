using Trustesse.Ivoluntia.API.Extensions;
using Trustesse.Ivoluntia.API.Middlewares;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;

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
