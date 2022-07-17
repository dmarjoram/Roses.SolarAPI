namespace Roses.SolarAPI.Extensions
{
    public static class PrimitiveExtension
    {
        public static TimeOnly ToTimeOnlyFoxESS(this short rawValue)
        {
            int hours = (int)Math.Round(rawValue / 256d, MidpointRounding.ToZero);
            int minutes = rawValue % 256;
            return new TimeOnly(hours, minutes);
        }

        public static short ToFoxESSRegister(this TimeOnly rawValue)
        {
            return (short)(rawValue.Hour * 256 + rawValue.Minute);
        }
    }
}
