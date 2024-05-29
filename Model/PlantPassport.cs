using System;

namespace WpfApp1.Model
{
    public class PlantPassport
    {
        public string Id { get; set; }
        public string PlantName { get; set; }
        public string Sector { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }

    public class BasketItem
    {
        public string PlantName { get; set; }
        public int Count { get; set; }
    }

}
