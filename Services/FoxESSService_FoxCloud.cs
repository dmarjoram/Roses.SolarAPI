using Microsoft.Extensions.Caching.Memory;
using Roses.SolarAPI.Extensions;
using Roses.SolarAPI.Models.FoxCloud;
using System.Net;

namespace Roses.SolarAPI.Services
{
    public partial class FoxESSService
    {
        private const string FoxCloudTokenPrefix = "FoxCloudToken";
        private const string FoxCloudDeviceListPrefix = "FoxCloudDeviceList";

        private const string SetBatteryChargeTimesUri = "https://www.foxesscloud.com/c/v0/device/battery/time/set";
        private const string FoxCloudLoginUri = "https://www.foxesscloud.com/c/v0/user/login";
        private const string DeviceListUri = "https://www.foxesscloud.com/c/v0/device/list";
        private const string SetWorkModeUri = "https://www.foxesscloud.com/c/v0/device/setting/set";

        private const int FoxCloudRetryDelayMilliseconds = 5000;
        private const int FoxDeviceListCacheHours = 1;

        public async Task<Device[]> GetDeviceList(CancellationToken ct)
        {
            if (_memoryCache.TryGetValue($"{FoxCloudDeviceListPrefix}:{_config!.Username}", out Device[] deviceList))
            {
                _logger.LogInformation("FoxCloud device list returned from cache.");
                return deviceList;
            }

            DeviceListRequest request = new DeviceListRequest();

            Func<Task<DeviceListResponse>> sendRequest = async () =>
            {
                // Add FoxCloud SPA access token to request
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Token", await FoxCloudLogin(ct));

                _logger.LogInformation("Sending request to FoxCloud of type {request}", request);

                HttpResponseMessage response = await client.PostAsJsonAsync(DeviceListUri, request, cancellationToken: ct);
                response.EnsureSuccessStatusCode();

                return (await response.Content.ReadFromJsonAsync<DeviceListResponse>(cancellationToken: ct))!;
            };

            DeviceListResponse responseBody = new();

            responseBody = await SendWithRetryAsync(sendRequest, responseBody, ct);

            _logger.LogInformation("Device list retrieved via FoxCloud.");

            deviceList = responseBody.Result?.Devices!;

            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(FoxDeviceListCacheHours));

            _memoryCache.Set($"{FoxCloudDeviceListPrefix}:{_config!.Username}", deviceList, cacheEntryOptions);

            return deviceList!;
        }

        public async Task<string> FoxCloudDisableForceChargeTimePeriod1(CancellationToken ct = default)
        {
            SetBatteryChargeTimesRequest request = new SetBatteryChargeTimesRequest()
            {
                Sn = await ResolveSerialNumberAsync(ct),
                Times = new Time[]
                {
                    new Time() {
                        StartTime = new StartTime() { Hour = "0", Minute = "0" },
                        EnableCharge = false,
                        EnableGrid = false,
                        EndTime = new EndTime() { Hour = "0", Minute = "0" },
                        Tip = string.Empty
                    },
                    new Time() {
                        StartTime = new StartTime() { Hour = "0", Minute = "0" },
                        EnableCharge = false,
                        EnableGrid = false,
                        EndTime = new EndTime() { Hour = "0", Minute = "0" },
                        Tip = string.Empty
                    },
                }
            };

            return (await SetBatteryChargeTimes(request, ct))!.ToFoxStatus()!.ToString();
        }

        public async Task<string> FoxCloudForceChargeForTodayTimePeriod1(bool enableGridCharging = false, CancellationToken ct = default)
        {
            TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
            TimeOnly end = new TimeOnly(23, 59);

            SetBatteryChargeTimesRequest request = new SetBatteryChargeTimesRequest()
            {
                Sn = await ResolveSerialNumberAsync(ct),
                Times = new Time[]
                {
                    new Time() {
                        StartTime = new StartTime() { Hour = now.Hour.ToString("00"), Minute = now.Minute.ToString("00") },
                        EnableCharge = true,
                        EnableGrid = enableGridCharging,
                        EndTime = new EndTime() { Hour = end.Hour.ToString("00"), Minute = end.Minute.ToString("00") },
                        Tip = string.Empty
                    },
                    new Time() {
                        StartTime = new StartTime() { Hour = "0", Minute = "0" },
                        EnableCharge = false,
                        EnableGrid = false,
                        EndTime = new EndTime() { Hour = "0", Minute = "0" },
                        Tip = string.Empty
                    },
                }
            };

            return (await SetBatteryChargeTimes(request, ct))!.ToFoxStatus()!.ToString();
        }

