using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Unosquare.PocoData.Tests.DataModels;

namespace Unosquare.PocoData.Tests
{
    public static class Utils
    {

        /**
         * This method should be changed to read the user without using the library
         */
        public static Employee? SelectEmployee(int employeeId)
        {
            var employee = new Employee
            {
                EmployeeId = employeeId
            };

            using var db = new SampleDb();
            var result = db.Employees.SelectSingle(employee);

            return result ? employee : null;
        }

        internal static async Task<bool> ExistsEmployees()
        {
            await using var con = new SqlConnection(DbTest.ConnectionString);
            con.Open();
            var command = con.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Employees'";

            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
            return (int)result > 0;
        }
    }
}
