using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Unosquare.PocoData.Tests.DataModels;

namespace Unosquare.PocoData.Tests
{
    public abstract class DbTest : IDisposable
    {
        public static string connectionString = "Data Source=GDLA-LT-181228A\\SQLEXPRESS; Integrated Security=True; Initial Catalog=pocodatatest; MultipleActiveResultSets=True;";

        public DbTest(string query)
        {
            if (query.Equals(string.Empty))
            {
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    command.ExecuteNonQuery();
                }
            }

        }

        public void Dispose()
        {
            var query = "DROP TABLE Employees";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
