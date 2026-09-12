using Entities;
using ServiceContracts.Enums;

namespace ServiceContracts.DTO
{
  /// <summary>
  /// Represents DTO class that is used as return type of most methods of Persons Service
  /// </summary>
  public class PersonResponse
  {
    public Guid PersonID { get; set; }
    public string? PersonName { get; set; }
    public string? Email { get; set; }
    public double? Age { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public Guid? CountryID { get; set; }
    public string? Country { get; set; }
    public string? Address { get; set; }
    public bool ReceiveNewsLetters { get; set; }
    public string? TIN { get; set; }
    public string? MaskedTIN => string.IsNullOrEmpty(TIN) ?
        null : new string('*', TIN.Length - 4) + TIN.Substring(TIN.Length - 4);

    /// <summary>
    /// Compares current object data with the parameter object
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>True or False, indicating whether all person details are
    /// matched with the specified parameter object
    /// </returns>
    public override bool Equals(object? obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      PersonResponse person = (PersonResponse)obj;
      return ComparePersonDetails(person);
    }

    public override string ToString()
    {
      return $"Person ID: {PersonID}, Person Name: {PersonName}, Email: {Email}, Gender: {Gender}, Date of Birth: {DateOfBirth?.ToString("dd MMM yyyy")}, Country ID: {CountryID}, Country: {Country}, Receive News Letter: {ReceiveNewsLetters}, TIN: {TIN}";
    }

    private bool ComparePersonDetails(PersonResponse other)
    {
      return
          PersonID == other.PersonID &&
          PersonName == other.PersonName &&
          Email == other.Email &&
          DateOfBirth == other.DateOfBirth &&
          Gender == other.Gender &&
          CountryID == other.CountryID &&
          Address == other.Address &&
          ReceiveNewsLetters == other.ReceiveNewsLetters &&
          TIN == other.TIN;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public PersonUpdateRequest ToPersonUpdateRequest()
    {
      return new PersonUpdateRequest()
      {
        PersonID = PersonID,
        PersonName = PersonName,
        Email = Email,
        Address = Address,
        Gender = Enum.Parse<GenderOptions>(Gender ?? string.Empty, true),
        DateOfBirth = DateOfBirth,
        CountryID = CountryID,
        ReceiveNewsLetters = ReceiveNewsLetters,
        TIN = TIN
      };
    }
  }

  public static class PersonExtensions
  {
    /// <summary>
    /// An extension method to convert an object of Person class into PersonResponse class
    /// </summary>
    /// <param name="person">Returns the converted PersonResponse object</param>
    /// <returns></returns>
    public static PersonResponse ToPersonResponse(this Person person)
    {
      // person => PersonResponse
      return new PersonResponse()
      {
        PersonID = person.PersonID,
        PersonName = person.PersonName,
        Email = person.Email,
        DateOfBirth = person.DateOfBirth,
        Gender = person.Gender,
        CountryID = person.CountryID,
        ReceiveNewsLetters = person.ReceiveNewsLetters,
        Address = person.Address,
        Age =
          (person.DateOfBirth != null) ?
          Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25) : null,
        TIN = person.TIN,
        Country = person.Country?.CountryName
      };
    }
  }
}