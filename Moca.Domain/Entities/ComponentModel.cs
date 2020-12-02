using System.Collections.Generic;

namespace Moca.Domain.Entities
{

    public class ComponentModel
    {
        public ComponentModel()
        {
            Components = new HashSet<Component>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public int BrandId { get; set; }
        public string Description { get; set; }

        public virtual ComponentBrand Brand { get; set; }
        public virtual ICollection<Component> Components { get; set; }
    }
}
