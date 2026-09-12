using Microsoft.Extensions.Logging;
using Serilog;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using ServiceContracts.PersonsServiceContracts;
using System.Reflection;

namespace Services.PersonsService
{
  public class PersonsSorterService : IPersonsSorterService
  {
    private readonly ILogger<PersonsSorterService> _logger;

    public PersonsSorterService(ILogger<PersonsSorterService> logger,
        IDiagnosticContext diagnosticContext)
    {
      _logger = logger;
    }

    public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy,
        SortOrderOptions sortOrder)
    {
      _logger.LogInformation("GetSortedPersons of PersonsService");

      if (string.IsNullOrEmpty(sortBy))
        return allPersons;

      PropertyInfo? property = typeof(PersonResponse).GetProperty(sortBy);

      if (property == null)
        return allPersons;

      //Handling properties with string data type
      if (property.PropertyType == typeof(string))
      {
        var sortedPersonsStringFields = (sortOrder == SortOrderOptions.ASC)
            ? allPersons.OrderBy(temp =>
              (string?)property.GetValue(temp), StringComparer.OrdinalIgnoreCase).ToList()
            : allPersons.OrderByDescending(temp =>
              (string?)property.GetValue(temp), StringComparer.OrdinalIgnoreCase).ToList();

        return sortedPersonsStringFields;
      }

      Func<PersonResponse, object?> keySelector = person =>
          property.GetValue(person);

      var sortedPersons = (sortOrder == SortOrderOptions.ASC)
          ? allPersons.OrderBy(keySelector)
          : allPersons.OrderByDescending(keySelector);

      return sortedPersons.ToList();
    }
  }
}