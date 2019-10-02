﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Unosquare.PocoData.Tests.DataModels;

namespace Unosquare.PocoData.Tests
{
    public abstract class DbTest : IDisposable
    {
        public static string connectionString = "Data Source=(local)\SQL2017; Integrated Security=True; Initial Catalog=pocodata; MultipleActiveResultSets=True;";

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
            var query = "DROP TABLE IF EXISTS Employees";
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
