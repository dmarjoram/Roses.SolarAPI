using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    public partial class SetWorkModeRequest : IFoxRequest
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("values")]
        public object? Values { get; set; } 

        [JsonIgnore]
        public string RequestUri => SetWorkModeUri;

		[JsonIgnore]
		public bool GetRequest => false;

		public SetWorkModeRequest(string spaKey, string workMode)
        {
            if (string.IsNullOrWhiteSpace(spaKey))
            {
                throw new ArgumentOutOfRangeException(nameof(spaKey), "Please provide a spaKey.");
            }

            if (!WorkModeSpaKeys.ALL.Any(key => key == spaKey))
            {
                throw new ArgumentOutOfRangeException(nameof(spaKey), "A valid SPA key has not been provided.");
            }

            if (!WorkModes.ALL.Any(mode => mode == workMode))
            {
                throw new ArgumentOutOfRangeException(nameof(workMode), "A valid work mode has not been provided.");
            }

            Key = $"{spaKey.Trim()}__02__00";

            switch (spaKey)
            {
                case WorkModeSpaKeys.H106:
                    Values = new Values108() { Mode = workMode };
                    break;
                case WorkModeSpaKeys.H108:
                    Values = new Values108() { Mode = workMode };
                    break;
                case WorkModeSpaKeys.H111:
                    Values = new Values111() { Mode = workMode };
                    break;
                case WorkModeSpaKeys.H112:
                    Values = new Values112() { Mode = workMode };
                    break;
                default:
                    Values = new ValuesOpWorkMode() { Mode = workMode };
                    Key = $"{spaKey.Trim()}";
                    break;
            }
        }

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

            if (!WorkModes.ALL.Any(mode => mode == (Values as IValues)?.Mode))
            {
                throw new ArgumentOutOfRangeException(nameof(SetWorkModeRequest), "A valid work mode has not been provided.");
            }
        }

        private const string SetWorkModeUri = "https://www.foxesscloud.com/c/v0/device/setting/set";

        private interface IValues
        {
            string? Mode { get; set; }
        }

        private class Values106 : IValues
        {
            [JsonPropertyName("h106__02__00")]
            public string? Mode { get; set; }
        }

        private class Values108 : IValues
        {
            [JsonPropertyName("h108__02__00")]
            public string? Mode { get; set; }
        }

        private class Values111 : IValues
        {
            [JsonPropertyName("h111__02__00")]
            public string? Mode { get; set; }
        }

        private class Values112 : IValues
        {
            [JsonPropertyName("h112__02__00")]
            public string? Mode { get; set; }
        }

        private class Values115 : IValues
        {
            [JsonPropertyName("h115__02__00")]
            public string? Mode { get; set; }
        }

        private class ValuesOpWorkMode : IValues
        {
            [JsonPropertyName("operation_mode__work_mode")]
            public string? Mode { get; set; }
        }
    }

	public class WorkModes
    {
        public const string FEED_IN = "Feedin";
        public const string SELF_USE = "SelfUse";
        public const string BACKUP = "Backup";
		public const string FORCE_DISCHARGE = "ForceDischarge";

		public readonly static string[] ALL = new[] { FEED_IN, SELF_USE, BACKUP, FORCE_DISCHARGE };
    }

    public class WorkModeSpaKeys
    {
        public const string H106 = "h106";
        public const string H108 = "h108";
        public const string H111 = "h111";
        public const string H112 = "h112";
        public const string H115 = "h115";
        public const string OpWorkMode = "operation_mode__work_mode";

		public const string DEFAULT = OpWorkMode;
        public readonly static string[] ALL = new[] { OpWorkMode, H115, H106, H108, H111, H112 };
    }
}
