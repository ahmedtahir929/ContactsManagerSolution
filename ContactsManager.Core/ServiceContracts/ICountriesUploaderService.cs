using Microsoft.AspNetCore.Http;

namespace ServiceContracts.CountriesServiceContracts
{
  /// <summary>
  /// Represents business logic for manipulating Country Entity
  /// </summary>
  public interface ICountriesUploaderService
  {
    /// <summary>
    /// Uploads countries from an Excel file
    /// </summary>
    /// <param name="formFile"></param>
    /// <returns>Returns number of countries added</returns>
    Task<int> UploadCountriesFromExcelFile(IFormFile formFile);
  }
}