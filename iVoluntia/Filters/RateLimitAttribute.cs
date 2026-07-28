using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.API.Filters
{
    public class RateLimitAttribute : Attribute, IAsyncActionFilter
    {
        public int MaxRequests { get; set; } = 10;
        public int WindowSeconds { get; set; } = 60;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

            var userId = currentUserService.GetUserId();
            var cacheKey = $"ratelimit:{userId}:{context.ActionDescriptor.Id}";

            var counter = cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(WindowSeconds);
                return new RequestCounter();
            });

            var count = Interlocked.Increment(ref counter.Count);

            if (count > MaxRequests)
            {
                context.Result = new ObjectResult(
                    ResponseHelper.BuildResponse<object>("Too many requests. Please try again later.", StatusCodes.Status429TooManyRequests, null, false))
                {
                    StatusCode = StatusCodes.Status429TooManyRequests
                };
                return;
            }

            await next();
        }

        private class RequestCounter
        {
            public int Count;
        }
    }
}
