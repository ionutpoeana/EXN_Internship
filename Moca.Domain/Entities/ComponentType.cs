using System.Collections.Generic;

namespace Moca.Domain.Entities
{
    public class ComponentType
    {
        public ComponentType()
        {
            ComponentRequests = new HashSet<ComponentRequest>();
            Components = new HashSet<Component>();
        }

        public int ID { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ComponentRequest> ComponentRequests { get; set; }
        public virtual ICollection<Component> Components { get; set; }
    }
}
