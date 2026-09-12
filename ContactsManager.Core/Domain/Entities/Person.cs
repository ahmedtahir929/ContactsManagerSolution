using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
  /// <summary>
  /// Person domain model
  /// </summary>
  public class Person
  {
    [Key]
    public Guid PersonID { get; set; }
    [StringLength(40)] //nvarchar(40)
    public string? PersonName { get; set; }
    [StringLength(40)]
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [StringLength(10)]
    public string? Gender { get; set; }
    //uniqueidentifier for country, foreign key to CountryID in Countries table
    public Guid? CountryID { get; set; }
    [StringLength(200)]
    public string? Address { get; set; }
    //bit data type no need for length
    public bool ReceiveNewsLetters { get; set; }
    public string? TIN { get; set; }
    [ForeignKey("CountryID")]
    public virtual Country? Country { get; set; } //Navigation property to Country entity

    public override string ToString()
    {
      return $"Person ID: {PersonID}, Name: {PersonName}, Email: {Email}, Date of Birth: {DateOfBirth}, Gender: {Gender}, Country ID: {CountryID}, Address: {Address}, Receive News Letters: {ReceiveNewsLetters}, Country: {Country?.CountryName}";
    }
  }
}
