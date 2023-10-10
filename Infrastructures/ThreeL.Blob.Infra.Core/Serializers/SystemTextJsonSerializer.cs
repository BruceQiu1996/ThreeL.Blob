using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace ThreeL.Blob.Infra.Core.Serializers
{
    public static class SystemTextJsonSerializer
    {
        public static JsonSerializerOptions GetDefaultOptions(Action<JsonSerializerOptions>? configOptions = null)
        {
            var options = new JsonSerializerOptions();
            options.Encoder = GetDefaultEncoder();
            options.ReadCommentHandling = JsonCommentHandling.Skip;
            options.PropertyNameCaseInsensitive = true;
            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            configOptions?.Invoke(options);
            return options;
        }

        public static JavaScriptEncoder GetDefaultEncoder() => JavaScriptEncoder.Create(new TextEncoderSettings(UnicodeRanges.All));
    }
}
