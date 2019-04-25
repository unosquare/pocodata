using System;
using Xunit;

namespace Unosquare.PocoData.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var result = false;
            Assert.False(result, "1 should not be prime");
        }

        [Fact]
        public void Test2()
        {
            Assert.True(true, "msg");
        }
    }
}
