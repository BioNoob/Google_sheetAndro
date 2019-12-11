using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Google_sheetAndro.Authentication;
using Google_sheetAndro.Class;
using Google_sheetAndro.Services;
using Plugin.CurrentActivity;
using Refractored.XamForms.PullToRefresh.Droid;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Google_sheetAndro.Droid
{
    [Activity(Label = "Небо для всех", Theme = "@style/Theme.Splash", Icon = "@mipmap/icon",
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
            var network = Connectivity.NetworkAccess;
            if (network == NetworkAccess.None)
            {
                Toast.MakeText(Android.App.Application.Context, "Отсутствует интернет соединение", ToastLength.Long).Show();
                //Toast.MakeText(Android.App.Application.Context, "Приложение пока не поддерживает редактирование офлайн", ToastLength.Long).Show();
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(1000);
                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                });
            }
            else
            {
                Auth = new GoogleAuthenticator(Configuration.ClientId, Configuration.Scope, Configuration.RedirectUrl, this);
                StartActivity(typeof(MainActivity));
            }
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //if (requestCode == RequestLocationId)
            //{
            //    if ((grantResults.Length == 1) && (grantResults[0] == (int)Permission.Granted))
            //    {

            //    }
            //// Permissions granted - display a message.
            //        else
            //    {

            //    }
            //// Permissions denied - display a message.
            //}
            //else
            //{
            //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //}

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
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(1000);
                Toast.MakeText(Android.App.Application.Context, "Приложение будет закрыто", ToastLength.Long).Show();
                await Task.Delay(1000);
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            });
            //var closer = DependencyService.Get<ICloseApplication>();
            //closer?.closeApplication();
        }

        public void OnAuthenticationFailed(string message, Exception exception)
        {
            Toast.MakeText(Android.App.Application.Context, "Ошибка входа " + message, ToastLength.Long).Show();
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(1000);
                Toast.MakeText(Android.App.Application.Context, "Приложение будет закрыто", ToastLength.Long).Show();
                await Task.Delay(1000);
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            });

            //var closer = DependencyService.Get<ICloseApplication>();
            //closer?.closeApplication();
        }
    }

}