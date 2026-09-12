using ServiceContracts.DTO;
using Entities;
using AutoFixture;
using RepositoryContracts;
using Moq;
using FluentAssertions;
using ServiceContracts.CountriesServiceContracts;
using Services.CountriesService;

namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesAdderService _countriesAdderService;
        private readonly ICountriesGetterService _countriesGetterService;
        private readonly ICountriesRepository _countriesRepository;
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
        private readonly Fixture _fixture;

        public CountriesServiceTest()
        {
            _countriesRepositoryMock = new Mock<ICountriesRepository>();
            _countriesRepository = _countriesRepositoryMock.Object;

            _countriesAdderService = new CountriesAdderService(_countriesRepository);
            _countriesGetterService = new CountriesGetterService(_countriesRepository);

            _fixture = new Fixture();
        }

        #region AddCountry
        //When CountryAddRequest is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry_ToBeArgumentNullException()
        {
            //Arrange
            CountryAddRequest? country_add_request = null;

            //Act
            Func<Task> action = async () =>
            {
                await _countriesAdderService.AddCountry(country_add_request);
            };

            //Assert
            await action.Should().ThrowAsync <ArgumentNullException>();
        }


        //When the CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull_ToBeArgumentException()
        {
            //Arrange
            CountryAddRequest? country_add_request = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, null as string)
                .Create();

            //Act
            Func<Task> action = async () =>
            {
                await _countriesAdderService.AddCountry(country_add_request);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName_ToBeArgumentException()
        {
            //Arrange
            CountryAddRequest country_add_request1 = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, "USA")
                .Create();
            CountryAddRequest country_add_request2 = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, "USA")
                .Create();

            Country country = country_add_request1.ToCountry();

            CountryResponse country_response = country.ToCountryResponse();

            _countriesRepositoryMock
                .SetupSequence(temp => temp.GetCountryByCountryName(It.IsAny<string>()))
                .ReturnsAsync((Country?)null)
                .ReturnsAsync(country);
            _countriesRepositoryMock
                .Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country);

            //Act
            Func<Task> action = async () =>
            {
                await _countriesAdderService.AddCountry(country_add_request1);
                await _countriesAdderService.AddCountry(country_add_request2);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When you supply the proper country name, it should insert(add) the country to the
        //existing list of countries
        [Fact]
        public async Task AddCountry_ProperCountryDetails_ToBeSuccessful()
        {
            //Arrange   
            CountryAddRequest? countryAddRequest = _fixture.Create<CountryAddRequest>();

            Country country = countryAddRequest.ToCountry();

            CountryResponse country_response_expected = country.ToCountryResponse();

            _countriesRepositoryMock
                .SetupSequence(temp => temp.GetCountryByCountryName(It.IsAny<string>()))
                .ReturnsAsync((Country?)null)
                .ReturnsAsync(country);
            _countriesRepositoryMock
                .Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country);

            //Act
            CountryResponse country_response_actual = await _countriesAdderService.AddCountry(countryAddRequest);

            country_response_actual.CountryID = country_response_expected.CountryID;

            //Assert
            country_response_actual.Should().BeEquivalentTo(country_response_expected);
        }
        #endregion

        #region GetAllCountries
        [Fact]
        //The list of countries should be empty by default (before adding any countries)
        public async Task GetAllCountries_ToBeEmptyList()
        {
            //Arrange
            _countriesRepositoryMock
                .Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(new List<Country>());

            //Act
            List<CountryResponse> actual_country_response_list = await _countriesGetterService.GetAllCountries();

            //Assert
            actual_country_response_list.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllCountries_AddFewCountries_ToBeSuccessful()
        {
            //Arrange
            List<Country> country_list = new List<Country>() 
            {
                _fixture.Build<Country>()
                    .With(temp => temp.CountryName, "USA")
                    .With(temp => temp.Persons, null as ICollection<Person>)
                    .Create(),

                _fixture.Build<Country>()
                    .With(temp => temp.CountryName, "UK")
                    .With(temp => temp.Persons, null as ICollection<Person>)
                    .Create(),

                _fixture.Build<Country>()
                    .With(temp => temp.CountryName, "India")
                    .With(temp => temp.Persons, null as ICollection<Person>)
                    .Create()
            };

            List<CountryResponse> country_response_list_expected = 
                country_list.Select(temp => temp.ToCountryResponse()).ToList();

            _countriesRepositoryMock
                .Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(country_list);

            //Act
            List<CountryResponse> actual_country_response_list = await _countriesGetterService.GetAllCountries();

            //Assert
            actual_country_response_list.Should().BeEquivalentTo(country_response_list_expected);
        }
        #endregion

        #region GetCountryByCountryID
        [Fact]
        //If we supply null as CountryID, it should return null as CountryResponse
        public async Task GetCountryByCountryID_NullCountryId_ToBeNull()
        {
            //Arrange
            Guid? countryID = null;

            //Act
            CountryResponse? country_response_from_get_method =
                await _countriesGetterService.GetCountryByCountryID(countryID);

            //Assert
            country_response_from_get_method.Should().BeNull();
        }

        [Fact]
        //If we supply valid a country id, it should return the matching country details
        //as CountryResponse object
        public async Task GetCountryByCountryID_ValidCountryID_ToBeSuccessful()
        {
            //Arrange
            Country country = _fixture.Build<Country>()
                .With(temp => temp.Persons, null as ICollection<Person>)
                .Create();

            CountryResponse country_response_expected = country.ToCountryResponse();

            _countriesRepositoryMock
                .Setup(temp => temp.GetCountryByCountryID(It.IsAny<Guid>()))
                .ReturnsAsync(country);

            //Act
            CountryResponse? actual_country_response =
                await _countriesGetterService.GetCountryByCountryID(country_response_expected.CountryID);

            //Assert
            actual_country_response.Should().BeEquivalentTo(country_response_expected);
        }
        #endregion
    }
}