using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Text;
using Unosquare.PocoData.Sql;
using Xunit;

namespace Unosquare.PocoData.Tests
{
    public class SimpleTest
    {
        private string connectionString { get; set; }
        public SimpleTest()
        {


            var fileName = Path.GetTempPath() + "pocoData.sdf";
            var password = "pocodata";

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            connectionString = String.Format("DataSource = \"{0}\"", fileName, password);
            SqlCeEngine en = new SqlCeEngine(connectionString);
            en.CreateDatabase();

        }

        [Fact]
        public void Connection_test()
        {

            // var MyDB = new SqlPocoDb(connectionString);

            Assert.True(true, "");
        }

    }
}
