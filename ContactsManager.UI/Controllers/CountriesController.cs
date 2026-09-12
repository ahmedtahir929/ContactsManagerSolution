using Microsoft.AspNetCore.Mvc;
using ServiceContracts.CountriesServiceContracts;

namespace CRUDExample.Controllers
{
  [Route("[controller]")]
  public class CountriesController : Controller
  {
    private readonly ICountriesUploaderService _countriesUploaderService;

    public CountriesController(ICountriesUploaderService countriesUploaderService)
    {
      _countriesUploaderService = countriesUploaderService;
    }

    [Route("[action]")]
    public IActionResult UploadFromExcel()
    {
      return View();
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
    {
      if (excelFile == null || excelFile.Length == 0)
      {
        ViewBag.ErrorMessage = "Please select a valid Excel file.";
        return View();
      }

      if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
      {
        ViewBag.ErrorMessage = "Unsupported file. 'xlsx' file is expected";
        return View();
      }

      int numOfCountriesInserted =
        await _countriesUploaderService.UploadCountriesFromExcelFile(excelFile);

      ViewBag.SuccessMessage = $"{numOfCountriesInserted} countries have been inserted successfully.";

      return View();
    }
  }
}
