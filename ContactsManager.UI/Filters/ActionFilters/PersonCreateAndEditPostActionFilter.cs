using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts.CountriesServiceContracts;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilters
{
  public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter
  {
    private readonly ICountriesGetterService _countriesGetterService;
    private readonly ILogger<PersonCreateAndEditPostActionFilter> _logger;

    public PersonCreateAndEditPostActionFilter(ICountriesGetterService countriesService,
        ILogger<PersonCreateAndEditPostActionFilter> logger)
    {
      _countriesGetterService = countriesService;
      _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      //TO DO: before logic here
      _logger.LogInformation("{ActionFilterName}.{MethodName}() - before logic", nameof(PersonCreateAndEditPostActionFilter), nameof(OnActionExecutionAsync));

      if (context.Controller is PersonsController personsController &&
          !personsController.ModelState.IsValid)
      {
        List<CountryResponse> countries = 
          await _countriesGetterService.GetAllCountries();

        // Convert countries to SelectListItem
        personsController.ViewBag.Countries = countries.Select(temp =>
            new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() }
        );

        personsController.ViewBag.Errors =
            personsController.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

        //short-circuit the subsequent action filters & action method only if the model state is invalid.
        context.Result = personsController.View(context.ActionArguments.Values.First()); //assigned action result
      }
      else
      {
        await next(); //call the next action filter or action method
      }

      //TO DO: after logic here
      _logger.LogInformation("{ActionFilterName}.{MethodName}() - after logic", nameof(PersonCreateAndEditPostActionFilter), nameof(OnActionExecutionAsync));
    }
  }
}
