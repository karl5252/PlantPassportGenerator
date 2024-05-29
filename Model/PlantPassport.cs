using System;

namespace WpfApp1.Model
{
    public class PlantPassport
    {
        public int Id { get; set; }
        public string PlantName { get; set; }
        public string Species { get; set; }
        public string Origin { get; set; }
        public DateTime DateAdded { get; set; }
    }

}
