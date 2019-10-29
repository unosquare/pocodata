using Unosquare.PocoData.Tests.DataModels;
using Xunit;
using FluentAssertions;

namespace Unosquare.PocoData.Tests
{
    public class ExistsTableTest : DbTest
    {
        public ExistsTableTest ()
            : base(QueryFactory.CreateEmployeeTable)
        { }

        [Fact]
        public async void ExistValid()
        {
            using var db = new SampleDb();
            var result = await db.Definition.TableExistsAsync<Employee>();
            result.Should<bool>().Be(true);
        }

        [Fact]
        public async void ExistInvalid()
        {
            using var db = new SampleDb();
            var result = await db.Definition.TableExistsAsync<Thing>();
            result.Should<bool>().Be(false);
        }
    }
}
