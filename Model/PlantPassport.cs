using System;

namespace WpfApp1.Model
{
    public class PlantPassport
        /// <summary>
        /// Three fields are required for a plant passport: Plant Name in latin, Id, Sector/currentYear, country of origin always PL
    {
        public string Id { get; set; }
        public string PlantName { get; set; }
        public string Sector { get; set; }
        public DateTime DateAdded { get; set; } //exclude from pdf generation but keep track maybe?
    }

}
