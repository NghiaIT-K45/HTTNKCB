using System.ComponentModel.DataAnnotations;

namespace HttnKcb.Api.DTOs;

public record DepartmentCreateDto(
    [Required, MaxLength(150)] string Name,
    [MaxLength(500)] string? Description,
    bool IsActive = true
);

public record DepartmentReadDto(
    int Id,
    string Name,
    string? Description,
    bool IsActive
);

public record DoctorCreateDto(
    [Required, MaxLength(150)] string FullName,
    string? LicenseNumber,
    string? Phone,
    string? Email,
    [Required] int DepartmentId
);

public record DoctorReadDto(
    int Id,
    string FullName,
    string? LicenseNumber,
    string? Phone,
    string? Email,
    int DepartmentId,
    string DepartmentName
);

public enum UserRoleDto { Admin = 1, Receptionist = 2, Nurse = 3, Doctor = 4, Manager = 5 }

public record UserCreateDto(
    [Required, MaxLength(100)] string Username,
    [Required, MaxLength(200)] string FullName,
    string? Email,
    UserRoleDto Role = UserRoleDto.Receptionist,
    bool IsActive = true
);

public record UserReadDto(
    int Id,
    string Username,
    string FullName,
    string? Email,
    UserRoleDto Role,
    bool IsActive
);
