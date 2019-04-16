namespace Unosquare.PocoData.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using DataModels;
    using Unosquare.PocoData.Sql;

    class Program
    {
        static async Task Main(string[] args)
        {
            using (var db = new SampleDb())
            {
                if (await db.Definition.TableExistsAsync<Employee>())
                    await db.Definition.DropTableAsync<Employee>();

                await db.Definition.CreateTableAsync<Employee>();
                var data = GenerateEmployeeData(10000);

                var sw = new Stopwatch();
                sw.Start();

                await db.InsertManyAsync(data, false);

                // foreach (var e in data)
                //    await db.InsertAsync(e, false);

                sw.Stop();

                var employeesTable = db.TableProxy<Employee>();
                var allEmps = employeesTable.SelectAll();

                Console.WriteLine($"Wrote {data.Count} records in {sw.ElapsedMilliseconds:0.000} ms.");

                var command = db.Connection.CreateCommand() as SqlCommand;
                command.CommandText = db.Commands.SelectAllCommandText(typeof(Employee)) + $" WHERE YEAR(DateOfBirth) >= @Year";
                command.AddParameter("@Year", 1990);

                var youngEmployees = await db.SelectManyAsync<Employee>(command);


                sw.Restart();
                data = (await db.SelectAllAsync<Employee>()).ToList();

                sw.Stop();
                Console.WriteLine($"Read {data.Count} records in {sw.ElapsedMilliseconds:0.000} ms.");
            }

            Console.ReadKey(true);
        }

        private static IReadOnlyList<Employee> GenerateEmployeeData(int count)
        {
            var r = new Random();
            var result = new List<Employee>(count);
            for (var i = 0; i < count; i++)
            {
                result.Add(new Employee(
                    -1,
                    $"Name {r.Next(count)}",
                    $"email{r.Next(count)}@domain.com",
                    new DateTime(r.Next(1960, 1998), r.Next(1, 12), r.Next(1, 28)),
                    r.Next(-3, 3) < 0 ? default(int?) : r.Next(5),
                    (MaritalStatus)r.Next(0, 3),
                    null));
            }

            return result;
        }
    }
}
