namespace Unosquare.PocoData.Sample
{
    using Annotations;
    using DataModels;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Unosquare.PocoData.Sql;

    public class SampleDb : SqlPocoDb
    {
        public SampleDb()
            : base($"Data Source=.; Integrated Security=True; Initial Catalog=pocodata; MultipleActiveResultSets=True;")
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

            }

            public async Task<IEnumerable<Employee>> GetYoungEmployeesAsync()
            {
                var command = PocoDb.Connection.CreateCommand() as SqlCommand;
                command.CommandText = PocoDb.Commands.SelectAllCommandText(typeof(Employee)) + $" WHERE YEAR(DateOfBirth) >= @Year";
                command.AddParameter("@Year", 1990);

                return await SelectManyAsync(command);
            }
        }
    }
}
