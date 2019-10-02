using System;
using Unosquare.PocoData.Tests.DataModels;
using Xunit;
using FluentAssertions;
using System.Data.SqlClient;

namespace Unosquare.PocoData.Tests
{
    public class DropTableTest : DbTest
    {

        public DropTableTest()
            : base (QueryFactory.CreateEmployeeTable)
        { }

        [Fact]
        public async void DropTable()
        {
            using (var db = new SampleDb())
            {
                await db.Definition.DropTableAsync<Employee>();
                var result = await Utils.ExistsEmployees();

                result.Should<bool>().Be(false);
            }
        }

        [Fact(DisplayName ="Cannot drop table bc it does not exist")]
        public async void DropInvalidTable()
        {
            using (var db = new SampleDb())
            {
                await Assert.ThrowsAsync<SqlException>(
                    () => db.Definition.DropTableAsync<Thing>());
            }
        }
    }
}
