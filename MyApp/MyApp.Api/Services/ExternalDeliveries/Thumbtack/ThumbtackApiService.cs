using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyApp.Api.Services.ExternalDeliveries.Thumbtack
{
    public sealed class ThumbtackApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ThumbtackOptions _options;
        private readonly ILogger<ThumbtackApiService> _logger;

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public ThumbtackApiService(
            HttpClient httpClient,
            IOptions<ThumbtackOptions> options,
            ILogger<ThumbtackApiService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync(
            CancellationToken cancellationToken = default)
        {
            var environment = GetCurrentEnvironment();

            if (string.IsNullOrWhiteSpace(environment.TokenUrl))
                throw new InvalidOperationException(
                    "Thumbtack TokenUrl is missing.");

            if (string.IsNullOrWhiteSpace(environment.ClientId))
                throw new InvalidOperationException(
                    "Thumbtack ClientId is missing.");

            if (string.IsNullOrWhiteSpace(environment.ClientSecret))
                throw new InvalidOperationException(
                    "Thumbtack ClientSecret is missing.");

            if (string.IsNullOrWhiteSpace(environment.Audience))
                throw new InvalidOperationException(
                    "Thumbtack Audience is missing.");

            /*
             * Thumbtack uses OAuth2 client credentials.
             *
             * The supplied documentation identifies HydraOAuth but does not
             * specify that a scope is mandatory. Send scope only when
             * Thumbtack explicitly provides one.
             */

            var rawCredentials =
                $"{environment.ClientId}:{environment.ClientSecret}";

            var encodedCredentials =
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(rawCredentials));

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    environment.TokenUrl);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    encodedCredentials);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));

            var tokenFields =
                new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["audience"] = environment.Audience.Trim()
                };

            if (!string.IsNullOrWhiteSpace(environment.Scope))
            {
                tokenFields["scope"] =
                    environment.Scope.Trim();
            }

            request.Content =
                new FormUrlEncodedContent(tokenFields);

            _logger.LogInformation(
                "Thumbtack OAuth request starting. Environment={Environment}",
                _options.UseStaging
                    ? "Staging"
                    : "Production");

            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            var responseText =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Thumbtack OAuth failed. HTTP={StatusCode}, Response={Response}",
                    (int)response.StatusCode,
                    responseText);

                throw new InvalidOperationException(
                    $"Thumbtack OAuth token request failed. " +
                    $"HTTP {(int)response.StatusCode}: {responseText}");
            }

            ThumbtackTokenResponse? tokenResponse;

            try
            {
                tokenResponse =
                    JsonSerializer.Deserialize<ThumbtackTokenResponse>(
                        responseText,
                        JsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Unable to parse Thumbtack OAuth response.",
                    ex);
            }

            if (tokenResponse == null ||
                string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException(
                    "Thumbtack OAuth response did not contain access_token.");
            }

            _logger.LogInformation(
                "Thumbtack OAuth token obtained successfully.");

            return tokenResponse.AccessToken;
        }

        public async Task<ThumbtackBusinessSearchResponse>
            SearchBusinessesAsync(
                string categoryId,
                string zipCode,
                string trackingId,
                int limit = 10,
                CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                throw new InvalidOperationException(
                    "Thumbtack integration is disabled.");
            }

            if (string.IsNullOrWhiteSpace(categoryId))
            {
                throw new InvalidOperationException(
                    "Thumbtack category ID is missing.");
            }

            if (string.IsNullOrWhiteSpace(zipCode))
            {
                throw new InvalidOperationException(
                    "Thumbtack ZIP code is required.");
            }

            zipCode = zipCode.Trim();

            if (zipCode.Length != 5 ||
                !zipCode.All(char.IsDigit))
            {
                throw new InvalidOperationException(
                    "Thumbtack ZIP code must be a valid 5-digit ZIP code.");
            }

            if (string.IsNullOrWhiteSpace(_options.UtmSource))
            {
                throw new InvalidOperationException(
                    "Thumbtack UtmSource is missing.");
            }

            if (!_options.UtmSource.StartsWith(
                    "cma-",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Thumbtack UtmSource must begin with 'cma-'.");
            }

            if (limit < 1)
                limit = 1;

            if (limit > 30)
                limit = 30;

            var environment =
                GetCurrentEnvironment();

            if (string.IsNullOrWhiteSpace(environment.ApiBaseUrl))
            {
                throw new InvalidOperationException(
                    "Thumbtack ApiBaseUrl is missing.");
            }

            var accessToken =
                await GetAccessTokenAsync(
                    cancellationToken);

            var payload =
                new ThumbtackBusinessSearchRequest
                {
                    CategoryId = categoryId,
                    ZipCode = zipCode,
                    Limit = limit,
                    UtmData =
                        new ThumbtackUtmData
                        {
                            UtmSource =
                                _options.UtmSource,

                            UtmContent =
                                trackingId
                        }
                };

            var requestUrl =
                $"{environment.ApiBaseUrl.TrimEnd('/')}/v4/businesses/search";

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    requestUrl);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));

            request.Content =
                JsonContent.Create(
                    payload,
                    options: JsonOptions);

            _logger.LogInformation(
                "Thumbtack Business Search starting. CategoryId={CategoryId}, ZipCode={ZipCode}, TrackingId={TrackingId}",
                categoryId,
                zipCode,
                trackingId);

            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            var responseText =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Thumbtack Business Search failed. HTTP={StatusCode}, Response={Response}",
                    (int)response.StatusCode,
                    responseText);

                throw new InvalidOperationException(
                    $"Thumbtack businesses search failed. " +
                    $"HTTP {(int)response.StatusCode}: {responseText}");
            }

            ThumbtackBusinessSearchResponse? result;

            try
            {
                result =
                    JsonSerializer.Deserialize<
                        ThumbtackBusinessSearchResponse>(
                        responseText,
                        JsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Unable to parse Thumbtack businesses search response.",
                    ex);
            }

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Thumbtack businesses search returned an empty response.");
            }

            return result;
        }

        private ThumbtackEnvironmentOptions
            GetCurrentEnvironment()
        {
            return _options.UseStaging
                ? _options.Staging
                : _options.Production;
        }

        private sealed class ThumbtackBusinessSearchRequest
        {
            [JsonPropertyName("categoryID")]
            public string CategoryId { get; set; } =
                string.Empty;

            [JsonPropertyName("zipCode")]
            public string ZipCode { get; set; } =
                string.Empty;

            [JsonPropertyName("limit")]
            public int Limit { get; set; }

            [JsonPropertyName("utmData")]
            public ThumbtackUtmData UtmData { get; set; } =
                new();
        }

        private sealed class ThumbtackUtmData
        {
            [JsonPropertyName("utm_source")]
            public string UtmSource { get; set; } =
                string.Empty;

            [JsonPropertyName("utm_content")]
            public string? UtmContent { get; set; }
        }

        public sealed class ThumbtackBusinessSearchResponse
        {
            [JsonPropertyName("searchID")]
            public string? SearchId { get; set; }

            [JsonPropertyName("data")]
            public List<ThumbtackBusinessResult>
                Data
            { get; set; } =
                    new();

            [JsonPropertyName("metadata")]
            public ThumbtackSearchMetadata?
                Metadata
            { get; set; }
        }

        public sealed class ThumbtackBusinessResult
        {
            [JsonPropertyName("businessID")]
            public string? BusinessId { get; set; }

            [JsonPropertyName("businessName")]
            public string? BusinessName { get; set; }

            [JsonPropertyName("businessIntroduction")]
            public string? BusinessIntroduction { get; set; }

            [JsonPropertyName("businessLocation")]
            public string? BusinessLocation { get; set; }

            [JsonPropertyName("rating")]
            public decimal? Rating { get; set; }

            [JsonPropertyName("numberOfReviews")]
            public int? NumberOfReviews { get; set; }

            [JsonPropertyName("yearsInBusiness")]
            public int? YearsInBusiness { get; set; }

            [JsonPropertyName("numberOfHires")]
            public int? NumberOfHires { get; set; }

            [JsonPropertyName("isTopPro")]
            public bool? IsTopPro { get; set; }

            [JsonPropertyName("servicePageURL")]
            public string? ServicePageUrl { get; set; }

            [JsonPropertyName("businessImageURL")]
            public string? BusinessImageUrl { get; set; }

            [JsonPropertyName("widgets")]
            public ThumbtackWidgets?
                Widgets
            { get; set; }
        }

        public sealed class ThumbtackWidgets
        {
            [JsonPropertyName("requestFlowURL")]
            public string? RequestFlowUrl { get; set; }

            [JsonPropertyName("servicePageURL")]
            public string? ServicePageUrl { get; set; }
        }

        public sealed class ThumbtackSearchMetadata
        {
            [JsonPropertyName("categoryID")]
            public string? CategoryId { get; set; }

            [JsonPropertyName("categoryName")]
            public string? CategoryName { get; set; }

            [JsonPropertyName("zipCode")]
            public string? ZipCode { get; set; }

            [JsonPropertyName("requestLocation")]
            public string? RequestLocation { get; set; }

            [JsonPropertyName("seeMoreProsURL")]
            public string? SeeMoreProsUrl { get; set; }
        }

        private sealed class ThumbtackTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }

            [JsonPropertyName("token_type")]
            public string? TokenType { get; set; }

            [JsonPropertyName("expires_in")]
            public int? ExpiresIn { get; set; }

            [JsonPropertyName("scope")]
            public string? Scope { get; set; }
        }
    }
}


