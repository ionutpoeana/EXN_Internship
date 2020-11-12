using System;

namespace Contracts
{
    [Serializable]
    public class Bike : Vehicle
    {

        public string Color { get; set; }

        public string ModelDescription { get; set; }

        public bool IsForChildren { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Nickname { get; set; }

        public decimal Price { get; set; }
    }
}
