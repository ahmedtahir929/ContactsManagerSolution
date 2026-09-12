using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResultFilters
{
    public class PersonsListResultFilter : IAsyncResultFilter
    {
        private readonly ILogger<PersonsListResultFilter> _logger;

        public PersonsListResultFilter(ILogger<PersonsListResultFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            //TO DO: before logic here
            _logger.LogInformation("{ResultFilterName}.{MethodName}() - before",
                nameof(PersonsListResultFilter), nameof(OnResultExecutionAsync));

            context.HttpContext.Response.Headers["Last-Modified"] =
                DateTime.UtcNow.ToString("R");

            await next(); //call the subsequent result filters or action result

            //TO DO: after logic here
            _logger.LogInformation("{ResultFilterName}.{MethodName}() - after",
                nameof(PersonsListResultFilter), nameof(OnResultExecutionAsync));

            var statusCode = context.HttpContext.Response.StatusCode;

            _logger.LogInformation("{ResultFilterName}.{MethodName}() - status code: {StatusCode}",
                nameof(PersonsListResultFilter), nameof(OnResultExecutionAsync), statusCode);
        }
    }
}
