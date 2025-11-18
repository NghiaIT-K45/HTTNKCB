using System.ComponentModel.DataAnnotations;

namespace HttnKcb.Api.Models;

public enum UserRole
{
    Admin = 1,
    Receptionist = 2,
    Nurse = 3,
    Doctor = 4,
    Manager = 5
}

public class AppUser
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress, MaxLength(200)]
    public string? Email { get; set; }

    public UserRole Role { get; set; } = UserRole.Receptionist;

    public bool IsActive { get; set; } = true;
}
