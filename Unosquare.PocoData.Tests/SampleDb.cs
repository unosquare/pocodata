﻿namespace Unosquare.PocoData.Tests
{
    using DataModels;
    using Sql;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    public class SampleDb : SqlPocoDb
    {
        public SampleDb()
            : base("Server=(local)\\SQL2017;User ID=sa;Password=Password12!;Initial Catalog=pocodata;")
        {
            Employees = new EmployeesTable(this);
        }

        public PocoTableProxy<Employee> EmployeesDefault => TableProxy<Employee>();

        public EmployeesTable Employees { get; }

        public class EmployeesTable : PocoTableProxy<Employee>
        {
            public EmployeesTable(IPocoDb db)
                : base(db, true)
            {
                // placeholder
            }

            public Task<IEnumerable<Employee>> GetYoungEmployeesAsync()
            {
                var command = PocoDb.Connection.CreateCommand() as SqlCommand;
                command.CommandText = PocoDb.Commands.SelectAllCommandText(typeof(Employee)) + $" WHERE YEAR(DateOfBirth) >= @Year";
                command.AddParameter("@Year", 1990);

                return SelectManyAsync(command);
            }
        }
    }
}
