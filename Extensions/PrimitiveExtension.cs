using Roses.SolarAPI.Models.FoxCloud;
using Roses.SolarAPI.Services;

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

        public static TimeOnly ToTimeOnly(this EndTime rawValue)
        {
            if (rawValue == null || rawValue.Hour == null || rawValue.Minute == null)
            {
                throw new ArgumentNullException("Time is not provided.");
            }

            return new TimeOnly(int.Parse(rawValue.Hour!), int.Parse(rawValue.Minute!));
        }

        public static TimeOnly ToTimeOnly(this StartTime rawValue)
        {
            if (rawValue == null || rawValue.Hour == null || rawValue.Minute == null)
            {
                throw new ArgumentNullException("Time is not provided.");
            }

            return new TimeOnly(int.Parse(rawValue.Hour!), int.Parse(rawValue.Minute!));
        }

		public static TimeOnly ToStartTimeOnly(this Policy rawValue)
		{
			if (rawValue == null || rawValue.StartHour == null || rawValue.StartMinute == null)
			{
				throw new ArgumentNullException("Start time is not provided.");
			}

			return new TimeOnly((int)rawValue.StartHour!, (int)rawValue.StartMinute!);
		}

		public static TimeOnly ToEndTimeOnly(this Policy rawValue)
		{
			if (rawValue == null || rawValue.EndHour == null || rawValue.EndMinute == null)
			{
				throw new ArgumentNullException("Start time is not provided.");
			}

			return new TimeOnly((int)rawValue.EndHour!, (int)rawValue.EndMinute!);
		}

		public static short ToFoxESSRegister(this TimeOnly rawValue)
        {
            return (short)(rawValue.Hour * 256 + rawValue.Minute);
        }

        public static FoxErrorNumber ToFoxStatus(this IFoxResponse response)
        {
            if (response == null)
            {
                return FoxErrorNumber.NoResponse;
            }

            if (!Enum.IsDefined(typeof(FoxErrorNumber), response.Errno))
            {
                return FoxErrorNumber.Unknown;
            }

            return (FoxErrorNumber)response.Errno;
        }
    }
}
