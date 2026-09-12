using OfficeOpenXml;
using ServiceContracts.DTO;
using ServiceContracts.PersonsServiceContracts;

namespace Services.PersonsService
{
  public class PersonsGetterServiceWithFewExcelFields : IPersonsGetterService
  {
    private readonly IPersonsGetterService _personsGetterService;

    public PersonsGetterServiceWithFewExcelFields(IPersonsGetterService personsGetterService)
    {
      _personsGetterService = personsGetterService;
    }

    public async Task<List<PersonResponse>> GetAllPersons()
    {
      return await _personsGetterService.GetAllPersons();
    }

    public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
    {
      return await _personsGetterService.GetFilteredPersons(searchBy, searchString);
    }

    public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
    {
      return await _personsGetterService.GetPersonByPersonID(personID);
    }

    public async Task<MemoryStream> GetPersonsCSV()
    {
      return await _personsGetterService.GetPersonsCSV();
    }

    public async Task<MemoryStream> GetPersonsExcel()
    {
      MemoryStream memoryStream = new MemoryStream();
      using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
      {
        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");

        //Headers
        worksheet.Cells["A1"].Value = nameof(PersonResponse.PersonName);
        worksheet.Cells["B1"].Value = nameof(PersonResponse.Email);
        worksheet.Cells["C1"].Value = nameof(PersonResponse.DateOfBirth);
        worksheet.Cells["D1"].Value = nameof(PersonResponse.Age);
        worksheet.Cells["E1"].Value = nameof(PersonResponse.Gender);

        using (ExcelRange headerRange = worksheet.Cells["A1:E1"])
        {
          headerRange.Style.Font.Bold = true;
          headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
          headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        int row = 2;
        List<PersonResponse> allPersons = await GetAllPersons();

        foreach (PersonResponse person in allPersons)
        {
          worksheet.Cells[row, 1].Value = person.PersonName;
          worksheet.Cells[row, 2].Value = person.Email;
          worksheet.Cells[row, 3].Value = person.DateOfBirth?.ToString("dd MMM yyyy");
          worksheet.Cells[row, 4].Value = person.Age;
          worksheet.Cells[row, 5].Value = person.Gender;

          row++;
        }

        worksheet.Cells[$"A1:E{row}"].AutoFitColumns();

        await excelPackage.SaveAsAsync(memoryStream);
      }
      memoryStream.Position = 0;

      return memoryStream;
    }
  }
}
