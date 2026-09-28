using System.ComponentModel.DataAnnotations;

namespace TableFlow.Api.Contracts;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}