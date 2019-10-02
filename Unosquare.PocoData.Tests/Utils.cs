using Unosquare.PocoData.Tests.DataModels;

namespace Unosquare.PocoData.Tests
{
    public static class Utils
    {

        /**
         * This method should be changed to read the user without using the librery
         */
        public static Employee SelectEmployee(int employeeId)
        {
            bool result;
            var employee = new Employee()
            {
                EmployeeId = employeeId
            };

            using (var db = new SampleDb())
            {
                result = db.Employees.SelectSingle(employee);
            }

            return result ? employee : null;
        }
    }
}
