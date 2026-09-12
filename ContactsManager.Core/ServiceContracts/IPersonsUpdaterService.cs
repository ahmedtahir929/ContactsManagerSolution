using ServiceContracts.DTO;

namespace ServiceContracts.PersonsServiceContracts
{
  /// <summary>
  /// Represents business logic for manipulating Person entity
  /// </summary>
  public interface IPersonsUpdaterService
  {
    /// <summary>
    /// Updates the person details based on the given person ID
    /// </summary>
    /// <param name="personUpdateRequest">Person details to update, including person ID</param>
    /// <returns>Returns PersonResponse object after updation</returns>
    Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);
  }
}
