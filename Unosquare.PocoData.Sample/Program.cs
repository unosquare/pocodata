namespace Unosquare.PocoData.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using DataModels;
    using Unosquare.PocoData.Sql;

    class Program
    {
        static async Task Main(string[] args)
        {
            using (var db = await SqlPocoDb.OpenLocalAsync("litedata"))
            {
                if (await db.Definition.TableExistsAsync<Employee>())
                    await db.Definition.DropTableAsync<Employee>();

                await db.Definition.CreateTableAsync<Employee>();
                var data = GenerateEmployeeData(10000);

                var sw = new Stopwatch();
                sw.Start();

                foreach (var e in data)
                    await db.InsertAsync(e, false);

                sw.Stop();

                Console.WriteLine($"Wrote {data.Count} records in {sw.ElapsedMilliseconds:0.000} ms.");

                sw.Restart();
                data = await db.SelectAsync<Employee>();

                sw.Stop();
                Console.WriteLine($"Read {data.Count} records in {sw.ElapsedMilliseconds:0.000} ms.");
            }

            Console.ReadKey(true);
        }

        private static IReadOnlyList<Employee> EmployeeSeedData
        {
            get
            {
                return new List<Employee>()
                {
                    new Employee(101, "Employee 101", "empl@domain.com", new DateTime(1978,2,4), null, MaritalStatus.Single, "hey there"),
                    new Employee(102, "Employee 102", "empl@domain.com", new DateTime(1978,3,4), 1, MaritalStatus.None, "hey there1"),
                    new Employee(103, "Employee 103", "empl@domain.com", new DateTime(1978,4,4), null, MaritalStatus.Divorced, "hey there"),
                    new Employee(104, "Employee 104", "empl@domain.com", new DateTime(1978,5,4), 3, MaritalStatus.Single, "hey there"),
                    new Employee(105, "Employee 105", "empl@domain.com", new DateTime(1978,6,4), null, MaritalStatus.Married, "hey there"),
                    new Employee(106, "Employee 106", "empl@domain.com", new DateTime(1978,7,4), 5, MaritalStatus.Married, "hey there"),
                    new Employee(107, "Employee 107", "empl@domain.com", new DateTime(1978,8,4), null, MaritalStatus.Single, "hey there"),
                    new Employee(108, "Employee 108", "empl@domain.com", new DateTime(1978,9,4), 7, MaritalStatus.Widowed, "hey there"),
                    new Employee(109, "Employee 109", "empl@domain.com", new DateTime(1978,10,4), null, MaritalStatus.Single, "hey there"),
                    new Employee(110, "Employee 110", "empl@domain.com", new DateTime(1978,11,4), 9, MaritalStatus.Single, "hey there"),
                };
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
