using AutoFixture;
using FluentAssertions;
using System.Data.SqlClient;
using Unosquare.PocoData.Tests.DataModels;
using Xunit;

namespace Unosquare.PocoData.Tests
{
    public class UpdateTest : DbTest
    {
        public UpdateTest() 
            : base(QueryFactory.CreateEmployeeTable + QueryFactory.InsertTeamQuery) 
        {
        }

        [Fact]
        public void Update() 
        {
            var fixture= new Fixture();

            var employee = fixture.Build<Employee>()
                                        .With(x => x.EmployeeId, 1)
                                        .Create();

            using (var db = new SampleDb())
            {
                db.Employees.Update(employee);
            }

            var newValue = Utils.SelectEmployee(1);

            newValue.FullName.Should().Be(employee.FullName);
            newValue.EmailAddress.Should().Be(employee.EmailAddress);
            newValue.DateOfBirth.Should().BeCloseTo(employee.DateOfBirth);
            newValue.Children.Should().Be(employee.Children);
            newValue.Reads.Should().Be(employee.Reads);
        }

        [Fact]
        public async void UpdateAsync()
        {
            var fixture = new Fixture();

            var employee = fixture.Build<Employee>()
                                        .With(x => x.EmployeeId, 1)
                                        .Create();

            using (var db = new SampleDb())
            {
                await db.Employees.UpdateAsync(employee);
            }

            var newValue = Utils.SelectEmployee(1);

            newValue.FullName.Should().Be(employee.FullName);
            newValue.EmailAddress.Should().Be(employee.EmailAddress);
            newValue.DateOfBirth.Should().BeCloseTo(employee.DateOfBirth);
            newValue.Children.Should().Be(employee.Children);
            newValue.Reads.Should().Be(employee.Reads);
        }
    }
}