//using Microsoft.Extensions.Options;
 //using System.Net.Http.Headers;
 //using System.Net.Http.Json;
 //using System.Text;
 //using System.Text.Json;
 //using System.Text.Json.Serialization;

//namespace MyApp.Api.Services.ExternalDeliveries.Thumbtack
//{
//    public sealed class ThumbtackApiService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly ThumbtackOptions _options;
//        private readonly ILogger<ThumbtackApiService> _logger;

//        private static readonly JsonSerializerOptions JsonOptions =
//            new()
//            {
//                PropertyNameCaseInsensitive = true
//            };

//        public ThumbtackApiService(
//            HttpClient httpClient,
//            IOptions<ThumbtackOptions> options,
//            ILogger<ThumbtackApiService> logger)
//        {
//            _httpClient = httpClient;
//            _options = options.Value;
//            _logger = logger;
//        }

//        public async Task<string> GetAccessTokenAsync(
//            CancellationToken cancellationToken = default)
//        {
//            var environment = GetCurrentEnvironment();

//            if (string.IsNullOrWhiteSpace(environment.TokenUrl))
//                throw new InvalidOperationException(
//                    "Thumbtack TokenUrl is missing.");

//            if (string.IsNullOrWhiteSpace(environment.ClientId))
//                throw new InvalidOperationException(
//                    "Thumbtack ClientId is missing.");

