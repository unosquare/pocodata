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
                if (db.Employees.TableExists)
                    db.Employees.DropTable();

                db.Employees.CreateTable();

                var data = GenerateEmployeeData(10000);
                var sw = new Stopwatch();

                sw.Restart();
                await db.Employees.InsertManyAsync(data, false);
                sw.Stop();
                Console.WriteLine($"Wrote {db.Employees.CountAll()} records in {sw.ElapsedMilliseconds:0.000} ms.");

                sw.Restart();
                var youngEmployees = await db.Employees.GetYoungEmployeesAsync();
                sw.Stop();
                Console.WriteLine($"Selected {youngEmployees.Count()} records in {sw.ElapsedMilliseconds:0.000} ms.");

                foreach (var e in youngEmployees)
                    e.Children = 0;

                sw.Restart();
                var updates = await db.Employees.UpdateManyAsync(youngEmployees);
                sw.Stop();
                Console.WriteLine($"Updated {updates} records in {sw.ElapsedMilliseconds:0.000} ms.");

                youngEmployees = await db.Employees.GetYoungEmployeesAsync();
                var asserted = youngEmployees.Where(e => e.Children.HasValue && e.Children.Value != 0).ToList();

                sw.Restart();
                data = (await db.SelectAllAsync<Employee>()).ToList();
                sw.Stop();
                Console.WriteLine($"Selected {data.Count} records in {sw.ElapsedMilliseconds:0.000} ms.");

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
