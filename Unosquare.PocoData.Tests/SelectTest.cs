using Xunit;

namespace Unosquare.PocoData.Tests
{
    public class SelectTest : DbTest
    {
        [Fact]
        public void SelectAll()
        {
            using (var db = new SampleDb())
            {
                var employees = db.Employees.SelectAll();
                Assert.NotEmpty(employees);
            }
        }
    }
}
