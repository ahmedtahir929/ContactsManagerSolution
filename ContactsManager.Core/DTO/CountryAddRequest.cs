using Entities;
using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTO
{
  /// <summary>
  /// DTO class for adding a new country
  /// </summary>
  public class CountryAddRequest
  {
    [Required]
    public string? CountryName { get; set; }

    public Country ToCountry()
    {
      return new Country { CountryName = CountryName };
    }
  }
}
