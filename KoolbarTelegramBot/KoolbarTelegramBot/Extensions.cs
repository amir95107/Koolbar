using Newtonsoft.Json;
using System.Text;

namespace KoolbarTelegramBot
{
    public struct ConfigJSON
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; private set; }
    }
    public static class Extentions
    {
        public async static Task<ConfigJSON> ReadFromJson(string path)
        {
            var json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            return JsonConvert.DeserializeObject<ConfigJSON>(json);
        }
    }
}
