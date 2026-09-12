namespace ServiceContracts.PersonsServiceContracts
{
  /// <summary>
  /// Represents business logic for manipulating Person entity
  /// </summary>
  public interface IPersonsDeleterService
  {
    /// <summary>
    /// Deletes person based on the given ID
    /// </summary>
    /// <param name="PersonID">PersonID to delete</param>
    /// <returns>Returns true if the deletion is successful; otherwise false</returns>
    Task<bool> DeletePerson(Guid? PersonID);
  }
}
