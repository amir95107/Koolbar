using Newtonsoft.Json;
using System.Text;

namespace KoolbarTelegramBot.HttpClientProvider
{

    public static class ApiCall
    {
        private static readonly HttpClient _client;
        
        private static readonly string BaseUrl = "https://localhost:7171/api/";
        static ApiCall()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(BaseUrl);
#if !DEBUG
_client.BaseAddress = new Uri("http://localhost:5000/api/");
#endif
        }
        public static async Task<T> GetAsync<T>(string url) where T : class
        {
            var request = await _client.GetAsync(url);
            try
            {
                if (request.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(await request.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            throw new Exception(request.RequestMessage.Content.ReadAsStringAsync().Result);
        }

        public static async Task<T> PostAsync<T, U>(string url, U u)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(u), Encoding.UTF8, "application/json");
                var request = await _client.PostAsync(url, content);
                if (request.IsSuccessStatusCode)
                {
                    var response = JsonConvert.DeserializeObject<T>(await request.Content.ReadAsStringAsync());
                    return response;
                }
                throw new Exception(request.RequestMessage.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static async Task<T> PostAsync<T>(string url, T t)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(t), Encoding.UTF8, "application/json");
                var request = await _client.PostAsync(url, content);
                var responseContent = await request.Content.ReadAsStringAsync();
                if (request.IsSuccessStatusCode)
                {
                    var response = JsonConvert.DeserializeObject<T>(responseContent);

                    return response;
                }
                throw new Exception(await request.RequestMessage.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