//            if (string.IsNullOrWhiteSpace(environment.ClientSecret))
//                throw new InvalidOperationException(
//                    "Thumbtack ClientSecret is missing.");

//            if (string.IsNullOrWhiteSpace(environment.Audience))
//                throw new InvalidOperationException(
//                    "Thumbtack Audience is missing.");

//            if (string.IsNullOrWhiteSpace(environment.Scope))
//            {
//                throw new InvalidOperationException(
//                    "Thumbtack OAuth Scope is missing. " +
//                    "Copy the Required Scope from the AUTHORIZATIONS section " +
//                    "of POST /v4/businesses/search in Thumbtack API Reference.");
//            }

//            /*
//             * OAuth2 client authentication:
//             * Authorization: Basic base64(clientId:clientSecret)
//             */

//            var rawCredentials =
//                $"{environment.ClientId}:{environment.ClientSecret}";

//            var encodedCredentials =
//                Convert.ToBase64String(
//                    Encoding.UTF8.GetBytes(rawCredentials));

//            using var request =
//                new HttpRequestMessage(
//                    HttpMethod.Post,
//                    environment.TokenUrl);

//            request.Headers.Authorization =
//                new AuthenticationHeaderValue(
//                    "Basic",
//                    encodedCredentials);

//            request.Headers.Accept.Add(
//                new MediaTypeWithQualityHeaderValue(
//                    "application/json"));

//            request.Content =
//                new FormUrlEncodedContent(
//                    new Dictionary<string, string>
//                    {
//                        ["grant_type"] =
//                            "client_credentials",

//                        ["audience"] =
//                            environment.Audience,

//                        ["scope"] =
//                            environment.Scope
//                    });

//            _logger.LogInformation(
//                "Thumbtack OAuth request starting. Environment={Environment}",
//                _options.UseStaging
//                    ? "Staging"
//                    : "Production");

//            using var response =
//                await _httpClient.SendAsync(
//                    request,
//                    cancellationToken);

//            var responseText =
//                await response.Content.ReadAsStringAsync(
//                    cancellationToken);

//            if (!response.IsSuccessStatusCode)
//            {
//                _logger.LogError(
//                    "Thumbtack OAuth failed. HTTP={StatusCode}, Response={Response}",
//                    (int)response.StatusCode,
//                    responseText);

//                throw new InvalidOperationException(
//                    $"Thumbtack OAuth token request failed. " +
//                    $"HTTP {(int)response.StatusCode}: {responseText}");
//            }

