using Microsoft.Extensions.Caching.Memory;
using Roses.SolarAPI.Extensions;
using Roses.SolarAPI.Models.FoxCloud;
using System.Net;

namespace Roses.SolarAPI.Services
{
    public partial class FoxESSService
    {
        private const string FoxCloudTokenPrefix = "FoxCloudToken";
        private const string SetBatteryChargeTimesUri = "https://www.foxesscloud.com/c/v0/device/battery/time/set";
        private const string FoxCloudLoginUri = "https://www.foxesscloud.com/c/v0/user/login";

        private const int FoxCloudRetryDelayMilliseconds = 5000;

        public async Task<string> FoxCloudDisableForceChargeTimePeriod1(CancellationToken ct = default)
        {
            SetBatteryChargeTimesRequest request = new SetBatteryChargeTimesRequest()
            {
                Sn = _config!.SerialNumber,
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
                Sn = _config!.SerialNumber,
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

        private async Task<SetBatteryChargeTimesResponse> SetBatteryChargeTimes(SetBatteryChargeTimesRequest request, CancellationToken ct)
        {
            // Perform some client validation before we contact FoxCloud
            request.Validate();

            HttpClient client = new HttpClient();

            Func<Task<SetBatteryChargeTimesResponse>> sendRequest = async () =>
            {
                // Add FoxCloud SPA access token to request
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Token", await FoxCloudLogin(ct));

                _logger.LogInformation("Sending request to FoxCloud of type {request}", request);

                HttpResponseMessage response = await client.PostAsJsonAsync(SetBatteryChargeTimesUri, request, cancellationToken: ct);
                response.EnsureSuccessStatusCode();

                return (await response.Content.ReadFromJsonAsync<SetBatteryChargeTimesResponse>(cancellationToken: ct))!;
            };

            SetBatteryChargeTimesResponse responseBody = new();

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

            _logger.LogInformation("Battery times set successfully via FoxCloud.");

            return responseBody!;
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

            HttpClient client = new HttpClient();

            LoginRequest request = new LoginRequest()
            {
                User = _config!.Username,
                Password = _config.PasswordMD5Hash
            };

            // Perform some client validation before we contact FoxCloud
            request.Validate();

            Func<Task<LoginResponse>> sendRequest = async () =>
            {
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
    }
}
