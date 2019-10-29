using System.Linq;
using Unosquare.PocoData.Tests.DataModels;
using Xunit;

namespace Unosquare.PocoData.Tests
{
    public class SelectTest : DbTest
    {
        public SelectTest() : 
            base(QueryFactory.CreateEmployeeTable + QueryFactory.InsertTeamQuery) { }

        [Fact]
        public void SelectAll()
        {
            using var db = new SampleDb();
            var employees = db.Employees.SelectAll();
            Assert.Equal(5, employees.Count());
        }

        [Fact]
        public async void SelectAllAsync()
        {
            using var db = new SampleDb();
            var employee = await db.Employees.SelectAllAsync();
            Assert.Equal(5, employee.Count());
        }

        [Fact]
        public void SelectSingle()
        {
            using var db = new SampleDb();
            var employee = new Employee()
            {
                EmployeeId = 1
            };

            db.SelectSingle(employee);

            Assert.NotEqual(0, employee.FullName.Length);
        }

        [Fact]
        public async void SelectSingleAsync()
        {
            using var db = new SampleDb();
            var employee = new Employee()
            {
                EmployeeId = 1
            };
            var employees = await db.SelectSingleAsync(employee);

            Assert.NotEqual(0, employee.FullName.Length);
        }
    }
}