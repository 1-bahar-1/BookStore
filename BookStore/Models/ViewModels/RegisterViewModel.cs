using System.ComponentModel.DataAnnotations;

namespace BookStore.Models.ViewModels;
public class RegisterViewModel
{
    [Required]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "رمزها یکسان نیستند")]
    public string ConfirmPassword { get; set; }
}