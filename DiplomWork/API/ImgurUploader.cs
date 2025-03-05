using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace DiplomWork
{
    class ImgurUploader
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string ClientId = "120b21487556190"; 

        public async Task<string> UploadImageAsync(byte[] imageBytes)
        {
            using (var content = new MultipartFormDataContent())
            {
                
                var byteArrayContent = new ByteArrayContent(imageBytes);
                byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                
                content.Add(byteArrayContent, "image", "image.jpg");
                
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", ClientId);
                
                var response = await httpClient.PostAsync("https://api.imgur.com/3/image", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(jsonResponse);
                    return result.data.link;
                }
                else
                {
                    throw new Exception($"Error uploading image: {response.ReasonPhrase}");
                }
            }
        }
    }
}
