using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ActionFilters
{
  public class ResponseHeaderFilterFactoryAttribute : Attribute, IFilterFactory
  {
    public bool IsReusable => false;
    private readonly string? _responseHeaderKey;
    private readonly string? _responseHeaderValue;
    private readonly int _order = 0;

    public ResponseHeaderFilterFactoryAttribute(string? responseHeaderKey, string? responseHeaderValue, int order)
    {
      _responseHeaderKey = responseHeaderKey;
      _responseHeaderValue = responseHeaderValue;
      _order = order;
    }

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
      var logger = serviceProvider.GetService<ILogger<ResponseHeaderActionFilter>>();

      //return filter object
      ResponseHeaderActionFilter responseHeaderActionFilter = new ResponseHeaderActionFilter(logger, _responseHeaderKey, _responseHeaderValue, _order);
      return responseHeaderActionFilter;
    }
  }
  public class ResponseHeaderActionFilter : IAsyncActionFilter, IOrderedFilter
  {
    private readonly ILogger<ResponseHeaderActionFilter>? _logger;
    private readonly string? _responseHeaderKey;
    private readonly string? _responseHeaderValue;
    public int Order { get; }

    public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter>? logger, string? responseHeaderKey, string? responseHeaderValue, int order)
    {
      _logger = logger;
      _responseHeaderKey = responseHeaderKey;
      _responseHeaderValue = responseHeaderValue;
      Order = order;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      //TO DO: before logic here
      _logger?.LogInformation("{FilterName}.{ActionMethodName}() invoked - before logic", nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));

      await next(); //calls the subsequent filter or action method in the pipeline

      //TO DO: after logic here
      _logger?.LogInformation("{FilterName}.{ActionMethodName}() invoked - after logic", nameof(ResponseHeaderActionFilter), nameof(OnActionExecutionAsync));

      context.HttpContext.Response.Headers[_responseHeaderKey!] = _responseHeaderValue;
    }
  }
}
