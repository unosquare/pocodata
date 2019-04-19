# PocoData
The no-frills micro ORM for SQL Server

[![NuGet version](https://badge.fury.io/nu/PocoData.Sql.svg)](https://badge.fury.io/nu/PocoData.Sql)
:star: *Please star this project if you like it*

This library provides simplified access to data stores. Initially we only support SQL Server but the library can easily be extended to support other databases. Please not that data relations are not supported. You will need to implement those manually. Properties referencing other objects will not be persisted or mapped by this library. Only strings, enums and value types (along with their nullable counterparts) are supported.

The usage is straightforward. There are 2 separate NuGet packages: PocoData and PocoData.Sql. The packages have been spearated as sometimes you need a share data model class library across your applications. In that case you don't need to install the PocoData.Sql NuGet package in your shared class library. Only install the PocoData.Sql package in the project where you actually perform the data access.

You can create data models easily enough by adding attributes to your data model classes and their properties. For example.

```csharp
using System;
using Unosquare.PocoData.Annotations; // Make sure you import the namespace containing the data annotations.

[Table("Employees")]
public class Employee
{
	public Employee()
	{
		// a default constructor is not required but highly recommended.
	}

	[Key(true)] // Tells the mapper this property is part of a primary key and that its value is generated by the DB.
	public int EmployeeId { get; set; }

	[Required] // This string will be set as NOT NULL
	[StringLength(300)] // You can specify the string length. By default it is 255 characters.
	public string FullName { get; set; }

	public string EmailAddress { get; set; }

	public DateTime DateOfBirth { get; set; }

	public int? Children { get; set; }

	public MaritalStatus MaritalStatus { get; set; } // example of an enum (values will be stored as integers)

	public object Reserved { get; set; } // this will be ignored by the mapper.

	[NotMapped] // You can mark fields as not mapped.
	public int HashCode { get; set; }

	public override string ToString()
	{
		return $"{EmployeeId} {FullName} {EmailAddress} {MaritalStatus} {DateOfBirth} {Children}";
	}
}
```

If you can't add attributes to existing classes, you could also use the fluent API

```csharp
typeof(Employee).Table().ToTable("Employees");
typeof(Employee).Column(nameof(Employee.EmployeeId)).IsGeneratedKeyColumn();
```

Create your database container class

```csharp
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

	public EmployeesTable Employees { get; }
	
	// you could get a default table proxy for the type but this shows you
	// how to create your own table proxy with extended functionality.
	public class EmployeesTable : PocoTableProxy<Employee>
	{
		public EmployeesTable(IPocoDb db)
			: base(db, true)
		{
			// placeholder
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
```

Finally, use your database container class.

```csharp
using (var db = new SampleDb())
{
	var employees = new List<Employee>(EmployeesToGenerate);
	employees.AddRange(await db.Employees.SelectAllAsync());

	var youngEmployees = await db.Employees.GetYoungEmployeesAsync();
	foreach (var e in youngEmployees)
		e.Children = null;

	var updates = await db.Employees.UpdateManyAsync(youngEmployees);
}
```
