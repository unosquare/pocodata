using System;
using System.Collections.Generic;
using System.Text;
using Unosquare.PocoData.Tests.DataModels;

namespace Unosquare.PocoData.Tests
{
    class DbTest : IDisposable
    {
        public DbTest(): this(true) { }

        public DbTest(bool initialice)
        {
            if (!initialice)
            {
                return;
            }

            using (var db = new SampleDb())
            {
                var data = new Employee()
                {
                    FullName = "José Correa",
                    EmailAddress = "jose.correa@unosquare.com",
                    Children = 0,
                    DateOfBirth = new DateTime(1995, 6, 12)
                };

                db.Employees.Insert(data, false);
            }
        }

        public void Dispose()
        {
            using (var db = new SampleDb())
            {
                var employees = db.Employees.SelectAll();
                foreach (var employee in employees)
                    db.Employees.Delete(employee);
            }
        }
    }
}
