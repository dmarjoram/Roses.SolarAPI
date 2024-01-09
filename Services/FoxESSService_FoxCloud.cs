using Microsoft.Extensions.Caching.Memory;
using PuppeteerSharp;
using Roses.SolarAPI.Exceptions;
using Roses.SolarAPI.Extensions;
using Roses.SolarAPI.Models.FoxCloud;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Roses.SolarAPI.Services
{
    public partial class FoxESSService
    {
        private const string FoxCloudTokenPrefix = "FoxCloudToken";
        private const string FoxCloudDeviceListPrefix = "FoxCloudDeviceList";
        private const string FoxCloudSignatureBlob = "https://www.foxesscloud.com/js/signature.wasm";

        private const int FoxCloudRetryInvalidParamDelayMilliseconds = 500;
        private const int FoxCloudRetryDelayMilliseconds = 5000;
        private const int FoxDeviceListCacheHours = 1;

        // Keep track of SPA key that works
        public static string DefaultSpaKey = SpaKeys.DEFAULT;
        public static int CurrentSpaKey = 0;

        /// <summary>
        /// Get device list for FoxCloud account
        /// </summary>
        public async Task<Device[]> FoxCloudGetDeviceList(CancellationToken ct)
        {
            if (_memoryCache.TryGetValue($"{FoxCloudDeviceListPrefix}:{_config!.Username}", out Device[] deviceList))
            {
                _logger.LogInformation("FoxCloud device list returned from cache.");
                return deviceList;
            }

            DeviceListResponse responseBody = await SendFoxCloudRequest<DeviceListRequest, DeviceListResponse>(new(), ct);

            _logger.LogInformation("Device list retrieved via FoxCloud.");

            deviceList = responseBody.Result?.Devices!;

            if (deviceList != null)
            {
                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(FoxDeviceListCacheHours));

                _memoryCache.Set($"{FoxCloudDeviceListPrefix}:{_config!.Username}", deviceList, cacheEntryOptions);
            }

            return deviceList!;
        }

        /// <summary>
        /// Set MinSoc and MinSocGrid
        /// </summary>
        public async Task<string> FoxCloudSetBothBatteryMinSoC(ushort minSoc, ushort minSocGrid, CancellationToken ct = default)
        {
            SetBothBatteryMinSoCRequest request = new SetBothBatteryMinSoCRequest()
            {
                Sn = await ResolveSerialNumberAsync(ct),
                MinSoc = minSoc,
                MinGridSoc = minSocGrid
            };

            string response = (await SendFoxCloudRequest<SetBothBatteryMinSoCRequest, SetBothBatteryMinSoCResponse>(request, ct))!.ToFoxStatus()!.ToString();

            _logger.LogInformation("MinSOC and MinSOC (on grid) set successfully via FoxCloud.");

            return response;
        }


        /// <summary>
        /// Force discharge from now
        /// </summary>
        public async Task<string> FoxCloudForceDischargeToday(int forceDischargePower = 5000, CancellationToken ct = default)
        {
            TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
            TimeOnly end = new TimeOnly(23, 59);

            SetSchedulerRequest request = new SetSchedulerRequest()
            {
                DeviceSerialNumber = await ResolveSerialNumberAsync(ct),
                Policies = new Policy[]
                {
                    new Policy() {
                        StartHour = now.Hour,
                        StartMinute = now.Minute,
                        EndHour = end.Hour,
                        EndMinute = end.Minute,
                        WorkMode = WorkModes.FORCE_DISCHARGE,
                        MinSOCOnGrid = 10,
                        ForceDischargeSOC = 10,
                        ForceDischargePower = forceDischargePower
                    }
                }
            };

            string response = (await SendFoxCloudRequest<SetSchedulerRequest, SetSchedulerResponse>(request, ct))!.ToFoxStatus()!.ToString();

            _logger.LogInformation("Inverter schedule set successfully via FoxCloud.");

            return response;
        }

        /// <summary>
        /// Disable force discharge
        /// </summary>
        public async Task<string> FoxCloudDisableForceDischarge(CancellationToken ct = default)
        {
            DisableSchedulerRequest request = new DisableSchedulerRequest() { DeviceSerialNumber = await ResolveSerialNumberAsync(ct) };

            string response = (await SendFoxCloudRequest<DisableSchedulerRequest, DisableSchedulerResponse>(request, ct))!.ToFoxStatus()!.ToString();

            _logger.LogInformation("Inverter schedule disabled successfully via FoxCloud.");

            return response;
        }

        /// <summary>
        /// Disable force charge
        /// </summary>
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

            string response = (await SendFoxCloudRequest<SetBatteryChargeTimesRequest, SetBatteryChargeTimesResponse>(request, ct))!.ToFoxStatus()!.ToString();

            _logger.LogInformation("Battery times set successfully via FoxCloud.");

            return response;
        }

        /// <summary>
        /// Force charge from now until 23:59
        /// </summary>
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

            string response = (await SendFoxCloudRequest<SetBatteryChargeTimesRequest, SetBatteryChargeTimesResponse>(request, ct))!.ToFoxStatus()!.ToString();

            _logger.LogInformation("Battery times set successfully via FoxCloud.");

            return response;
        }

        /// <summary>
        /// Force charge around the clock
        /// </summary>
        public async Task<string> FoxCloudForceChargeAllTodayTimePeriod1(bool enableGridCharging = false, CancellationToken ct = default)
        {
            TimeOnly start = new TimeOnly(00, 01);
            TimeOnly end = new TimeOnly(23, 59);

            SetBatteryChargeTimesRequest request = new SetBatteryChargeTimesRequest()
            {
                Sn = await ResolveSerialNumberAsync(ct),
                Times = new Time[]
                {
                    new Time() {
                        StartTime = new StartTime() { Hour = start.Hour.ToString("00"), Minute = start.Minute.ToString("00") },
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

            string response = (await SendFoxCloudRequest<SetBatteryChargeTimesRequest, SetBatteryChargeTimesResponse>(request, ct))!.ToFoxStatus()!.ToString();

            _logger.LogInformation("Battery times set successfully via FoxCloud.");

            return response;
        }

        /// <summary>
        /// Set inverter for feed-in
        /// </summary>
        public async Task<string> FoxCloudSetWorkModeFeedIn(CancellationToken ct = default)
        {
            FoxErrorNumber responseCode = await SetWorkModeWithSpaKeyCheck(WorkModes.FEED_IN, ct);

            _logger.LogInformation("Work mode 'FeedIn' set successfully via FoxCloud.");

            return responseCode.ToString();
        }

        /// <summary>
        /// Set inverter for self-use
        /// </summary>
        public async Task<string> FoxCloudSetWorkModeSelfUse(CancellationToken ct = default)
        {
            FoxErrorNumber responseCode = await SetWorkModeWithSpaKeyCheck(WorkModes.SELF_USE, ct);

            _logger.LogInformation("Work mode 'SelfUse' set successfully via FoxCloud.");

            return responseCode.ToString();
        }

        /// <summary>
        /// Set inverter for backup
        /// </summary>
        public async Task<string> FoxCloudSetWorkModeBackup(CancellationToken ct = default)
        {
            FoxErrorNumber responseCode = await SetWorkModeWithSpaKeyCheck(WorkModes.BACKUP, ct);

            _logger.LogInformation("Work mode 'Backup' set successfully via FoxCloud.");

            return responseCode.ToString();
        }

        /// <summary>
        /// Generate a signature for Fox Cloud API calls
        /// </summary>
        /// <param name="uriPath">URI path</param>
        /// <param name="token">token</param>
        /// <param name="language">language code, e.g. en</param>
        /// <param name="timestamp">unix timestamp in milliseconds</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>signature</returns>
        public async Task<string> FoxCloudGenerateSignature(string uriPath, string token, string language, string timestamp, CancellationToken ct = default)
        {
            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions()
            {
                Headless = true,
                Args = new[] { "--no-sandbox" }
            }))

            using (var page = await browser.NewPageAsync())
            {
                await page.GoToAsync("http://127.0.0.1/embedded-signature.htm");

                await page.WaitForNetworkIdleAsync();

                return await page.EvaluateFunctionAsync<string>($"() => {{ var sign = Module.cwrap(\"begin_signature\", \"string\", [\"string\", \"string\", \"string\", \"string\"]); return sign(\"{uriPath}\", \"{token}\", \"{language}\", \"{timestamp}\"); }}");
            }
        }

        /// <summary>
        /// Generic HTTP request and response to FoxCloud
        /// </summary>
        /// <typeparam name="TRequest">type of request</typeparam>
        /// <typeparam name="TResponse">type of response</typeparam>
        /// <param name="request">concrete request param</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>response object</returns>
        private async Task<TResponse> SendFoxCloudRequest<TRequest, TResponse>(TRequest request, CancellationToken ct, bool noTokenRequired = false) where TResponse : IFoxResponse, new() where TRequest : IFoxRequest
        {
            // Perform some client validation before we contact FoxCloud
            request.Validate();

            Func<Task<TResponse>> sendRequest = async () =>
            {
                // Add FoxCloud SPA access token to request
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Clear();

                string token = string.Empty;

                if (!noTokenRequired)
                {
                    // To prevent a stack over flow when called from login method
                    token = await FoxCloudLogin(ct);
                    client.DefaultRequestHeaders.Add("Token", token);
                }

                long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                string sig = await FoxCloudGenerateSignature(new Uri(request.RequestUri).PathAndQuery, token, "en", timestamp.ToString());

                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Lang", "en");
                client.DefaultRequestHeaders.Add("Signature", sig);
                client.DefaultRequestHeaders.Add("Timestamp", timestamp.ToString());

                _logger.LogInformation("Sending request to FoxCloud of type {request}", request);

                HttpResponseMessage response = request.GetRequest
                    ? await client.GetAsync(request.RequestUri, cancellationToken: ct)
                    : await client.PostAsJsonAsync(request.RequestUri, request, cancellationToken: ct);

                response.EnsureSuccessStatusCode();

                return (await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct))!;
            };

            TResponse responseBody = new();

            return await SendWithRetryAsync(sendRequest, responseBody, ct);
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
                else if (responseBody.ToFoxStatus() == FoxErrorNumber.InvalidRequest)
                {
                    _logger.LogWarning($"FoxCloud reports the request is invalid. We will not retry with the same request parameters.");

                    await Task.Delay(FoxCloudRetryInvalidParamDelayMilliseconds);

                    return responseBody!;
                }
                else
                {
                    _logger.LogWarning($"Error calling FoxCloud. Retrying in {FoxCloudRetryDelayMilliseconds} ms.");

                    await Task.Delay(FoxCloudRetryDelayMilliseconds);
                }

                responseBody = await sendRequest();

                if (responseBody.ToFoxStatus() != FoxErrorNumber.OK)
                {
                    throw new FoxResponseException(responseBody.ToFoxStatus(), $"FoxCloud returned failure code {responseBody.Errno} ({responseBody.ToFoxStatus()}).");
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
            await AssertSignatureBlob();

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

            LoginResponse responseBody = await SendFoxCloudRequest<LoginRequest, LoginResponse>(request, ct, true);

            _logger.LogInformation("FoxCloud login successful.");

            token = responseBody.Result?.Token!;

            if (!string.IsNullOrWhiteSpace(token))
            {
                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.TokenCacheSeconds));

                _memoryCache.Set($"{FoxCloudTokenPrefix}:{_config!.Username}", token, cacheEntryOptions);
            }

            return token;
        }

        private async Task AssertSignatureBlob()
        {
            // Check for signature generator
            string signatureBlob = Path.Combine(_webHostEnvironment.WebRootPath, Path.Combine("js", "signature.wasm"));

            if (!File.Exists(signatureBlob))
            {
                try
                {
                    HttpClient client = new HttpClient();
                    using (Stream stream = await client.GetStreamAsync(FoxCloudSignatureBlob))
                    {
                        using (FileStream fileStream = new FileStream(signatureBlob, FileMode.CreateNew))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                    _logger.LogInformation("Signature blob downloaded.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error downloading signature blob. {ex.Message}");
                    if (File.Exists(signatureBlob))
                    {
                        _logger.LogInformation("Deleting incomplete signature blob.");
                        File.Delete(signatureBlob);
                    }
                }
            }
        }

        private async Task<string> ResolveSerialNumberAsync(CancellationToken ct)
        {
            if (_config!.SerialNumber == null)
            {
                _logger.LogWarning("No environment variable set for device serial number. Contacting FoxCloud to query first device on account.");

                _config.SerialNumber = (await FoxCloudGetDeviceList(ct))?.FirstOrDefault()?.DeviceSn;
            }

            return _config!.SerialNumber!;
        }

        private async Task<Guid?> ResolveCloudDeviceIdAsync(CancellationToken ct)
        {
            if (_config!.CloudDeviceId == null)
            {
                _logger.LogWarning("No environment variable set for cloud device ID. Contacting FoxCloud to query first device on account.");

                _config.CloudDeviceId = (await FoxCloudGetDeviceList(ct)).FirstOrDefault()?.DeviceId;
            }

            return _config!.CloudDeviceId!;
        }

        private async Task<FoxErrorNumber> SetWorkModeWithSpaKeyCheck(string workMode, CancellationToken ct)
        {
            Func<Task<FoxErrorNumber>> sendRequest = async () =>
            {
                SetWorkModeRequest request = new SetWorkModeRequest(DefaultSpaKey, workMode)
                {
                    Id = (await ResolveCloudDeviceIdAsync(ct)).GetValueOrDefault()
                };

                return (await SendFoxCloudRequest<SetWorkModeRequest, SetWorkModeResponse>(request, ct))!.ToFoxStatus()!;
            };

            FoxErrorNumber responseCode = await sendRequest();

            for (int i = 0; i < SpaKeys.ALL.Length; i++)
            {
                if (responseCode == FoxErrorNumber.InvalidRequest)
                {
                    // Try next SPA key
                    DefaultSpaKey = SpaKeys.ALL[++CurrentSpaKey % SpaKeys.ALL.Length];

                    _logger?.LogInformation("Trying next SPA key. {DefaultSpaKey}", DefaultSpaKey);

                    responseCode = await sendRequest();
                }
            }

            return responseCode;
        }
    }
}
