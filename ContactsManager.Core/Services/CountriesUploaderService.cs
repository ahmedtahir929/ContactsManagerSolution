using Entities;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts.CountriesServiceContracts;

namespace Services.CountriesService
{
  public class CountriesUploaderService : ICountriesUploaderService
  {
    private readonly ICountriesRepository _countriesRepository;

    //constructor
    public CountriesUploaderService(ICountriesRepository countriesRepository)
    {
      _countriesRepository = countriesRepository;
    }

    public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
    {
      MemoryStream memoryStream = new MemoryStream();

      await formFile.CopyToAsync(memoryStream);

      int numOfCountriesInserted = 0;

      using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
      {
        ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets["Countries"];

        int rowCount = workSheet.Dimension.Rows;

        for (int row = 2; row <= rowCount; row++)
        {
          string? cellValue = Convert.ToString(workSheet.Cells[row, 1].Value);

          if (!string.IsNullOrEmpty(cellValue))
          {
            string? countryName = cellValue;

            if (_countriesRepository.GetCountryByCountryName(countryName) == null)
            {
              Country country = new Country()
              {
                CountryName = countryName
              };

              await _countriesRepository.AddCountry(country);

              numOfCountriesInserted++;
            }
          }
        }
      }
      return numOfCountriesInserted;
    }
  }
}
