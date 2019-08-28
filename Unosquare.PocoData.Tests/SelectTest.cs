using System;
using System.Linq;
using Unosquare.PocoData.Tests.DataModels;
using Xunit;

namespace Unosquare.PocoData.Tests
{
    public class SelectTest : DbTest
    {
        public SelectTest() : base(false)
        {
            using (var db = new SampleDb())
            {
                var data = new Employee[] {
                    new Employee(1, "Marco Perez","marco.perez@unosquare.com", DateTime.Today, 0, null),
                    new Employee(2, "Carlos Solorzano","carlos.solorzano@unosquare.com", DateTime.Today, 0, null),
                    new Employee(3, "Ana Atayde","ana.atayde@unosquare.com", DateTime.Today, 0, null),
                    new Employee(4, "Ramiro Flores","ramiro.flores@unosquare.com", DateTime.Today, 0, null),
                    new Employee(5, "Manuel Santos","manuel.santos@unosquare.com", DateTime.Today, 0, null),
                };

                db.Employees.InsertMany(data, false);
            }
        }

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
