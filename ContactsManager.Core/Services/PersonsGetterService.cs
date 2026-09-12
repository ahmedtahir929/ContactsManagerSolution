using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using Serilog;
using SerilogTimings;
using ServiceContracts.DTO;
using ServiceContracts.PersonsServiceContracts;
using System.Globalization;
using System.Reflection;

namespace Services.PersonsService
{
  public class PersonsGetterService : IPersonsGetterService
  {
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<PersonsGetterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;
    public PersonsGetterService(IPersonsRepository personsRepository, ILogger<PersonsGetterService> logger,
        IDiagnosticContext diagnosticContext)
    {
      _personsRepository = personsRepository;
      _logger = logger;
      _diagnosticContext = diagnosticContext;
    }

    /// <summary>
    /// This method is used to get all the persons from the database and return the list of PersonResponse objects
    /// </summary>
    /// <returns></returns>
    public async Task<List<PersonResponse>> GetAllPersons()
    {
      _logger.LogInformation("GetAllPersons of PersonsService");

      var persons = await _personsRepository.GetAllPersons();
      //SELECT * FROM Persons
      //Reads all the records from Persons table (ResultSet) and convert each record into PersonResponse type and return the list of PersonResponse objects
      return persons.Select(temp => temp.ToPersonResponse()).ToList();
    }

    public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
    {
      if (personID == null)
      {
        return null;
      }

      Person? person = await _personsRepository.GetPersonByPersonID(personID.Value);

      if (person == null)
      {
        return null;
      }

      return person.ToPersonResponse();
    }

    public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
    {
      _logger.LogInformation("GetFilteredPersons of PersonsService");

      if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
      {
        return await GetAllPersons();
      }

      PropertyInfo? property = typeof(Person).GetProperty(searchBy);

      if (property == null)
      {
        return await GetAllPersons();
      }

      List<Person> matchingPersons;

      using (Operation.Time("Time for Filtered Persons from Database"))
      {
        matchingPersons = searchBy switch
        {
          nameof(PersonResponse.PersonName) =>
              await _personsRepository.GetFilteredPersons(p =>
                  p.PersonName!.Contains(searchString)),

          nameof(PersonResponse.Email) =>
              await _personsRepository.GetFilteredPersons(p =>
                  p.Email!.Contains(searchString)),


          nameof(PersonResponse.DateOfBirth) =>
              await _personsRepository.GetFilteredPersons(p =>
                  p.DateOfBirth!.Value.ToString("dd MMM yyyy").Contains(searchString)),

          nameof(PersonResponse.Gender) =>
              await _personsRepository.GetFilteredPersons(p =>
                  p.Gender!.Contains(searchString)),

          nameof(PersonResponse.CountryID) =>
              await _personsRepository.GetFilteredPersons(p =>
                  p.Country!.CountryName!.Contains(searchString)),

          nameof(PersonResponse.Address) =>
              await _personsRepository.GetFilteredPersons(p =>
                  p.Address!.Contains(searchString)),

          _ => await _personsRepository.GetAllPersons()
        };
      } //end of using block of serilog timings

      _diagnosticContext.Set("Persons", matchingPersons);

      return matchingPersons.Select(temp => temp.ToPersonResponse()).ToList();
    }

    public async Task<MemoryStream> GetPersonsCSV()
    {
      MemoryStream memoryStream = new MemoryStream();
      //StreamWriter is used to write data into the memory stream in a specific format (e.g., CSV)
      StreamWriter streamWriter = new StreamWriter(memoryStream);

      CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
      CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration, leaveOpen: true);

      //Headers
      //PersonName,Email,DateOfBirth,Age,...(other properties of PersonResponse)
      csvWriter.WriteField(nameof(PersonResponse.PersonName));
      csvWriter.WriteField(nameof(PersonResponse.Email));
      csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
      csvWriter.WriteField(nameof(PersonResponse.Age));
      csvWriter.WriteField(nameof(PersonResponse.Gender));
      csvWriter.WriteField(nameof(PersonResponse.Country));
      csvWriter.WriteField(nameof(PersonResponse.Address));
      csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));

      csvWriter.NextRecord();

      List<PersonResponse> allPersons = await GetAllPersons();

      //Values
      //Writing each person's details into the CSV file
      foreach (PersonResponse person in allPersons)
      {
        csvWriter.WriteField((person.PersonName));
        csvWriter.WriteField((person.Email));

        if (person.DateOfBirth.HasValue)
          csvWriter.WriteField(person.DateOfBirth.Value.ToString("dd MMM yyyy"));
        else
          csvWriter.WriteField(string.Empty);

        csvWriter.WriteField((person.Age));
        csvWriter.WriteField((person.Gender));
        csvWriter.WriteField((person.Country));
        csvWriter.WriteField((person.Address));
        csvWriter.WriteField((person.ReceiveNewsLetters));
        csvWriter.NextRecord();
        csvWriter.Flush();
      }

      memoryStream.Position = 0;
      return memoryStream;
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
        worksheet.Cells["F1"].Value = nameof(PersonResponse.Country);
        worksheet.Cells["G1"].Value = nameof(PersonResponse.Address);
        worksheet.Cells["H1"].Value = nameof(PersonResponse.ReceiveNewsLetters);

        using (ExcelRange headerRange = worksheet.Cells["A1:H1"])
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
          worksheet.Cells[row, 6].Value = person.Country;
          worksheet.Cells[row, 7].Value = person.Address;
          worksheet.Cells[row, 8].Value = person.ReceiveNewsLetters;

          row++;
        }

        worksheet.Cells[$"A1:H{row}"].AutoFitColumns();

        await excelPackage.SaveAsAsync(memoryStream);
      }
      memoryStream.Position = 0;

      return memoryStream;
    }
  }
}