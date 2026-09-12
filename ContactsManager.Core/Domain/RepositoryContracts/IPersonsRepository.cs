using Entities;
using System.Linq.Expressions;

namespace RepositoryContracts
{
  /// <summary>
  /// Represents data access logic for managing Person entity
  /// </summary>
  public interface IPersonsRepository
  {
    /// <summary>
    /// Adds a new person object to the data store
    /// </summary>
    /// <param name="person">Person object to add</param>
    /// <returns>Returns the person object after adding it to the table</returns>
    Task<Person> AddPerson(Person person);

    /// <summary>
    /// Retrieves all person objects from the data store
    /// </summary>
    /// <returns>List of person object from the table</returns>
    Task<List<Person>> GetAllPersons();

    /// <summary>
    /// Returns person object for the given personID from the data store; or null if not found
    /// </summary>
    /// <param name="personID">personID to search</param>
    /// <returns>A person object or null</returns>
    Task<Person?> GetPersonByPersonID(Guid personID);


    /// <summary>
    /// Returns all the persons objects based on the given expression
    /// </summary>
    /// <param name="predicate">LINQ expression to check</param>
    /// <returns>All matching persons with given condition</returns>
    Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate);


    /// <summary>
    /// Deletes a person object based on the person ID
    /// </summary>
    /// <param name="personID">person ID to search</param>
    /// <returns>Returns true, if the deletion is successful; otherwise false</returns>
    Task<bool> DeletePersonByPersonID(Guid personID);

    /// <summary>
    /// Updates a person object in the data store (if it exists) and returns the updated person object; otherwise null
    /// </summary>
    /// <param name="person">Person object to update</param>
    /// <returns>Returns the updated person object</returns>
    Task<Person?> UpdatePerson(Person person);
  }
}