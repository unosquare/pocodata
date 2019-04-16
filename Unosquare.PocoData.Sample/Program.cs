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
            const int EmployeesToGenerate = 100;
            const bool Reset = false;

            using (var db = new SampleDb())
            {
                if (Reset && db.Employees.TableExists)
                    db.Employees.DropTable();

                if (!db.Employees.TableExists)
                    db.Employees.CreateTable();

                if (db.Employees.CountAll() <= 0)
                    await db.Employees.InsertManyAsync(GenerateEmployeeData(EmployeesToGenerate), false);

                var employees = new List<Employee>(EmployeesToGenerate);
                employees.AddRange(await db.Employees.SelectAllAsync());

                var youngEmployees = await db.Employees.GetYoungEmployeesAsync();
                foreach (var e in youngEmployees)
                    e.Children = null;

                var updates = await db.Employees.UpdateManyAsync(youngEmployees);

                var updatedYoungEmployees = await db.Employees.GetYoungEmployeesAsync();
                var failedUpdates = updatedYoungEmployees.Where(e => e.Children.HasValue).ToList();
                Console.WriteLine($"Failed Updates: {failedUpdates.Count}");
            }
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
