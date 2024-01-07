using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Helpers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [ExcludeFromCodeCoverage]
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private readonly int requestCount;
        private readonly int timeInMinutes;

        public RateLimitAttribute(int requestCount, int timeInMinutes)
        {
            this.requestCount = requestCount;
            this.timeInMinutes = timeInMinutes;
        }
        private static readonly Dictionary<string, List<DateTime>> RequestTimes = new Dictionary<string, List<DateTime>>();

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string controllerActionKey = $"{context.Controller.GetType().Name}/{context.ActionDescriptor.DisplayName}";

            if (!RequestTimes.ContainsKey(controllerActionKey))
            {
                RequestTimes[controllerActionKey] = new List<DateTime>();
            }

            var requestTimes = RequestTimes[controllerActionKey];
            requestTimes.Add(DateTime.Now);

            if (requestTimes.Count > requestCount)
            {
                var timeElapsed = DateTime.Now - requestTimes[0];
                if (timeElapsed < TimeSpan.FromSeconds(timeInMinutes))
                {
                    context.Result = new ContentResult
                    {
                        Content = "Rate limit exceeded.",
                        StatusCode = StatusCodes.Status429TooManyRequests
                    };
                    return;
                }

                // Remove the oldest request time to keep only the last two
                requestTimes.RemoveAt(0);
            }

            base.OnActionExecuting(context);
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [ExcludeFromCodeCoverage]
    public class RateLimitForRoleAttribute : ActionFilterAttribute
    {
        private readonly int requestCount;
        private readonly int timeInMinutes;

        public RateLimitForRoleAttribute(int requestCount, int timeInMinutes)
        {
            this.requestCount = requestCount;
            this.timeInMinutes = timeInMinutes;
        }
        private static readonly Dictionary<string, List<DateTime>> RequestTimes = new Dictionary<string, List<DateTime>>();
        public const string RateLimitExceededKey = "RateLimitExceeded";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string controllerActionKey = $"{context.Controller.GetType().Name}/{context.ActionDescriptor.DisplayName}";

            if (!RequestTimes.ContainsKey(controllerActionKey))
            {
                RequestTimes[controllerActionKey] = new List<DateTime>();
            }

            var requestTimes = RequestTimes[controllerActionKey];
            requestTimes.Add(DateTime.Now);

            // Check if at least two requests have been made within one minute
            if (requestTimes.Count > requestCount)
            {
                var timeElapsed = DateTime.Now - requestTimes[0];
                if (timeElapsed < TimeSpan.FromSeconds(timeInMinutes))
                {
                    context.HttpContext.Items[RateLimitExceededKey] = true;
                    return;
                }

                // Remove the oldest request time to keep only the last two
                requestTimes.RemoveAt(0);
            }

            base.OnActionExecuting(context);
        }
    }
}
