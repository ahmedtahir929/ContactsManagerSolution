using AutoFixture;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Xunit.Abstractions;
using FluentAssertions;
using RepositoryContracts;
using Moq;
using System.Linq.Expressions;
using Serilog;
using Microsoft.Extensions.Logging;
using Services.PersonsService;
using ServiceContracts.PersonsServiceContracts;

namespace CRUDTests
{
  public class PersonsServiceTest
  {
    //private fields
    private readonly IPersonsGetterService _personsGetterService;
    private readonly IPersonsAdderService _personsAdderService;
    private readonly IPersonsUpdaterService _personsUpdaterService;
    private readonly IPersonsDeleterService _personsDeleterService;
    private readonly IPersonsSorterService _personsSorterService;
    private readonly IPersonsRepository _personsRepository;
    private readonly Mock<IPersonsRepository> _personsRepositoryMock;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IFixture _fixture;

    //constructor
    public PersonsServiceTest(ITestOutputHelper testOutputHelper)
    {
      _testOutputHelper = testOutputHelper;
      _fixture = new Fixture();

      var diagnosticContextMock = new Mock<IDiagnosticContext>();
      var loggerGetterMock = new Mock<ILogger<PersonsGetterService>>();
      var loggerAdderMock = new Mock<ILogger<PersonsAdderService>>();
      var loggerDeleterMock = new Mock<ILogger<PersonsDeleterService>>();
      var loggerSorterMock = new Mock<ILogger<PersonsSorterService>>();
      var loggerUpdaterMock = new Mock<ILogger<PersonsUpdaterService>>();

      _personsRepositoryMock = new Mock<IPersonsRepository>();
      _personsRepository = _personsRepositoryMock.Object; //Represents mock persons repository

      _personsGetterService = new PersonsGetterService(
        _personsRepository,
        loggerGetterMock.Object,
        diagnosticContextMock.Object);

      _personsAdderService = new PersonsAdderService(
          _personsRepository,
          loggerAdderMock.Object,
          diagnosticContextMock.Object);

      _personsDeleterService = new PersonsDeleterService(
          _personsRepository,
          loggerDeleterMock.Object,
          diagnosticContextMock.Object);

      _personsSorterService = new PersonsSorterService(
          loggerSorterMock.Object,
          diagnosticContextMock.Object);

      _personsUpdaterService = new PersonsUpdaterService(
          _personsRepository,
          loggerUpdaterMock.Object,
          diagnosticContextMock.Object);
    }

    #region AddPerson
    //When we supply null value as PersonAddRequest, it should throw ArgumentNullException
    [Fact]
    public async Task AddPerson_NullPerson_ToBeArgumentNullException()
    {
      //Arrange
      PersonAddRequest? personAddRequest = null;

      //Act
      Func<Task> action = async () =>
      {
        await _personsAdderService.AddPerson(personAddRequest);
      };

      //Assert
      await action.Should().ThrowAsync<ArgumentNullException>();
    }

    //When we supply null value as PersonName, it should throw ArgumentException
    [Fact]
    public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
    {
      //Arrange
      PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
          .With(temp => temp.PersonName, null as string)
          .Create();

      Person person = personAddRequest.ToPerson();

      //When PersonsRepository.AddPerson is called, it has to return the same "person" object
      _personsRepositoryMock
          .Setup(temp => temp.AddPerson(It.IsAny<Person>()))
          .ReturnsAsync(person);

      //Act
      Func<Task> action = async () =>
      {
        await _personsAdderService.AddPerson(personAddRequest);
      };

      //Assert
      await action.Should().ThrowAsync<ArgumentException>();
    }

    //When we supply proper person details, it should insert the person into
    //the persons list;
    //and it should return an object of PersonReponse, which includes with the newly
    //generated person id
    [Fact]
    public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
    {
      //Arrange
      PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
          .With(temp => temp.Email, "abc@example.com")
          .Create();

      Person person = personAddRequest.ToPerson();

      PersonResponse person_response_expected = person.ToPersonResponse();

      _personsRepositoryMock
          .Setup(temp => temp.AddPerson(It.IsAny<Person>()))
          .ReturnsAsync(person);

      //Act
      PersonResponse person_response_from_add =
          await _personsAdderService.AddPerson(personAddRequest);

      person_response_expected.PersonID = person_response_from_add.PersonID;
      person_response_expected.TIN = person_response_from_add.TIN;

      /* Assert */
      //In fluent assertion, first you have to choose the actual object, and then you have to choose the expected object

      person_response_from_add.PersonID.Should().NotBe(Guid.Empty);
      person_response_from_add.Should().BeEquivalentTo(person_response_expected);
    }
    #endregion

    #region GetPersonByPersonID
    //If we supply null as PersonID, it should return null as PersonResponse
    [Fact]
    public async Task GetPersonByPersonID_NullID_ToBeNull()
    {
      //Arrange
      Guid? personID = null;

      //Act
      PersonResponse? person_response_from_get =
          await _personsGetterService.GetPersonByPersonID(personID);

      //Assert
      person_response_from_get.Should().BeNull();
    }

