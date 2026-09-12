using AutoFixture;
using CRUDExample.Controllers;
using CRUDExample.Filters.ActionFilters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceContracts.CountriesServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using ServiceContracts.PersonsServiceContracts;

namespace CRUDTests
{
  public class PersonsControllerTest
  {
    private readonly IFixture _fixture;

    private readonly IPersonsGetterService _personsGetterService;
    private readonly IPersonsAdderService _personsAdderService;
    private readonly IPersonsUpdaterService _personsUpdaterService;
    private readonly IPersonsDeleterService _personsDeleterService;
    private readonly IPersonsSorterService _personsSorterService;
    private readonly ICountriesGetterService _countriesGetterService;

    private readonly Mock<IPersonsGetterService> _personsGetterServiceMock;
    private readonly Mock<IPersonsAdderService> _personsAdderServiceMock;
    private readonly Mock<IPersonsDeleterService> _personsDeleterServiceMock;
    private readonly Mock<IPersonsSorterService> _personsSorterServiceMock;
    private readonly Mock<IPersonsUpdaterService> _personsUpdaterServiceMock;
    private readonly Mock<ICountriesGetterService> _countriesGetterServiceMock;
    private readonly Mock<ILogger<PersonsController>> _loggerMockPersonsController;
    private readonly Mock<ILogger<PersonCreateAndEditPostActionFilter>> _loggerMockPersonCreateAndEditPostActionFilter;

    public PersonsControllerTest()
    {
      _fixture = new Fixture();
      _loggerMockPersonsController = new Mock<ILogger<PersonsController>>();
      _loggerMockPersonCreateAndEditPostActionFilter = new Mock<ILogger<PersonCreateAndEditPostActionFilter>>();

      _personsGetterServiceMock = new Mock<IPersonsGetterService>();
      _personsAdderServiceMock = new Mock<IPersonsAdderService>();
      _personsDeleterServiceMock = new Mock<IPersonsDeleterService>();
      _personsSorterServiceMock = new Mock<IPersonsSorterService>();
      _personsUpdaterServiceMock = new Mock<IPersonsUpdaterService>();
      _countriesGetterServiceMock = new Mock<ICountriesGetterService>();

      _personsGetterService = _personsGetterServiceMock.Object;
      _personsAdderService = _personsAdderServiceMock.Object;
      _personsUpdaterService = _personsUpdaterServiceMock.Object;
      _personsDeleterService = _personsDeleterServiceMock.Object;
      _personsSorterService = _personsSorterServiceMock.Object;

      _countriesGetterService = _countriesGetterServiceMock.Object;
    }

    #region Index

    [Fact]
    public async Task Index_ShouldReturnIndexViewWithPersonsList()
    {
      //Arrange
      List<PersonResponse> person_response_list = _fixture.Create<List<PersonResponse>>();

      PersonsController personsController =
          new PersonsController(
            _loggerMockPersonsController.Object,
            _countriesGetterService,
            _personsGetterService,
            _personsAdderService,
            _personsUpdaterService,
            _personsDeleterService,
            _personsSorterService
          );

      _personsGetterServiceMock
          .Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
          .ReturnsAsync(person_response_list);
      _personsSorterServiceMock
          .Setup(temp => temp.GetSortedPersons(
              It.IsAny<List<PersonResponse>>(),
              It.IsAny<string>(),
              It.IsAny<SortOrderOptions>())
          )
          .ReturnsAsync(person_response_list);

      //Act
      IActionResult result =
          await personsController.Index(_fixture.Create<string>(), _fixture.Create<string>(),
          _fixture.Create<string>(), _fixture.Create<SortOrderOptions>());

      //Assert
      ViewResult viewResult = Assert.IsType<ViewResult>(result);

      viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>(); //Checks if the action method returns ViewResult with any collection type such as List<>
      viewResult.ViewData.Model.Should().BeEquivalentTo(person_response_list);
    }

    #endregion

    #region Create

    [Fact]
    public async Task OnActionExecutionAsync_IfModelStateIsInvalid_ToReturnCreateView()
    {
      // Arrange
      PersonAddRequest personAddRequest =
          _fixture.Create<PersonAddRequest>();

      List<CountryResponse> countryResponseList =
          _fixture.Create<List<CountryResponse>>();

      PersonsController personsController =
          new PersonsController(
            _loggerMockPersonsController.Object,
            _countriesGetterService,
            _personsGetterService,
            _personsAdderService,
            _personsUpdaterService,
            _personsDeleterService,
            _personsSorterService
          );

      personsController.ModelState.AddModelError(
          "PersonName",
          "PersonName can't be blank");

      _countriesGetterServiceMock
          .Setup(temp => temp.GetAllCountries())
          .ReturnsAsync(countryResponseList);

      var httpContext =
          new DefaultHttpContext();

      var actionContext =
          new ActionContext(
              httpContext,
              new RouteData(),
              new ControllerActionDescriptor(),
              personsController.ModelState);

      var actionExecutingContext =
          new ActionExecutingContext(
              actionContext,
              new List<IFilterMetadata>(),
              new Dictionary<string, object?>()
              {
                { "personAddRequest", personAddRequest }
              },
              personsController);

      bool nextCalled = false;

      ActionExecutionDelegate next = () =>
      {
        nextCalled = true;

        var executedContext =
            new ActionExecutedContext(
                actionExecutingContext,
                new List<IFilterMetadata>(),
                personsController);

        return Task.FromResult(executedContext);
      };

      var filter =
          new PersonCreateAndEditPostActionFilter(
              _countriesGetterServiceMock.Object,
              _loggerMockPersonCreateAndEditPostActionFilter.Object);

      // Act
      await filter.OnActionExecutionAsync(
          actionExecutingContext,
          next);

      // Assert
      nextCalled.Should().BeFalse();

      actionExecutingContext.Result
          .Should()
          .BeOfType<ViewResult>();

      ViewResult viewResult =
          Assert.IsType<ViewResult>(
              actionExecutingContext.Result);

      viewResult.ViewData.Model
          .Should()
          .BeEquivalentTo(personAddRequest);

      _countriesGetterServiceMock.Verify(
          temp => temp.GetAllCountries(),
          Times.Once);
    }

    [Fact]
    public async Task Create_IfNoModelErrors_ToReturnRedirectToIndex()
    {
      //Arrange
      PersonAddRequest person_add_request = _fixture.Create<PersonAddRequest>();

      PersonResponse person_response = _fixture.Create<PersonResponse>();

      List<CountryResponse> country_repsonse_list = _fixture.Create<List<CountryResponse>>();

      PersonsController personsController =
          new PersonsController(
            _loggerMockPersonsController.Object,
            _countriesGetterService,
            _personsGetterService,
            _personsAdderService,
            _personsUpdaterService,
            _personsDeleterService,
            _personsSorterService
          );

      _countriesGetterServiceMock
          .Setup(temp => temp.GetAllCountries())
          .ReturnsAsync(country_repsonse_list);

      _personsAdderServiceMock
          .Setup(temp => temp.AddPerson(
              It.IsAny<PersonAddRequest>()))
          .ReturnsAsync(person_response);

      //Act
      IActionResult result =
          await personsController.Create(person_add_request);

      //Assert
      RedirectToActionResult redirectToActionResult =
          Assert.IsType<RedirectToActionResult>(result);

      redirectToActionResult.ActionName.Should().Be("Index");
    }

    #endregion
  }
}
