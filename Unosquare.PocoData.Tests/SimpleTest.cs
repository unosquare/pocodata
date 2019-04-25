namespace Unosquare.PocoData.Tests
{
    using Xunit;

    public class SimpleTest
    {
        [Fact]
        public void ValidConnection()
        {
            using (var db = new SampleDb())
            {
                Assert.True(db.Connection?.State == System.Data.ConnectionState.Open);
            }
        }

    }
}
