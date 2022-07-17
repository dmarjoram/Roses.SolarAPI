namespace Roses.SolarAPI.Extensions
{
    public static class PrimitiveExtension
    {
        public static TimeOnly ToTimeOnlyFoxESS(this ushort rawValue)
        {
            int hours = (int)Math.Round(rawValue / 256d, MidpointRounding.ToZero);
            int minutes = rawValue - hours * 256;
            return new TimeOnly(hours, minutes);
        }
    }
}
