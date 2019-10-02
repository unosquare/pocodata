using Unosquare.PocoData.Tests.DataModels;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace Unosquare.PocoData.Tests
{
    public class CountAllTest : DbTest
    {

        public CountAllTest () 
            : base(QueryFactory.CreateEmployeeTable + QueryFactory.InsertTeamQuery)
        { }

        [Fact]
        public void CountAll()
        {
            using (var db = new SampleDb())
            {
                var employees = db.CountAll(typeof(Employee));
                employees.Should<int>().BeGreaterThan(0);
            }
        }

        [Fact]
        public async void CountAllAsync()
        {
            using (var db = new SampleDb())
            {
                var employees = await db.CountAllAsync(typeof(Employee));
                employees.Should<int>().BeGreaterThan(0);
            }
        }
    }
}
