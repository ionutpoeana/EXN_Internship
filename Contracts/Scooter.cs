using System;

namespace Contracts
{
    [Serializable]
    public class Scooter : Vehicle
    {
        public string Color { get; set; }
        public bool IsElectric { get; set; }
        public int SpeedLimit { get; set; }
        public decimal Price { get; set; }
    }
}
