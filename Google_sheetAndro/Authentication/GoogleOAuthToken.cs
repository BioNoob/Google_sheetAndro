namespace Google_sheetAndro.Authentication
{
    public class GoogleOAuthToken
    {
        public string TokenType { get; set; }
        public string AccessToken { get; set; }
        public Xamarin.Auth.Account TokenAccount { get; set; }
    }
}
