using ServiceContracts.DTO;

namespace ServiceContracts.CountriesServiceContracts
{
  /// <summary>
  /// Represents business logic for manipulating Country Entity
  /// </summary>
  public interface ICountriesAdderService
  {
    /// <summary>
    /// Adds a country object to the list of countries
    /// </summary>
    /// <param name="countryAddRequest">CountryAddRequest object to be added</param>
    /// <returns>Returns the CountryResponse object after adding it
    /// (including newly generated id)</returns>
    Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);
  }
}