using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    public partial class SetWorkModeRequest : IFoxRequest
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; } = "h106__02__00";

        [JsonPropertyName("values")]
        public Values? Values { get; set; } = new Values();

        [JsonIgnore]
        public string RequestUri => SetWorkModeUri;

        public void Validate()
        {
            if (Id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(SetWorkModeRequest), "No cloud device ID is provided.");
            }

            if (string.IsNullOrWhiteSpace(Key))
            {
                throw new ArgumentOutOfRangeException(nameof(SetWorkModeRequest), "Key has been changed.");
            }

            if (!WorkModes.ALL.Any(mode => mode == Values?.Mode))
            {
                throw new ArgumentOutOfRangeException(nameof(SetWorkModeRequest), "A valid work mode has not been provided.");
            }
        }

        private const string SetWorkModeUri = "https://www.foxesscloud.com/c/v0/device/setting/set";
    }

    public partial class Values
    {
        [JsonPropertyName("h106__02__00")]
        public string? Mode { get; set; }
    }

    public class WorkModes
    {
        public const string FEED_IN = "Feedin";
        public const string SELF_USE = "SelfUse";
        public const string BACKUP = "Backup";

        public readonly static string[] ALL = new[] { FEED_IN, SELF_USE, BACKUP };
    }
}
