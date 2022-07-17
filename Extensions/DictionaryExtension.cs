using System.Globalization;

namespace Roses.SolarAPI.Extensions
{
    public static class DictionaryExtension
    {
        public static Dictionary<DateTime, long> ToDateTimeKey(this Dictionary<string, long> source)
        {
            return source.ToDictionary(kvp => {
                var date = DateTime.ParseExact(kvp.Key, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.AssumeLocal);
                return date;
            }, kvp => kvp.Value);
        }
    }
}
