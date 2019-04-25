namespace Unosquare.PocoData.Sample.DataModels
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

        public Employee(int id, string fullName, string emailAddress, DateTime dob, int? children, MaritalStatus status, object reserved)
            : this()
        {
            EmployeeId = id;
            FullName = fullName;
            EmailAddress = emailAddress;
            DateOfBirth = dob;
            Children = children;
            MaritalStatus = status;
            Reserved = reserved;
            HashCode = GetHashCode();
        }

        [Key(true)]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(300)]
        public string FullName { get; set; }

        public string EmailAddress { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int? Children { get; set; }

        public MaritalStatus MaritalStatus { get; set; }

        public object Reserved { get; set; }

        [NotMapped]
        public int HashCode { get; set; }

        public override string ToString() => $"{EmployeeId} {FullName} {EmailAddress} {MaritalStatus} {DateOfBirth} {Children}";
    }
}
