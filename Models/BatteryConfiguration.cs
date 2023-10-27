namespace Roses.SolarAPI.Models
{
    public class BatteryConfiguration
    {
        public bool ForceCharge1 { get; set; }

        public bool ChargeFromGrid1 { get; set; }

        public TimeOnly StartDate1 { get; set; }

        public TimeOnly EndDate1 { get; set; }

        public bool ForceCharge2 { get; set; }

        public bool ChargeFromGrid2 { get; set; }

        public TimeOnly StartDate2 { get; set; }

        public TimeOnly EndDate2 { get; set; }

        public ushort MinSoC { get; set; }

        public ushort MinSoCOnGrid { get; set; }

        public string WorkMode { get; set; }

    }
}