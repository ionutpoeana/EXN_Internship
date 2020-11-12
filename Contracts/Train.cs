using System;

namespace Contracts
{
    [Serializable]
    public class Train : Vehicle
    {
        public int TotalNumberOfPassengers { get; set; }
        public int MaxSpeedKMPerHour { get; set; }
        public string Country { get; set; }
        public bool Decommissioned { get; set; }
    }

}