//            ThumbtackTokenResponse? tokenResponse;

//            try
//            {
//                tokenResponse =
//                    JsonSerializer.Deserialize<ThumbtackTokenResponse>(
//                        responseText,
//                        JsonOptions);
//            }
//            catch (Exception ex)
//            {
//                throw new InvalidOperationException(
//                    "Unable to parse Thumbtack OAuth response.",
//                    ex);
//            }

//            if (tokenResponse == null ||
//                string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
//            {
//                throw new InvalidOperationException(
//                    "Thumbtack OAuth response did not contain access_token.");
//            }

//            _logger.LogInformation(
//                "Thumbtack OAuth token obtained successfully.");

//            return tokenResponse.AccessToken;
//        }

//        public async Task<ThumbtackBusinessSearchResponse>
//            SearchBusinessesAsync(
//                string categoryId,
//                string zipCode,
//                string trackingId,
//                int limit = 10,
//                CancellationToken cancellationToken = default)
//        {
//            if (!_options.Enabled)
//            {
//                throw new InvalidOperationException(
//                    "Thumbtack integration is disabled.");
//            }

//            if (string.IsNullOrWhiteSpace(categoryId))
//            {
//                throw new InvalidOperationException(
//                    "Thumbtack category ID is missing.");
//            }

//            if (string.IsNullOrWhiteSpace(zipCode))
//            {
//                throw new InvalidOperationException(
//                    "Thumbtack ZIP code is required.");
//            }

//            zipCode = zipCode.Trim();

//            if (zipCode.Length != 5 ||
//                !zipCode.All(char.IsDigit))
//            {
//                throw new InvalidOperationException(
//                    "Thumbtack ZIP code must be a valid 5-digit ZIP code.");
//            }

//            if (string.IsNullOrWhiteSpace(_options.UtmSource))
//            {
//                throw new InvalidOperationException(
//                    "Thumbtack UtmSource is missing.");
//            }

//            if (!_options.UtmSource.StartsWith(
//                    "cma-",
//                    StringComparison.OrdinalIgnoreCase))
//            {
//                throw new InvalidOperationException(
//                    "Thumbtack UtmSource must begin with 'cma-'.");
//            }

//            if (limit < 1)
//                limit = 1;

//            if (limit > 30)
//                limit = 30;

//            var environment =
//                GetCurrentEnvironment();

//            if (string.IsNullOrWhiteSpace(environment.ApiBaseUrl))
//            {
//                throw new InvalidOperationException(
//                    "Thumbtack ApiBaseUrl is missing.");
//            }

//            var accessToken =
//                await GetAccessTokenAsync(
//                    cancellationToken);

//            var payload =
//                new ThumbtackBusinessSearchRequest
//                {
//                    CategoryId = categoryId,
//                    ZipCode = zipCode,
//                    Limit = limit,
//                    UtmData =
//                        new ThumbtackUtmData
//                        {
//                            UtmSource =
//                                _options.UtmSource,

//                            UtmContent =
//                                trackingId
//                        }
//                };

//            var requestUrl =
//                $"{environment.ApiBaseUrl.TrimEnd('/')}/v4/businesses/search";

//            using var request =
//                new HttpRequestMessage(
//                    HttpMethod.Post,
//                    requestUrl);

//            request.Headers.Authorization =
//                new AuthenticationHeaderValue(
//                    "Bearer",
//                    accessToken);

//            request.Headers.Accept.Add(
//                new MediaTypeWithQualityHeaderValue(
//                    "application/json"));

//            request.Content =
//                JsonContent.Create(
//                    payload,
//                    options: JsonOptions);

//            _logger.LogInformation(
//                "Thumbtack Business Search starting. CategoryId={CategoryId}, ZipCode={ZipCode}, TrackingId={TrackingId}",
//                categoryId,
//                zipCode,
//                trackingId);

//            using var response =
//                await _httpClient.SendAsync(
//                    request,
//                    cancellationToken);

//            var responseText =
//                await response.Content.ReadAsStringAsync(
//                    cancellationToken);

//            if (!response.IsSuccessStatusCode)
//            {
//                _logger.LogError(
//                    "Thumbtack Business Search failed. HTTP={StatusCode}, Response={Response}",
//                    (int)response.StatusCode,
//                    responseText);

//                throw new InvalidOperationException(
//                    $"Thumbtack businesses search failed. " +
//                    $"HTTP {(int)response.StatusCode}: {responseText}");
//            }

//            ThumbtackBusinessSearchResponse? result;

