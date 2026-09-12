using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResourceFilters
{
    public class FeatureDisabledResourceFilter : IAsyncResourceFilter
    {
        private readonly ILogger<FeatureDisabledResourceFilter> _logger;
        private readonly bool _isFeatureDisabled; //Set this to true to enable the feature

        public FeatureDisabledResourceFilter(ILogger<FeatureDisabledResourceFilter> logger, bool isFeatureDisabled = true)
        {
            _logger = logger;
            _isFeatureDisabled = isFeatureDisabled;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            //TODO: before logic here
            _logger.LogInformation("{FilterName}.{MethodName}() - before", nameof(FeatureDisabledResourceFilter), nameof(OnResourceExecutionAsync));

            if (_isFeatureDisabled) 
            {
                context.Result = new StatusCodeResult(StatusCodes.Status501NotImplemented);
            }
            else
            {
                await next(); //Proceed to the subsequent filter or action
            }

            //TODO: after logic here
            _logger.LogInformation("{ResourceFilterName}.{MethodName}() - after", nameof(FeatureDisabledResourceFilter), nameof(OnResourceExecutionAsync));
        }
    }
}
