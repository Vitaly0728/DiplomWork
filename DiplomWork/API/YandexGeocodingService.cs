using Newtonsoft.Json.Linq;

namespace DiplomWork
{
    internal class YandexGeocodingService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly string? YandexGeocodeApiKey = Environment.GetEnvironmentVariable("YandexGeocodeApiKey");
        private const string YandexGeocodeUrl = "https://geocode-maps.yandex.ru/1.x/?apikey={0}&geocode={1},{2}&format=json";

        public async Task<string> GetAddressFromCoordinates(string latitude, string longitude)
        {
            try
            {
                string url = string.Format(YandexGeocodeUrl, YandexGeocodeApiKey, longitude, latitude);
                var response = await httpClient.GetStringAsync(url);

                JObject jsonResponse = JObject.Parse(response);
                var formattedAddress = jsonResponse["response"]["GeoObjectCollection"]["featureMember"][0]["GeoObject"]["metaDataProperty"]["GeocoderMetaData"]["Address"]["formatted"].ToString();

                return formattedAddress;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка при получении адреса: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка: {ex.Message}");
                return null;
            }
        }
    }
}