//            try
//            {
//                result =
//                    JsonSerializer.Deserialize<
//                        ThumbtackBusinessSearchResponse>(
//                        responseText,
//                        JsonOptions);
//            }
//            catch (Exception ex)
//            {
//                throw new InvalidOperationException(
//                    "Unable to parse Thumbtack businesses search response.",
//                    ex);
//            }

//            if (result == null)
//            {
//                throw new InvalidOperationException(
//                    "Thumbtack businesses search returned an empty response.");
//            }

//            return result;
//        }

//        private ThumbtackEnvironmentOptions
//            GetCurrentEnvironment()
//        {
//            return _options.UseStaging
//                ? _options.Staging
//                : _options.Production;
//        }

//        private sealed class ThumbtackBusinessSearchRequest
//        {
//            [JsonPropertyName("categoryID")]
//            public string CategoryId { get; set; } =
//                string.Empty;

//            [JsonPropertyName("zipCode")]
//            public string ZipCode { get; set; } =
//                string.Empty;

//            [JsonPropertyName("limit")]
//            public int Limit { get; set; }

//            [JsonPropertyName("utmData")]
//            public ThumbtackUtmData UtmData { get; set; } =
//                new();
//        }

//        private sealed class ThumbtackUtmData
//        {
//            [JsonPropertyName("utm_source")]
//            public string UtmSource { get; set; } =
//                string.Empty;

//            [JsonPropertyName("utm_content")]
//            public string? UtmContent { get; set; }
//        }

//        public sealed class ThumbtackBusinessSearchResponse
//        {
//            [JsonPropertyName("searchID")]
//            public string? SearchId { get; set; }

//            [JsonPropertyName("data")]
//            public List<ThumbtackBusinessResult>
//                Data
//            { get; set; } =
//                    new();

//            [JsonPropertyName("metadata")]
//            public ThumbtackSearchMetadata?
//                Metadata
//            { get; set; }
//        }

//        public sealed class ThumbtackBusinessResult
//        {
//            [JsonPropertyName("businessID")]
//            public string? BusinessId { get; set; }

//            [JsonPropertyName("businessName")]
//            public string? BusinessName { get; set; }

//            [JsonPropertyName("businessIntroduction")]
//            public string? BusinessIntroduction { get; set; }

//            [JsonPropertyName("businessLocation")]
//            public string? BusinessLocation { get; set; }

//            [JsonPropertyName("rating")]
//            public decimal? Rating { get; set; }

//            [JsonPropertyName("numberOfReviews")]
//            public int? NumberOfReviews { get; set; }

//            [JsonPropertyName("yearsInBusiness")]
//            public int? YearsInBusiness { get; set; }

//            [JsonPropertyName("numberOfHires")]
//            public int? NumberOfHires { get; set; }

//            [JsonPropertyName("isTopPro")]
//            public bool? IsTopPro { get; set; }

//            [JsonPropertyName("servicePageURL")]
//            public string? ServicePageUrl { get; set; }

//            [JsonPropertyName("businessImageURL")]
//            public string? BusinessImageUrl { get; set; }

//            [JsonPropertyName("widgets")]
//            public ThumbtackWidgets?
//                Widgets
//            { get; set; }
//        }

//        public sealed class ThumbtackWidgets
//        {
//            [JsonPropertyName("requestFlowURL")]
//            public string? RequestFlowUrl { get; set; }

//            [JsonPropertyName("servicePageURL")]
//            public string? ServicePageUrl { get; set; }
//        }

//        public sealed class ThumbtackSearchMetadata
//        {
//            [JsonPropertyName("categoryID")]
//            public string? CategoryId { get; set; }

//            [JsonPropertyName("categoryName")]
//            public string? CategoryName { get; set; }

//            [JsonPropertyName("zipCode")]
//            public string? ZipCode { get; set; }

//            [JsonPropertyName("requestLocation")]
//            public string? RequestLocation { get; set; }

//            [JsonPropertyName("seeMoreProsURL")]
//            public string? SeeMoreProsUrl { get; set; }
//        }

//        private sealed class ThumbtackTokenResponse
//        {
//            [JsonPropertyName("access_token")]
//            public string? AccessToken { get; set; }

//            [JsonPropertyName("token_type")]
//            public string? TokenType { get; set; }

//            [JsonPropertyName("expires_in")]
//            public int? ExpiresIn { get; set; }

//            [JsonPropertyName("scope")]
//            public string? Scope { get; set; }
//        }
//    }
//}