        /// <summary>
        /// Set inverter for feed-in
        /// </summary>
        public async Task<string> FoxCloudSetWorkModeFeedIn(CancellationToken ct = default)
        {
            SetWorkModeRequest request = new SetWorkModeRequest()
            {
                Id = (await ResolveCloudDeviceIdAsync(ct)).GetValueOrDefault()
            };

            request.Values!.Mode = WorkModes.FEED_IN;

            return (await SetWorkMode(request, ct))!.ToFoxStatus()!.ToString();
        }

        /// <summary>
        /// Set inverter for self-use
        /// </summary>
        public async Task<string> FoxCloudSetWorkModeSelfUse(CancellationToken ct = default)
        {
            SetWorkModeRequest request = new SetWorkModeRequest()
            {
                Id = (await ResolveCloudDeviceIdAsync(ct)).GetValueOrDefault()
            };

            request.Values!.Mode = WorkModes.SELF_USE;

            return (await SetWorkMode(request, ct))!.ToFoxStatus()!.ToString();
        }

        /// <summary>
        /// Set inverter for backup
        /// </summary>
        public async Task<string> FoxCloudSetWorkModeBackup(CancellationToken ct = default)
        {
            SetWorkModeRequest request = new SetWorkModeRequest()
            {
                Id = (await ResolveCloudDeviceIdAsync(ct)).GetValueOrDefault()
            };

            request.Values!.Mode = WorkModes.BACKUP;

            return (await SetWorkMode(request, ct))!.ToFoxStatus()!.ToString();
        }

        private async Task<SetWorkModeResponse> SetWorkMode(SetWorkModeRequest request, CancellationToken ct)
        {
            // Perform some client validation before we contact FoxCloud
            request.Validate();

            Func<Task<SetWorkModeResponse>> sendRequest = async () =>
            {
                // Add FoxCloud SPA access token to request
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Token", await FoxCloudLogin(ct));

                _logger.LogInformation("Sending request to FoxCloud of type {request}", request);

                HttpResponseMessage response = await client.PostAsJsonAsync(SetWorkModeUri, request, cancellationToken: ct);
                response.EnsureSuccessStatusCode();

                return (await response.Content.ReadFromJsonAsync<SetWorkModeResponse>(cancellationToken: ct))!;
            };

            SetWorkModeResponse responseBody = new();

            responseBody = await SendWithRetryAsync(sendRequest, responseBody, ct);

            _logger.LogInformation("Work mode set successfully via FoxCloud.");

            return responseBody!;
        }

        private async Task<SetBatteryChargeTimesResponse> SetBatteryChargeTimes(SetBatteryChargeTimesRequest request, CancellationToken ct)
        {
            // Perform some client validation before we contact FoxCloud
            request.Validate();

            Func<Task<SetBatteryChargeTimesResponse>> sendRequest = async () =>
            {
                // Add FoxCloud SPA access token to request
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Token", await FoxCloudLogin(ct));

                _logger.LogInformation("Sending request to FoxCloud of type {request}", request);

                HttpResponseMessage response = await client.PostAsJsonAsync(SetBatteryChargeTimesUri, request, cancellationToken: ct);
                response.EnsureSuccessStatusCode();

                return (await response.Content.ReadFromJsonAsync<SetBatteryChargeTimesResponse>(cancellationToken: ct))!;
            };

            SetBatteryChargeTimesResponse responseBody = new();

            responseBody = await SendWithRetryAsync(sendRequest, responseBody, ct);

            _logger.LogInformation("Battery times set successfully via FoxCloud.");

            return responseBody!;
        }

