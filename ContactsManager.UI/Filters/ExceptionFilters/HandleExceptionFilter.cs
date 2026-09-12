using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ExceptionFilters
{
  public class HandleExceptionFilter : IExceptionFilter
  {
    private readonly ILogger<HandleExceptionFilter> _logger;
    private readonly IHostEnvironment _environment;

    public HandleExceptionFilter(ILogger<HandleExceptionFilter> logger, IHostEnvironment environment)
    {
      _logger = logger;
      _environment = environment;
    }

    public void OnException(ExceptionContext context)
    {
      _logger.LogError("Exception Filter {FilterName}.{MethodName}()\n{ExceptionType}\n{ExceptionMessage}\n{InnerExceptionMessage}",
          nameof(HandleExceptionFilter),
          nameof(OnException), context.Exception.GetType().ToString(),
          context.Exception.Message,
          context.Exception.InnerException?.Message
      );
    }
  }
}
