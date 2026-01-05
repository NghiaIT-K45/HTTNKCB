using System.ComponentModel.DataAnnotations;

namespace HospitalTriage.Domain.Entities;

public class Department
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Khoa mặc định khi không match rule phân luồng.
    /// </summary>
    public bool IsGeneral { get; set; } = false;

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
