using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Configurations;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Clients.Win.Request
{
    public class HttpRequest
    {
        private string _token;
        private readonly HttpClient _httpClient;
        private readonly RemoteOptions _remoteOptions;
        private readonly JsonSerializerOptions _jsonOptions = SystemTextJsonSerializer.GetDefaultOptions();

        public HttpRequest(IOptions<RemoteOptions> remoteOptions)
        {
            _remoteOptions = remoteOptions.Value;
            _httpClient = new HttpClient();
            BuildHttpClient(_httpClient);
        }

        public void SetToken(string token)
        {
            _token = token;
            if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
            }

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        public async Task<HttpResponseMessage> PostAsync(string url, dynamic body, bool excuted = false)
        {
            HttpResponseMessage resp = null;
            if (body == null)
            {
                resp = await _httpClient.PostAsync(url, null);
            }
            else
            {
                var content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                resp = await _httpClient.PostAsync(url, content);
            }
            if (resp.IsSuccessStatusCode)
            {
                return resp;
            }
            else if (resp.StatusCode == HttpStatusCode.Unauthorized && !excuted)
            {
                var result = TryRefreshToken?.Invoke();
                if (result == null || !result.Value)
                {
                    ExcuteWhileUnauthorized?.Invoke();
                    return default;
                }
                excuted = true;
                return await PostAsync(url, body, excuted);
            }
            else if (resp.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await resp.Content.ReadAsStringAsync();
                ExcuteWhileBadRequest?.Invoke(message);
            }
            else if (resp.StatusCode == HttpStatusCode.InternalServerError)
            {
                var message = await resp.Content.ReadAsStringAsync();
                ExcuteWhileInternalServerError?.Invoke(message);
            }

            return default;
        }

        public async Task<HttpResponseMessage> GetAsync(string url, bool excuted = false)
        {
            var resp = await _httpClient.GetAsync(url).ConfigureAwait(false);
            if (resp.IsSuccessStatusCode)
            {
                return resp;
            }
            else if (resp.StatusCode == HttpStatusCode.Unauthorized && !excuted)
            {
                var result = TryRefreshToken?.Invoke();
                if (result == null || !result.Value)
                {
                    ExcuteWhileUnauthorized?.Invoke();
                    return default;
                }
                excuted = true;
                return await GetAsync(url, excuted);
            }
            else if (resp.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                ExcuteWhileBadRequest?.Invoke(message);
            }
            else if (resp.StatusCode == HttpStatusCode.InternalServerError)
            {
                var message = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                ExcuteWhileInternalServerError?.Invoke(message);
            }

            return default;
        }

        public async Task<UserRefreshTokenDto> RefreshTokenAsync(UserRefreshTokenDto body)
        {
            var content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var resp = await _httpClient.PostAsync(Const.REFRESH_TOKEN, content);
            if (resp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<UserRefreshTokenDto>(await resp.Content.ReadAsStringAsync(), _jsonOptions);
            }

            return default;
        }

        public event Func<bool> TryRefreshToken;//当服务端返回401的时候，尝试利用refreshtoken重新获取accesstoken以及refreshtoken
        public event Action ExcuteWhileUnauthorized; //401
        public event Action<string> ExcuteWhileBadRequest;//400
        public event Action<string> ExcuteWhileInternalServerError;//500

        private void BuildHttpClient(HttpClient httpClient, bool api = true)
        {
            if (api)
            {
                httpClient.BaseAddress = new Uri($"http://{_remoteOptions.Host}:{_remoteOptions.Port}/api/");
            }
            httpClient.Timeout = TimeSpan.FromSeconds(600); //Test
            httpClient.DefaultRequestVersion = HttpVersion.Version10;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
