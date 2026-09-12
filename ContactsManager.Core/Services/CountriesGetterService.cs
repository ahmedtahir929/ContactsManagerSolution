using Entities;
using RepositoryContracts;
using ServiceContracts.CountriesServiceContracts;
using ServiceContracts.DTO;

namespace Services.CountriesService
{
  public class CountriesGetterService : ICountriesGetterService
  {
    private readonly ICountriesRepository _countriesRepository;

    //constructor
    public CountriesGetterService(ICountriesRepository countriesRepository)
    {
      _countriesRepository = countriesRepository;
    }

    public async Task<List<CountryResponse>> GetAllCountries()
    {
      List<Country> countries = await _countriesRepository.GetAllCountries();

      return
          countries
          .Select(country => country.ToCountryResponse())
          .ToList();
    }

    public async Task<CountryResponse?> GetCountryByCountryID(Guid? countryID)
    {
      if (countryID == null)
        return null;

      Country? country_response_from_list =
           await _countriesRepository.GetCountryByCountryID(countryID.Value);

      if (country_response_from_list == null)
        return null;

      return country_response_from_list.ToCountryResponse();
    }
  }
}
