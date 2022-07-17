namespace Roses.SolarAPI.Models
{
    public class SolarProduction
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int Hours { get; set; }

        public double Kwh { get; set; }
        
    }
}