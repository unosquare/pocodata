namespace Unosquare.PocoData.Tests
{
    internal static class QueryFactory
    {
        public static string InsertTeamQuery = @"
            INSERT INTO Employees VALUES('Ana Atayde', 'ana.atayde@unosquare.com', '01/01/1990', 0, 0);
            INSERT INTO Employees VALUES('Ramiro Flores', 'ramiro.flores@unosquare.com', '01/01/1990', 0, 0);
            INSERT INTO Employees VALUES('Marco Perez', 'marco.perez@unosquare.com', '01/01/1990', 0, 0);
            INSERT INTO Employees VALUES('Carlos Solorzano', 'carlos.solorzano@unosquare.com', '01/01/1990', 0, 0);
            INSERT INTO Employees VALUES('Manuel Santos', 'manuel.santos@unosquare.com', '01/01/1990', 0, 0);
        ";

        public static string CreateEmployeeTable = @"
            CREATE TABLE Employees (
                EmployeeId INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
                FullName VARCHAR(300) NOT NULL,
                EmailAddress VARCHAR(300) NOT NULL,
                DateOfBirth DATETIME NOT NULL,
                Children INT,
                Reads INT
            );";

        public static string SelectEmployeeById(int id)
            => $@"SELECT * FROM Employees WHERE EmployeeId = {id};";
    }
}
