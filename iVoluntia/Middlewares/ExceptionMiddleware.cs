using Newtonsoft.Json;
using System.Net;
using Trustesse.Ivoluntia.Commons.Extensions.Logging;

namespace Trustesse.Ivoluntia.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IConfiguration configuration;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            this.configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // Call the next delegate/middleware in the pipeline
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, $"Something went wrong: {ex.Message}");

                // Handle the exception and create a custom response
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Create your custom error response object
            var errorResponse = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An unexpected error occurred.",
                Details = exception.Message // Optionally include more details
            };

            // Serialize the response to JSON and write it out
            var jsonResponse = JsonConvert.SerializeObject(errorResponse);
            LogHandler.WriteLog(configuration.GetConnectionString("LogPath")!, "Error ===> " + jsonResponse + exception.InnerException + exception.StackTrace, LogType.Exception);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
