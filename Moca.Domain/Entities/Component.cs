using System;
using System.Collections.Generic;

namespace Moca.Domain.Entities
{
    public class Component
    {
        public Component()
        {
            ComponentHistories = new HashSet<ComponentHistory>();
            DefectiveComponents = new HashSet<DefectiveComponent>();
            InverseParent = new HashSet<Component>();
        }

        public int ID { get; set; }
        public int TypeId { get; set; }
        public int ModelId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public bool IsDeleted { get; set; }
        public int? ParentId { get; set; }

        public virtual ComponentModel Model { get; set; }
        public virtual Component Parent { get; set; }
        public virtual ComponentType Type { get; set; }
        public virtual ICollection<ComponentHistory> ComponentHistories { get; set; }
        public virtual ICollection<DefectiveComponent> DefectiveComponents { get; set; }
        public virtual ICollection<Component> InverseParent { get; set; }
    }
}
