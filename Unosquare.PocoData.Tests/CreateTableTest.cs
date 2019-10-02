namespace Unosquare.PocoData.Tests
{
    using Xunit;
    using FluentAssertions;
    using Annotations;
    using System;
    using System.Data.SqlClient;

    public class CreateTableTest : IDisposable
    {
        [Fact(DisplayName ="Key column is required")]
        public async void CreateTableWithoutKey()
        {
            using (var db = new SampleDb())
            {
               await Assert.ThrowsAsync<SqlException>
                    (() => db.Definition.CreateTableAsync<ThingWithoutKey>());
            }
        }

        [Fact(DisplayName="Only IntK Keys are allowed")]
        public async void CreateTableStringKey()
        {
            using (var db = new SampleDb())
            {
                await Assert.ThrowsAsync<SqlException>
                     (() => db.Definition.CreateTableAsync<ThingStringKey>());
            }
        }

        [Fact]
        public async void CreateTable()
        {
            using (var db = new SampleDb())
            {
                var result = await db.Definition.CreateTableAsync<Thing>();
                result.Should<int>().Be(-1);
            }
        }

        public void Dispose()
        {
            var query = "DROP TABLE IF EXISTS Thing;" +
                "DROP TABLE IF EXISTS Employee;";
            using (SqlConnection con = new SqlConnection(DbTest.connectionString))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    internal class ThingWithoutKey
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }

    internal class ThingStringKey
    {
        [Key(true)]
        public string Name { get; set; }

        public string Description { get; set; }
    }

    internal class Thing
    {
        [Key(true)]
        public int Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
