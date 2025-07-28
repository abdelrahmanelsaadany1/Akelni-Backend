//using System.Net.Http;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace Services.Auth
//{
//    public class FacebookAuthService
//    {
//        public async Task<(string Email, string FacebookId)> VerifyFacebookTokenAsync(string accessToken)
//        {
//            using var client = new HttpClient();
//            var response = await client.GetAsync($"https://graph.facebook.com/me?fields=email,id&access_token={accessToken}");

//            if (!response.IsSuccessStatusCode)
//                throw new Exception("Invalid Facebook access token.");

//            var content = await response.Content.ReadAsStringAsync();
//            var json = JsonDocument.Parse(content).RootElement;

//            var email = json.GetProperty("email").GetString();
//            var id = json.GetProperty("id").GetString();

//            return (email, id);
//        }
//    }
//}