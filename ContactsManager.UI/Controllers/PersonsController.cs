using CRUDExample.Filters;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.AuthorizationFilters;
using CRUDExample.Filters.ExceptionFilters;
using CRUDExample.Filters.ResultFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts.CountriesServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using ServiceContracts.PersonsServiceContracts;

namespace CRUDExample.Controllers
{
  [Route("[controller]")]
  //Add custom response header to all action methods of this controller
  [ResponseHeaderFilterFactory("My-Key-From-Controller", "My-Value-From-Controller", 3)]
  [TypeFilter(typeof(HandleExceptionFilter))]
  [TypeFilter(typeof(PersonAlwaysRunResultFilter))]
  public class PersonsController : Controller
  {
    private readonly ILogger<PersonsController> _logger;
    private readonly ICountriesGetterService _countriesGetterService;
    private readonly IPersonsGetterService _personsGetterService;
    private readonly IPersonsAdderService _personsAdderService;
    private readonly IPersonsUpdaterService _personsUpdaterService;
    private readonly IPersonsDeleterService _personsDeleterService;
    private readonly IPersonsSorterService _personsSorterService;

    public PersonsController(
      ILogger<PersonsController> logger,
      ICountriesGetterService countriesGetterService,
      IPersonsGetterService personsGetterService,
      IPersonsAdderService personsAdderService,
      IPersonsUpdaterService personsUpdaterService,
      IPersonsDeleterService personsDeleterService,
      IPersonsSorterService personsSorterService)
    {
      _logger = logger;
      _countriesGetterService = countriesGetterService;
      _personsGetterService = personsGetterService;
      _personsAdderService = personsAdderService;
      _personsUpdaterService = personsUpdaterService;
      _personsDeleterService = personsDeleterService;
      _personsSorterService = personsSorterService;
    }

    //URL:/Persons/Index
    [Route("[action]")]
    [Route("/")]
    [TypeFilter(typeof(PersonsListActionFilter), Order = 4)]
    //Add custom response header to this action method
    [ResponseHeaderFilterFactory("My-Key-From-ActionMethod", "My-Value-From-ActionMethod", 1)]
    [TypeFilter(typeof(PersonsListResultFilter))]
    [SkipFilter]
    public async Task<IActionResult> Index(string searchBy, string? searchString,
        string sortBy = nameof(PersonResponse.PersonName),
        SortOrderOptions sortOrder = SortOrderOptions.ASC)
    {
      _logger.LogInformation("{ActionMethodName} action method of {ControllerName} invoked",
          nameof(Index), nameof(PersonsController));

      _logger.LogDebug("searchBy: {searchBy}, searchString: {searchString}, sortBy: {sortBy}, sortOrder: {sortOrder}", searchBy, searchString, sortBy, sortOrder);

      List<PersonResponse> persons =
          await _personsGetterService.GetFilteredPersons(searchBy, searchString);

      //Sort
      List<PersonResponse> sortedPersons =
          await _personsSorterService.GetSortedPersons(persons, sortBy, sortOrder);

      return View(sortedPersons);
    }

    //Executes when the user clicks on the "Create Person" hyperlink (while opening the create view)
    [Route("[action]")]
    public async Task<IActionResult> Create()
    {
      List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
      ViewBag.Countries = countries.Select(
         temp =>
         new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() }
      );
      //<option value="1">Name</option>

      return View();
    }

    //A view that has a form "submit" btn has two views: one HttpGet second HttpPost
    //HttpGet: To load the view
    //HttpPost: To submit the form

    [Route("[action]")]
    [HttpPost]
    [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
    public async Task<IActionResult> Create(PersonAddRequest personAddRequest)
    {
      // Call the service method on valid model state
      PersonResponse personResponse = await _personsAdderService.AddPerson(personAddRequest);

      // Navigate to Index() action method
      return RedirectToAction("Index", "Persons");
    }

    [HttpGet]
    [Route("[action]/{personID:guid}")] //Eg: /Persons/Edit/3fa85f64-5717-4562-b3fc-2c963f66afa6
    [TypeFilter(typeof(TokenResultFilter))]
    public async Task<IActionResult> Edit(Guid personID)
    {
      PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);

      if (personResponse == null)
      {
        return RedirectToAction("Index");
      }

      PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

      List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
      ViewBag.Countries = countries.Select(temp =>
      new SelectListItem { Text = temp.CountryName, Value = temp.CountryID.ToString() });

      return View(personUpdateRequest);
    }

    [HttpPost]
    [Route("[action]/{personID:guid}")]
    [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
    //[TypeFilter(typeof(TokenAuthorizationFilter))]
    public async Task<IActionResult> Edit(PersonUpdateRequest personUpdateRequest)
    {
      PersonResponse? personResponse =
          await _personsGetterService.GetPersonByPersonID(personUpdateRequest.PersonID);

      if (personResponse == null)
      {
        return RedirectToAction("Index");
      }

      PersonResponse updatedPerson = await _personsUpdaterService.UpdatePerson(personUpdateRequest);

      return RedirectToAction("Index");
    }

    [HttpGet]
    [Route("[action]/{personID:guid}")]
    public async Task<IActionResult> Delete(Guid personID)
    {
      PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);

      if (personResponse == null)
      {
        return RedirectToAction("Index");
      }

      return View(personResponse);
    }

    [HttpPost]
    [Route("[action]/{personID:guid}")]
    public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest)
    {
      PersonResponse? personResponse =
          await _personsGetterService.GetPersonByPersonID(personUpdateRequest.PersonID);

      if (personResponse == null)
      {
        return RedirectToAction("Index");
      }

      await _personsDeleterService.DeletePerson(personUpdateRequest.PersonID);
      return RedirectToAction("Index");
    }

    [Route("[action]")]
    public async Task<IActionResult> PersonsPDF()
    {
      //Get list of persons
      List<PersonResponse> persons = await _personsGetterService.GetAllPersons();

      //Return view as pdf
      return new ViewAsPdf("PersonsPDF", persons, ViewData)
      {
        PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 10, 20, 10),
        PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
      };
    }

    [Route("[action]")]
    public async Task<IActionResult> PersonsCSV()
    {
      MemoryStream memoryStream = await _personsGetterService.GetPersonsCSV();

      return File(memoryStream, "application/octet-stream", "persons.csv");
    }

    [Route("[action]")]
    public async Task<IActionResult> PersonsExcel()
    {
      MemoryStream memoryStream = await _personsGetterService.GetPersonsExcel();

      return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
    }
  }
}