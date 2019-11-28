using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Google_sheetAndro.Services
{
    public class GoogleService
    {
        public async Task<GoogleEmail> GetEmailAsync(string tokenType, string accessToken)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);
            var json = await httpClient.GetStringAsync("https://www.googleapis.com/oauth2/v2/userinfo");//https://www.googleapis.com/auth/userinfo.email");//"https://www.googleapis.com/userinfo/email?alt=json");
            var email = JsonConvert.DeserializeObject<GoogleEmail>(json);
            //https://github.com/sameerkapps/SecureStorage
            /*
             *   "id": "100153214095093018867",
  "email": "yasma.corp@gmail.com",
  "verified_email": true,
  "picture": "https://lh3.googleusercontent.com/a-/AAuE7mCr23i1pyWicbrxmz-DBYPpokUfbYHt89234ZK0"
             */
            return email;
        }
    }
}
