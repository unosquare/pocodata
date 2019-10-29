using Unosquare.PocoData.Tests.DataModels;
using AutoFixture;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace Unosquare.PocoData.Tests
{
    public class InsertTest : DbTest
    {
        public InsertTest() : base(QueryFactory.CreateEmployeeTable)
        {
        }

        [Fact]
        public void Insert()
        {
            var fixture = new Fixture();
            using var db = new SampleDb();
            var employee = fixture.Create<Employee>();
            db.Employees.Insert(employee, false);

            Assert.NotEqual(0, employee.EmployeeId);
        }

        [Fact]
        public async void InsertAsync()
        {
            var fixture = new Fixture();
            using var db = new SampleDb();
            var employee = fixture.Create<Employee>();
            await db.Employees.InsertAsync(employee, false);

            Assert.NotEqual(0, employee.EmployeeId);
        }

        [Fact(Skip = "Missing add trigger to generate a change and reload info")]
        public void InsertUpdate()
        {
            var fixture = new Fixture();
            using var db = new SampleDb();
            var employee = fixture.Build<Employee>()
.Without(x => x.Reads)
.Create();

            db.Employees.Insert(employee, true);
            employee.Reads.Should().BeGreaterThan(0);
        }

        [Fact(Skip = "Missing add trigger to generate a change and reload info")]
        public async void InsertUpdateAsync()
        {
            var fixture = new Fixture();
            using var db = new SampleDb();
            var employee = fixture.Build<Employee>()
.Without(x => x.Reads)
.Create();

            await db.Employees.InsertAsync(employee, true);
            employee.Reads.Should().BeGreaterThan(0);
        }

        [Fact]
        public void InsertMany()
        {
            var fixture = new Fixture();
            using var db = new SampleDb();
            var employees = fixture.CreateMany<Employee>(5);
            db.Employees.InsertMany(employees, false);

            employees.Count().Should().Be(5);
        }

        [Fact]
        public async void InsertManyAsync()
        {
            var fixture = new Fixture();
            using var db = new SampleDb();
            var employees = fixture.CreateMany<Employee>(5);
            await db.Employees.InsertManyAsync(employees, false);

            employees.Count().Should().Be(5);
        }

    }
}
