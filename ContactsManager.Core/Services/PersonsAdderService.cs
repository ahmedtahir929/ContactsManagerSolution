using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using Serilog;
using ServiceContracts.DTO;
using ServiceContracts.PersonsServiceContracts;
using Services.Helpers;

namespace Services.PersonsService
{
  public class PersonsAdderService : IPersonsAdderService
  {
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<PersonsAdderService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;
    public PersonsAdderService(IPersonsRepository personsRepository, ILogger<PersonsAdderService> logger,
        IDiagnosticContext diagnosticContext)
    {
      _personsRepository = personsRepository;
      _logger = logger;
      _diagnosticContext = diagnosticContext;
    }

    public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
    {
      _logger.LogInformation("{ServiceName}.{MethodName}() invoked",
        nameof(PersonsAdderService), nameof(AddPerson));

      ArgumentNullException.ThrowIfNull(personAddRequest);

      //Model validations
      ValidationHelper.ModelValidation(personAddRequest);

      //convert personAddRequest into Person
      Person person = personAddRequest.ToPerson();

      //generate PersonID
      person.PersonID = Guid.NewGuid();
      person.TIN = TinGeneratorHelper.GenerateTIN();

      await _personsRepository.AddPerson(person);

      //convert the Person object into PersonResponse type
      return person.ToPersonResponse();
    }
  }
}