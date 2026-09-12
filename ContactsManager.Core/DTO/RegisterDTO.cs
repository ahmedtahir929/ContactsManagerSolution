using System.ComponentModel.DataAnnotations;

namespace ContactsManager.Core.DTO
{
  public class RegisterDTO
  {
    [Required(ErrorMessage = "Name can't be blank")]
    public string? PersonName { get; set; }
    [Required(ErrorMessage = "Email can't be blank")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string? Email { get; set; }
    [Required(ErrorMessage = "Phone can't be blank")]
    [RegularExpression("^\\+?[1-9]\\d{1,14}$", ErrorMessage = "Invalid Phone Number")]
    public string? Phone { get; set; }
    [Required(ErrorMessage = "Password can't be blank")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
    [Required(ErrorMessage = "Confirm Password can't be blank")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }
  }
}
