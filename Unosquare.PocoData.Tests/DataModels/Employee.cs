namespace Unosquare.PocoData.Tests.DataModels
{
    using System;
    using Annotations;

    [Table("Employees")]
    public class Employee
    {
        public Employee()
        {
            // placeholder
        }

        public Employee(int id, string fullName, string emailAddress, DateTime dob, int? children, object reserved)
            : this()
        {
            EmployeeId = id;
            FullName = fullName;
            EmailAddress = emailAddress;
            DateOfBirth = dob;
            Children = children;
            Reserved = reserved;
            HashCode = this.GetHashCode();
        }

        [Key(true)]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(300)]
        public string FullName { get; set; }

        public string EmailAddress { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int? Children { get; set; }

        public object Reserved { get; set; }

        [NotMapped]
        public int HashCode { get; set; }

        public override string ToString() => $"{EmployeeId} {FullName} {EmailAddress} {DateOfBirth} {Children}";
    }
}
