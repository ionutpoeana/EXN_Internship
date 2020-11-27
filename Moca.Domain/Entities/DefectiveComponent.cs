using System;

namespace Moca.Domain.Entities
{
    public class DefectiveComponent
    {
        public int ID { get; set; }
        public int ComponentId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Observations { get; set; }
        public int ReparationStatus { get; set; }

        public virtual Component Component { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
