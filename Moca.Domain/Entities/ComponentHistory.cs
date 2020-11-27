using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moca.Domain.Entities
{
    public class ComponentHistory
    {
        public int ID { get; set; }
        public int EmployeeId { get; set; }
        public int ComponentId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysInUse { get; set; }
        public string Observations { get; set; }
        public virtual Component Component { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
