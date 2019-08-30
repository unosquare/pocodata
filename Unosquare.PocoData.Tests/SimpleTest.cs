namespace Unosquare.PocoData.Tests
{
    using System;
    using System.Linq;
    using Unosquare.PocoData.Tests.DataModels;
    using Xunit;

    public class SimpleTest : DbTest
    {
        public static string query = @"
            INSERT INTO Employees VALUES('Ana Atayde', 'ana.atayde@unosquare.com', '01/01/1990', 0);
            INSERT INTO Employees VALUES('Ramiro Flores', 'ramiro.flores@unosquare.com', '01/01/1990', 0);
            INSERT INTO Employees VALUES('Marco Perez', 'marco.perez@unosquare.com', '01/01/1990', 0);
            INSERT INTO Employees VALUES('Carlos Solorzano', 'carlos.solorzano@unosquare.com', '01/01/1990', 0);
            INSERT INTO Employees VALUES('Manuel Santos', 'manuel.santos@unosquare.com', '01/01/1990', 0);
        ";
        public SimpleTest(): base(query) { }

        [Fact]
        public void ValidConnection()
        {
            using (var db = new SampleDb())
            {
                Assert.True(db.Connection?.State == System.Data.ConnectionState.Open);
            }
        }

        [Fact]
        public void ValidInsert()
        {
            using (var db = new SampleDb())
            {
                var data = new Employee()
                {
                    FullName = "José Correa",
                    EmailAddress = "jose.correa@unosquare.com",
                    Children = 0,
                    DateOfBirth = new DateTime(1995, 6, 12)
                };

                var result = db.Employees.Insert(data, false);
                Assert.True(data.EmployeeId != 0);
            }
        }

        [Fact]
        public void ValidSelect()
        {
            using (var db = new SampleDb())
            {
                var data = db.Employees.SelectAll();
                Assert.NotEmpty(data);
            }
        }

        [Fact]
        public void ValidUpdate()
        {
            using (var db = new SampleDb())
            {
                var toUpdate = db.Employees.SelectAll().FirstOrDefault();
                var newValue = new Random().Next(100);
                toUpdate.Children = newValue;

                db.Update(toUpdate);
                Assert.True(toUpdate.Children == newValue);
            }
        }

    }
}
