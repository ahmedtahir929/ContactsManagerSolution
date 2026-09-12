using Entities;
using Exceptions;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using Serilog;
using ServiceContracts.DTO;
using ServiceContracts.PersonsServiceContracts;
using Services.Helpers;

namespace Services.PersonsService
{
  public class PersonsUpdaterService : IPersonsUpdaterService
  {
    private readonly IPersonsRepository _personsRepository;
    private readonly ILogger<PersonsUpdaterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;
    public PersonsUpdaterService(IPersonsRepository personsRepository, ILogger<PersonsUpdaterService> logger,
        IDiagnosticContext diagnosticContext)
    {
      _personsRepository = personsRepository;
      _logger = logger;
      _diagnosticContext = diagnosticContext;
    }

    public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
    {
      _logger.LogInformation("{ServiceName}.{MethodName}() invoked",
        nameof(PersonsUpdaterService), nameof(UpdatePerson));

      //Checking if the personUpdateRequest obj is null
      ArgumentNullException.ThrowIfNull(personUpdateRequest);

      //Validation
      ValidationHelper.ModelValidation(personUpdateRequest);

      //Checking if the person record exists in the database for the given personID
      Person? matchingPerson =
          await _personsRepository.GetPersonByPersonID(personUpdateRequest.PersonID)
          ?? throw new InvalidPersonIDException("Given person ID doesn't exist");

      //Update all details
      matchingPerson.PersonName = personUpdateRequest.PersonName;
      matchingPerson.Email = personUpdateRequest.Email;
      matchingPerson.Address = personUpdateRequest.Address;
      matchingPerson.CountryID = personUpdateRequest.CountryID;
      matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
      matchingPerson.Gender = Convert.ToString(personUpdateRequest.Gender);
      matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

      await _personsRepository.UpdatePerson(matchingPerson);

      //Return the updated as PersonResponse
      return matchingPerson.ToPersonResponse();
    }
  }
}