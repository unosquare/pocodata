namespace Unosquare.PocoData.Sample
{
    using DataModels;
    using Unosquare.PocoData.Sql;

    public class SampleDb : SqlPocoDb
    {
        public SampleDb()
            : base($"Data Source=.; Integrated Security=True; Initial Catalog=pocodata; MultipleActiveResultSets=True;")
        {

        }

        public PocoTableProxy<Employee> Employees => TableProxy<Employee>();
    }
}
