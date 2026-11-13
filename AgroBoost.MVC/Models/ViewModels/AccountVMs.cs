using System.ComponentModel.DataAnnotations;

namespace AgroBoost.MVC.Models.ViewModels;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required]
    [MaxLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Register As")]
    public string UserType { get; set; } = "Buyer"; // Buyer, SHG, FPG

    [MaxLength(200)]
    [Display(Name = "Organization Name")]
    public string? OrganizationName { get; set; }

    [MaxLength(20)]
    [Display(Name = "Organization Type")]
    public string? OrganizationType { get; set; } // SHG, FPG

    [MaxLength(500)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [MaxLength(100)]
    [Display(Name = "District")]
    public string? District { get; set; }

    [MaxLength(100)]
    [Display(Name = "State")]
    public string? State { get; set; }

    [MaxLength(20)]
    [Display(Name = "GST Number")]
    public string? GSTNumber { get; set; }

    [MaxLength(50)]
    [Display(Name = "Registration Number")]
    public string? RegistrationNumber { get; set; }
}

public class ProfileViewModel
{
    [Required]
    [MaxLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "Organization Name")]
    public string? OrganizationName { get; set; }

    [MaxLength(500)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [MaxLength(100)]
    [Display(Name = "District")]
    public string? District { get; set; }

    [MaxLength(100)]
    [Display(Name = "State")]
    public string? State { get; set; }

    [MaxLength(20)]
    [Display(Name = "GST Number")]
    public string? GSTNumber { get; set; }

    [MaxLength(50)]
    [Display(Name = "Registration Number")]
    public string? RegistrationNumber { get; set; }
}

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string OldPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}