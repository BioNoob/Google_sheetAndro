using Android.Content.PM;
using Android.App;
using Android.OS;
using Android.Runtime;
using Plugin.CurrentActivity;
using Refractored.XamForms.PullToRefresh.Droid;
using Google_sheetAndro.Authentication;
using System;
using Google_sheetAndro.Services;
using Android.Widget;
using Google_sheetAndro.Class;

namespace Google_sheetAndro.Droid
{
    [Activity(Label = "База полётов", Theme = "@style/Theme.Splash", Icon = "@mipmap/icon",
        MainLauncher = true, NoHistory = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SplashScreen : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IGoogleAuthenticationDelegate//Activity
    {
        public static GoogleAuthenticator Auth;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Xamarin.FormsGoogleMaps.Init(this, savedInstanceState);
            PullToRefreshLayoutRenderer.Init();
            XFGloss.Droid.Library.Init(this, savedInstanceState);
            global::Xamarin.Auth.Presenters.XamarinAndroid.AuthenticationConfiguration.Init(this, savedInstanceState);
            Auth = new GoogleAuthenticator(Configuration.ClientId, Configuration.Scope, Configuration.RedirectUrl, this);
            StartActivity(typeof(MainActivity));
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        public async void OnAuthenticationCompleted(GoogleOAuthToken token)
        {
            // Retrieve the user's email address
            var googleService = new GoogleService();
            var email = await googleService.GetEmailAsync(token.TokenType, token.AccessToken);
            await Xamarin.Essentials.SecureStorage.SetAsync("token", email.email);
            await Xamarin.Essentials.SecureStorage.SetAsync("picture", email.picture);
            StaticInfo.AccountEmail = email.email;
            StaticInfo.AccountPicture = email.picture;
        }

        public void OnAuthenticationCanceled()
        {
            Toast.MakeText(Android.App.Application.Context, "Вход отменен", ToastLength.Long).Show();
        }

        public void OnAuthenticationFailed(string message, Exception exception)
        {
            Toast.MakeText(Android.App.Application.Context, "Ошибка входа " + message, ToastLength.Long).Show();
        }
    }

}