    //If we supply a valid personID, then it should return the valid person details
    //as PersonResponse object
    [Fact]
    public async Task GetPersonByPersonID_WithValidPersonID_ToBeSuccessful()
    {
      //Arrange
      Person person = _fixture.Build<Person>()
          .With(temp => temp.Email, "joe@example.com")
          .With(temp => temp.Country, null as Country)
          .Create();

      //Mocking the GetPersonByPersonID() repository method
      _personsRepositoryMock
          .Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
          .ReturnsAsync(person);

      PersonResponse person_response_expected = person.ToPersonResponse();

      //Act
      PersonResponse? person_response_from_get =
          await _personsGetterService.GetPersonByPersonID(person.PersonID);

      //Assert
      person_response_from_get.Should().Be(person_response_expected);
    }
    #endregion

    #region GetAllPersons

    //The GetAllPersons() should return an empty list by default
    [Fact]
    public async Task GetAllPersons_EmptyList_ToBeEmptyList()
    {
      //Arrange
      var persons = new List<Person>();

      _personsRepositoryMock
          .Setup(temp => temp.GetAllPersons())
          .ReturnsAsync(persons);

      //Act
      List<PersonResponse> persons_list_from_get = await _personsGetterService.GetAllPersons();

      //Assert
      persons_list_from_get.Should().BeEmpty();
    }

    //First, we will add few persons; and then when we call GetAllPersons(), it should
    //return the same persons that were added
    [Fact]
    public async Task GetAllPersons_WithFewPersons_ToBeSuccessful()
    {
      //Arrange
      List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

      List<PersonResponse> person_response_list_expected =
          persons.Select(temp => temp.ToPersonResponse()).ToList();

      //print person_response_list_from_add
      _testOutputHelper.WriteLine("Expected:");
      foreach (PersonResponse person_response_from_add in person_response_list_expected)
      {
        _testOutputHelper.WriteLine(person_response_from_add.ToString());
      }

      _personsRepositoryMock
          .Setup(temp => temp.GetAllPersons())
          .ReturnsAsync(persons);

      //Act
      List<PersonResponse> persons_response_list_from_get =
          await _personsGetterService.GetAllPersons();

      //print persons_response_list_from_get
      _testOutputHelper.WriteLine("Actual:");
      foreach (PersonResponse person_response_from_get in persons_response_list_from_get)
      {
        _testOutputHelper.WriteLine(person_response_from_get.ToString());
      }

      //Assert
      persons_response_list_from_get.Should().BeEquivalentTo(person_response_list_expected);
    }
    #endregion

    #region GetFilteredPersons

    //If the search text is empty, and search by is "PersonName", it should return all persons
    [Fact]
    public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful()
    {
      //Arrange
      List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

      List<PersonResponse> person_response_list_expected =
          persons.Select(temp => temp.ToPersonResponse()).ToList();

      //print person_response_list_from_add
      _testOutputHelper.WriteLine("Expected:");
      foreach (PersonResponse person_response in person_response_list_expected)
      {
        _testOutputHelper.WriteLine(person_response.ToString());
      }

      _personsRepositoryMock
          .Setup(temp => temp.GetAllPersons())
          .ReturnsAsync(persons);

      //Act
      List<PersonResponse> persons_response_list_from_search =
          await _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "");


      //print persons_response_list_from_get
      _testOutputHelper.WriteLine("Actual:");
      foreach (PersonResponse person_response_from_get in persons_response_list_from_search)
      {
        _testOutputHelper.WriteLine(person_response_from_get.ToString());
      }

      /* Assert */
      persons_response_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
    }

    //First we will add a few persons; and then we will search based on person name
    //with some search string. It should return the matching persons
    [Fact]
    public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
    {
      //Arrange
      List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

      List<PersonResponse> person_response_list_expected =
          persons.Select(temp => temp.ToPersonResponse()).ToList();

      //print person_response_list_from_add
      _testOutputHelper.WriteLine("Expected:");
      foreach (PersonResponse person_response_from_add in person_response_list_expected)
      {
        _testOutputHelper.WriteLine(person_response_from_add.ToString());
      }

      _personsRepositoryMock
          .Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
          .ReturnsAsync(persons);

      //Act
      List<PersonResponse> persons_response_list_from_search =
          await _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "ma"); //searching if some person's PersonName conatains "ma"


      //print persons_response_list_from_get
      _testOutputHelper.WriteLine("Actual:");
      foreach (PersonResponse person_response_from_get in persons_response_list_from_search)
      {
        _testOutputHelper.WriteLine(person_response_from_get.ToString());
      }

