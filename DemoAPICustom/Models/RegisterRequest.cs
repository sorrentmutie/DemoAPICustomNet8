using System.ComponentModel.DataAnnotations;

namespace DemoAPICustom.Models;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = String.Empty;

    [Required]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Password non complessa")]
    public string Password { get; set; } = String.Empty;

}
