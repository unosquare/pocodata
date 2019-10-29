using Unosquare.PocoData.Tests.DataModels;
using Xunit;

namespace Unosquare.PocoData.Tests
{
    public class DeleteTest : DbTest
    {
        public DeleteTest(): 
            base(QueryFactory.CreateEmployeeTable + QueryFactory.InsertTeamQuery)
        {
        }

        [Fact]
        public void Delete()
        {
            using (var db = new SampleDb())
            {
                var employee = new Employee()
                {
                    EmployeeId = 1
                };
                
                db.Employees.Delete(employee);
            }
            
            var newData =  Utils.SelectEmployee(1);
            Assert.Null(newData);
        }

        [Fact]
        public async void DeleteAsync()
        {
            using (var db = new SampleDb())
            {
                var employee = new Employee()
                {
                    EmployeeId = 1
                };

                await db.Employees.DeleteAsync(employee);

            }

            var newData = Utils.SelectEmployee(1);
            Assert.Null(newData);
        }

    }
}