        /// <summary>
        /// Helper method to handle timeouts and retry a FoxCloud call
        /// </summary>
        private async Task<TResponse> SendWithRetryAsync<TResponse>(Func<Task<TResponse>> sendRequest, TResponse responseBody, CancellationToken ct) where TResponse : IFoxResponse
        {
            try
            {
                responseBody = await sendRequest();
                responseBody = await CheckResponseAndRetry(sendRequest!, responseBody, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception calling FoxCloud. {exception}", ex);

                responseBody = await CheckResponseAndRetry(sendRequest!, responseBody, ct);
            }

            return responseBody;
        }

        /// <summary>
        /// Check fox cloud response and attempt a retry if not successful
        /// </summary>
        /// <typeparam name="TResponse">the type of the response object</typeparam>
        /// <param name="sendRequest">fox cloud request message</param>
        /// <param name="responseBody">deserialised reponse</param>
        /// <returns>fox cloud API response</returns>
        /// <exception cref="WebException">any http level exception</exception>
        private async Task<TResponse> CheckResponseAndRetry<TResponse>(Func<Task<TResponse>> sendRequest, TResponse responseBody, CancellationToken ct = default) where TResponse : IFoxResponse
        {
            if (responseBody.ToFoxStatus() != FoxErrorNumber.OK)
            {
                if (responseBody.ToFoxStatus() == FoxErrorNumber.TokenInvalid)
                {
                    _logger.LogWarning($"Error calling FoxCloud as token is invalid. Getting new token and retrying in {FoxCloudRetryDelayMilliseconds / 2} ms.");

                    // Get a new uncached token
                    await FoxCloudLogin(ct, false);

                    await Task.Delay(FoxCloudRetryDelayMilliseconds / 2);
                }
                else
                {
                    _logger.LogWarning($"Error calling FoxCloud. Retrying in {FoxCloudRetryDelayMilliseconds} ms.");

                    await Task.Delay(FoxCloudRetryDelayMilliseconds);
                }

                responseBody = await sendRequest();

                if (responseBody.ToFoxStatus() != FoxErrorNumber.OK)
                {
                    throw new WebException($"FoxCloud returned failure code {responseBody.Errno} ({responseBody.ToFoxStatus()}).");
                }
            }

            return responseBody!;
        }

        /// <summary>
        /// Get a FoxCloud token
        /// </summary>
        /// <param name="ct">cancellation token</param>
        /// <returns>fox cloud token</returns>
        private async Task<string> FoxCloudLogin(CancellationToken ct = default, bool useCache = true)
        {
            if (useCache && _memoryCache.TryGetValue($"{FoxCloudTokenPrefix}:{_config!.Username}", out string token))
            {
                _logger.LogInformation("FoxCloud login returning cached token.");
                return token;
            }

            LoginRequest request = new LoginRequest()
            {
                User = _config!.Username,
                Password = _config.PasswordMD5Hash
            };

            // Perform some client validation before we contact FoxCloud
            request.Validate();

            Func<Task<LoginResponse>> sendRequest = async () =>
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.PostAsJsonAsync(FoxCloudLoginUri, request, cancellationToken: ct);
                response.EnsureSuccessStatusCode();

                return (await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct))!;
            };

            LoginResponse responseBody = new();

            try
            {
                responseBody = await sendRequest();
                responseBody = await CheckResponseAndRetry(sendRequest!, responseBody, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception calling FoxCloud. {exception}", ex);

                responseBody = await CheckResponseAndRetry(sendRequest!, responseBody, ct);
            }

            _logger.LogInformation("FoxCloud login successful.");

            token = responseBody.Result?.Token!;

            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.TokenCacheSeconds));

            _memoryCache.Set($"{FoxCloudTokenPrefix}:{_config!.Username}", token, cacheEntryOptions);

            return token;
        }

        private async Task<string> ResolveSerialNumberAsync(CancellationToken ct)
        {
            if (_config!.SerialNumber == null)
            {
                _logger.LogWarning("No environment variable set for device serial number. Contacting FoxCloud to query first device on account.");

                _config.SerialNumber = (await GetDeviceList(ct)).FirstOrDefault()?.DeviceSn;
            }

            return _config!.SerialNumber!;
        }

        private async Task<Guid?> ResolveCloudDeviceIdAsync(CancellationToken ct)
        {
            if (_config!.CloudDeviceId == null)
            {
                _logger.LogWarning("No environment variable set for cloud device ID. Contacting FoxCloud to query first device on account.");

                _config.CloudDeviceId = (await GetDeviceList(ct)).FirstOrDefault()?.DeviceId;
            }

            return _config!.CloudDeviceId!;
        }
    }
}
