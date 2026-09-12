using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Filters.ActionFilters
{
  public class PersonsListActionFilter : IActionFilter
  {
    private readonly ILogger<PersonsListActionFilter> _logger;

    public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
    {
      _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
      //To Do: Add logic to be executed before the action method is called
      _logger.LogInformation("{FilterName}.{MethodName} method",
          nameof(PersonsListActionFilter), nameof(OnActionExecuting));

      //Now accessible anywhere in the action method or in the action result method
      context.HttpContext.Items["arguments"] = 
        new Dictionary<string, object?>(context.ActionArguments);

      if (context.ActionArguments.TryGetValue("searchBy", out object? value))
      {
        string? searchBy = value as string;

        if (!string.IsNullOrEmpty(searchBy))
        {
          // Validate the searchBy parameter
          var validSearchByFields = new List<string>
          {
            nameof(PersonResponse.PersonName),
            nameof(PersonResponse.Email),
            nameof(PersonResponse.DateOfBirth),
            nameof(PersonResponse.Gender),
            nameof(PersonResponse.Country),
            nameof(PersonResponse.Address)
          };

          //reset the searchBy parameter value
          if (!validSearchByFields.Contains(searchBy))
          {
            _logger.LogWarning("Invalid searchBy parameter: {searchBy}. Setting it to PersonName.", searchBy);

            //setting searchBy to default: "PersonName"
            context.ActionArguments["searchBy"] = nameof(PersonResponse.PersonName);
          }
        }
      }

    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
      //To Do: Add logic to be executed after the action method is called

      _logger.LogInformation("{FilterName}.{MethodName} method",
         nameof(PersonsListActionFilter), nameof(OnActionExecuting));

      PersonsController? personsController = context.Controller as PersonsController;

      IDictionary<string, object?>? parameters =
          context.HttpContext.Items["arguments"] as IDictionary<string, object?>;

      if (parameters != null)
      {
        //Retrieve the searchBy and searchString values from the parameters dictionary object
        parameters.TryGetValue("searchBy", out object? seachByValue);
        parameters.TryGetValue("searchString", out object? searchStringValue);
        parameters.TryGetValue("sortBy", out object? sortByValue);
        parameters.TryGetValue("sortOrder", out object? sortOrderValue);

        personsController?.ViewBag.CurrentSearchBy = seachByValue as string;
        personsController?.ViewBag.CurrentSearchString = searchStringValue as string;
        personsController?.ViewBag.CurrentSortBy = sortByValue as string;
        personsController?.ViewBag.CurrentSortOrder = sortOrderValue?.ToString();

        //Setting default CurrentSortBy and CurrentSortOrder
        if (personsController?.ViewBag.CurrentSortBy == null)
          personsController?.ViewBag.CurrentSortBy = nameof(PersonResponse.PersonName);
        if (personsController?.ViewBag.CurrentSortOrder == null)
          personsController?.ViewBag.CurrentSortOrder = nameof(SortOrderOptions.ASC);
      }

      //Search
      personsController?.ViewBag.SearchFields = new Dictionary<string, string>()
      {
        { nameof(PersonResponse.PersonName), "Person Name" },
        { nameof(PersonResponse.Email), "Email" },
        { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
        { nameof(PersonResponse.Gender), "Gender" },
        { nameof(PersonResponse.Country), "Country" },
        { nameof(PersonResponse.Address), "Address" },
      };
    }
  }
}
