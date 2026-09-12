using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Exceptions;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using Serilog;
using SerilogTimings;
using ServiceContracts.DTO;
using ServiceContracts.PersonsServiceContracts;
namespace Services.PersonsService
{
  public class PersonsDeleterService : IPersonsDeleterService
  {
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<PersonsDeleterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;
    public PersonsDeleterService(IPersonsRepository personsRepository, ILogger<PersonsDeleterService> logger,
        IDiagnosticContext diagnosticContext)
    {
      _personsRepository = personsRepository;
      _logger = logger;
      _diagnosticContext = diagnosticContext;
    }

    public async Task<bool> DeletePerson(Guid? personID)
    {
      _logger.LogInformation("{ServiceName}.{MethodName}() invoked",
        nameof(PersonsDeleterService), nameof(DeletePerson));

      if (personID == null)
      {
        throw new ArgumentException("Person ID cannot be empty.", nameof(personID));
      }

      //Calling the stored procedure to delete the person record from the database
      var person = await _personsRepository.GetPersonByPersonID(personID.Value);

      if (person == null)
      {
        return false; // No record found to delete
      }

      await _personsRepository.DeletePersonByPersonID(personID.Value);

      return true;
    }
  }
}