﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Clients.Win.Request
{
    public abstract class HttpRequest
    {
        internal string _token;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = SystemTextJsonSerializer.GetDefaultOptions();

        private readonly string _host;
        private readonly ushort _port;

        public HttpRequest(string host, ushort port)
        {
            _host = host;
            _port = port;
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
                var result = await (TryRefreshToken?.Invoke());
                if (!result)
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

        public async Task<HttpResponseMessage> PostAvatarAsync(string filename, byte[] bytes, bool excuted = false)
        {
            using (var client = new HttpClient())
            {
                BuildHttpClient(client);
                if (!string.IsNullOrEmpty(_token))
                {
                    if (client.DefaultRequestHeaders.Contains("Authorization"))
                    {
                        client.DefaultRequestHeaders.Remove("Authorization");
                    }
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
                }
                var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = filename
                };
                var content = new MultipartFormDataContent
                {
                    fileContent
                };

                var resp = await client.PostAsync(string.Format(Const.UPLOAD_AVATAR), content);
                if (resp.IsSuccessStatusCode)
                {
                    return resp;
                }
                else if (resp.StatusCode == HttpStatusCode.Unauthorized && !excuted)
                {
                    var result = await (TryRefreshToken?.Invoke());
                    if (!result)
                    {
                        ExcuteWhileUnauthorized?.Invoke();
                        return default;
                    }
                    excuted = true;
                    return await PostAvatarAsync(filename, bytes, excuted);
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
            }

            return default;
        }

        public async Task<HttpResponseMessage> PutAsync(string url, dynamic body, bool excuted = false)
        {
            HttpResponseMessage resp = null;
            if (body == null)
            {
                resp = await _httpClient.PutAsync(url, null);
            }
            else
            {
                var content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                resp = await _httpClient.PutAsync(url, content);
            }
            if (resp.IsSuccessStatusCode)
            {
                return resp;
            }
            else if (resp.StatusCode == HttpStatusCode.Unauthorized && !excuted)
            {
                var result = await (TryRefreshToken?.Invoke());
                if (!result)
                {
                    ExcuteWhileUnauthorized?.Invoke();
                    return default;
                }
                excuted = true;
                return await PutAsync(url, body, excuted);
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
                var result = await (TryRefreshToken?.Invoke());
                if (!result)
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

        public Func<Task<bool>> TryRefreshToken;//当服务端返回401的时候，尝试利用refreshtoken重新获取accesstoken以及refreshtoken
        public event Action ExcuteWhileUnauthorized; //401
        public event Action<string> ExcuteWhileBadRequest;//400
        public event Action<string> ExcuteWhileInternalServerError;//500

        public void BuildHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri($"http://{_host}:{_port}/api/");
            httpClient.Timeout = TimeSpan.FromSeconds(600); //Test
            httpClient.DefaultRequestVersion = HttpVersion.Version10;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