      /* Assert */
      persons_response_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
    }
    #endregion

    #region GetSortedPersons
    //When we sort based on the PersonName in DESC,
    //it should return the persons list in descending on PersonName 
    [Fact]
    public async Task GetSortedPersons_ToBeSuccessful()
    {
      //Arrange
      List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

      List<PersonResponse> person_response_list_expected =
          persons.Select(temp => temp.ToPersonResponse()).ToList();

      _personsRepositoryMock
          .Setup(temp => temp.GetAllPersons())
          .ReturnsAsync(persons);

      //Expected
      _testOutputHelper.WriteLine("Expected:");
      foreach (PersonResponse person_response_from_add in person_response_list_expected)
      {
        _testOutputHelper.WriteLine(person_response_from_add.ToString());
      }
      List<PersonResponse> allPersons = await _personsGetterService.GetAllPersons();

      //Act
      List<PersonResponse> persons_response_list_from_sort =
         await _personsSorterService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);

      //Actual
      _testOutputHelper.WriteLine("Actual:");
      foreach (PersonResponse person_response_from_sort in persons_response_list_from_sort)
      {
        _testOutputHelper.WriteLine(person_response_from_sort.ToString());
      }

      /* Assert */
      persons_response_list_from_sort.Should().BeInDescendingOrder(temp => temp.PersonName);
    }
    #endregion

    #region UpdatePerson
    //When you supply null value as PersonUpdateRequest, it should throw ArgumentNullException
    [Fact]
    public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
    {
      //Arrange
      PersonUpdateRequest? person_update_request = null;

      //Act
      Func<Task> action = async () =>
      {
        await _personsUpdaterService.UpdatePerson(person_update_request);
      };

      //Assert
      await action.Should().ThrowAsync<ArgumentNullException>();
    }

    //When we supply invalid person ID it should throw ArgumentException
    [Fact]
    public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
    {
      //Arrange
      PersonUpdateRequest person_update_request = new PersonUpdateRequest() { PersonID = Guid.NewGuid() };

      //Act
      Func<Task> action = async () =>
      {
        await _personsUpdaterService.UpdatePerson(person_update_request);
      };

      //Assert
      await action.Should().ThrowAsync<ArgumentException>();
    }

    //When PersonName is null, it should throw ArgumentException
    [Fact]
    public async Task UpdatePerson_NullPersonName_ToBeArgumentException()
    {
      //Arrange
      Person person = _fixture.Build<Person>()
          .With(temp => temp.PersonName, null as string)
          .With(temp => temp.Email, "someone@example.com")
          .With(temp => temp.Country, null as Country)
          .With(temp => temp.Gender, "Male")
          .Create();

      PersonResponse person_response = person.ToPersonResponse();

      PersonUpdateRequest person_update_request = person_response.ToPersonUpdateRequest();


      //Act
      Func<Task> action = async () =>
      {
        await _personsUpdaterService.UpdatePerson(person_update_request);
      };

      //Assert
      await action.Should().ThrowAsync<ArgumentException>();
    }

    //When Email is null, it should throw ArgumentException
    [Fact]
    public async Task UpdatePerson_NullEmail_ToBeArgumentException()
    {
      //Arrange
      Person person = _fixture.Build<Person>()
          .With(temp => temp.Email, null as string)
          .With(temp => temp.Country, null as Country)
          .With(temp => temp.Gender, "Male")
          .Create();

      PersonResponse person_response = person.ToPersonResponse();

      PersonUpdateRequest person_update_request = person_response.ToPersonUpdateRequest();

      //Act
      Func<Task> action = async () =>
      {
        await _personsUpdaterService.UpdatePerson(person_update_request);
      };

      //Assert
      await action.Should().ThrowAsync<ArgumentException>();
    }

    //First we will add a new person, and try to update the person name and email
    [Fact]
    public async Task UpdatePerson_PersonFullDetailsUpdation()
    {
      //Arrange
      Person person = _fixture.Build<Person>()
          .With(temp => temp.Email, "someone@example.com")
          .With(temp => temp.Country, null as Country)
          .With(temp => temp.Gender, "Male")
          .Create();

      PersonResponse person_response_expected = person.ToPersonResponse();

      PersonUpdateRequest person_update_request = person_response_expected.ToPersonUpdateRequest();

      _personsRepositoryMock
          .Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
          .ReturnsAsync(person);
      _personsRepositoryMock
          .Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
          .ReturnsAsync(person);

      //Act
      PersonResponse person_response_from_update = await _personsUpdaterService.UpdatePerson(person_update_request);

      /* Assert */
      person_response_from_update.Should().Be(person_response_expected);
    }
    #endregion

    #region DeletePerson

    //If you supply a valid PersonID, it should return true
    [Fact]
    public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
    {
      //Arrange
      Person person = _fixture.Build<Person>()
          .With(temp => temp.PersonName, "Rahman")
          .With(temp => temp.Email, "someone@example.com")
          .With(temp => temp.Country, null as Country)
          .With(temp => temp.Gender, "Male")
          .Create();

      _personsRepositoryMock
          .Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
          .ReturnsAsync(true);
      _personsRepositoryMock
          .Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
          .ReturnsAsync(person);

      //Act
      bool isDeleted = await _personsDeleterService.DeletePerson(person.PersonID);

      /* Assert */
      isDeleted.Should().BeTrue();
    }

    //If you supply an invalid PersonID, it should return false
    [Fact]
    public async Task DeletePerson_InvalidPersonID()
    {
      //Act
      bool isDeleted = await _personsDeleterService.DeletePerson(Guid.NewGuid());

      /* Assert */
      //Assert.False(isDeleted);
      isDeleted.Should().BeFalse();
    }

    #endregion
  }
}