using System;

namespace Moca.Domain.Entities
{
    public class ComponentRequest
    {
        public int ID { get; set; }
        public int TypeId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime RequestDate { get; set; }
        public int Status { get; set; }
        public string Observations { get; set; }
        public decimal EstimatedPrice { get; set; }


        public virtual Employee Employee { get; set; }
        public virtual ComponentType Type { get; set; }
    }
}
