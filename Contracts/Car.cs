using System;

namespace Contracts
{
    [Serializable]
    public class Car : Vehicle
    {
        public string Vin { get; set; }
        public string Model { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int Mileage { get; set; }
        public decimal Price { get; set; }
    }
}
