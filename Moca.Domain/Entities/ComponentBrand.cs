using System.Collections.Generic;

namespace Moca.Domain.Entities
{
    public class ComponentBrand
    {
        public ComponentBrand()
        {
            ComponentModels = new HashSet<ComponentModel>();
        }

        public int ID { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ComponentModel> ComponentModels { get; set; }
    }
}
