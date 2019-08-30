using System;
using System.Linq;
using Unosquare.PocoData.Tests.DataModels;
using Xunit;

namespace Unosquare.PocoData.Tests
{
    public class SelectTest : DbTest
    {
        public static string query = @"
            INSERT INTO Employees VALUES('Ana Atayde', 'ana.atayde@unosquare.com', '01/01/1990', 0);
            INSERT INTO Employees VALUES('Ramiro Flores', 'ramiro.flores@unosquare.com', '01/01/1990', 0);
            INSERT INTO Employees VALUES('Marco Perez', 'marco.perez@unosquare.com', '01/01/1990', 0);
            INSERT INTO Employees VALUES('Carlos Solorzano', 'carlos.solorzano@unosquare.com', '01/01/1990', 0);
            INSERT INTO Employees VALUES('Manuel Santos', 'manuel.santos@unosquare.com', '01/01/1990', 0);
        ";
        public SelectTest() : base(query) { }

        [Fact]
        public void SelectAll()
        {
            using (var db = new SampleDb())
            {
                var employees = db.Employees.SelectAll();
                Assert.Equal(5, employees.Count());
            }
        }

        [Fact]
        public async void SelectAllAsync()
        {
            using (var db = new SampleDb())
            {
                var employee = await db.Employees.SelectAllAsync();
                Assert.Equal(5, employee.Count());
            }
        }
    }
}
