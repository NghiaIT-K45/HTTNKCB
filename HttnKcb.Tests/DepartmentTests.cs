using System;                
using System.Linq;            
using FluentAssertions;
using HttnKcb.Api.Data;
using HttnKcb.Api.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HttnKcb.Tests
{
  public class DepartmentTests
  {
    private static HospitalDbContext GetDb()
    {
      var options = new DbContextOptionsBuilder<HospitalDbContext>()
          // Guid.NewGuid() cần using System;
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;

      var db = new HospitalDbContext(options);
      return db;
    }

    [Fact]
    public void CanCreateDepartment()
    {
      // Arrange
      var db = GetDb();
      var dep = new Department { Name = "Khoa Nội", Description = "Nội tổng quát" };

      // Act
      db.Departments.Add(dep);
      db.SaveChanges();

      // Assert
      // Count() cần using System.Linq;
      db.Departments.Count().Should().Be(1);
      db.Departments.First().Name.Should().Be("Khoa Nội");
    }

    [Fact]
    public void DoctorMustReferenceDepartment()
    {
      // Arrange
      var db = GetDb();
      var dep = new Department { Name = "Khoa Nhi" };
      db.Departments.Add(dep);
      db.SaveChanges();

      // Act
      var doctor = new Doctor { FullName = "BS. A", DepartmentId = dep.Id };
      db.Doctors.Add(doctor);
      db.SaveChanges();

      // Assert
      var loaded = db.Doctors.Include(d => d.Department).First();
      loaded.Department!.Name.Should().Be("Khoa Nhi");
    }
  }
}
