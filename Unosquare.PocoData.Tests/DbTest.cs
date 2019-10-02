using System;
using System.Data.SqlClient;

namespace Unosquare.PocoData.Tests
{
    public abstract class DbTest : IDisposable
    {
        public static string ConnectionString = "Data Source=(local)\\SQL2017; Integrated Security=True; Initial Catalog=pocodata; MultipleActiveResultSets=True;";

        protected DbTest(string query)
        {
            if (query.Equals(string.Empty))
            {
                return;
            }

            using var con = new SqlConnection(ConnectionString);
            con.Open();
            using var command = new SqlCommand(query, con);
            command.ExecuteNonQuery();

        }

        public void Dispose()
        {
            const string query = "DROP TABLE IF EXISTS Employees";
            using var con = new SqlConnection(ConnectionString);
            con.Open();
            using var command = new SqlCommand(query, con);
            command.ExecuteNonQuery();
        }
    }
}
