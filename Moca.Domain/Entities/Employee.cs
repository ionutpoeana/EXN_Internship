using System;
using System.Collections.Generic;


namespace Moca.Domain.Entities
{
    public class Employee
    {
        public Employee()
        {
            ComponentHistories = new HashSet<ComponentHistory>();
            ComponentRequests = new HashSet<ComponentRequest>();
            DefectiveComponents = new HashSet<DefectiveComponent>();
        }

        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string TelephoneNumber { get; set; }
        public string Address { get; set; }
        public int RoleId { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime DateOfBirth { get; set; }

        public virtual Role Role { get; set; }
        public virtual ICollection<ComponentHistory> ComponentHistories { get; set; }
        public virtual ICollection<ComponentRequest> ComponentRequests { get; set; }
        public virtual ICollection<DefectiveComponent> DefectiveComponents { get; set; }
    }
}
