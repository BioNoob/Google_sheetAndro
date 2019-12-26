using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google_sheetAndro.Authentication;
using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using Google_sheetAndro.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Google_sheetAndro.Droid
{
    [Activity(Label = "База полётов", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)] //MainLauncher = false, 
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IGoogleAuthenticationDelegate
    {
        static bool fl_wait = false;
        static App ap;
        //public static GoogleOauth Auth;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if(!fl_wait)
            {
                if (!LoaderFunction.fl_offline)
                {
                    ap = new App();
                    LoadApplication(ap);
                    LoginDo();
                }
                else
                {
                    LoadOfflineVersion();
                }
            }
            else
            {
                LoadApplication(ap);
            }
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }
        private void LoadOfflineVersion()
        {
            LoadApplication(new AppOffline());
        }
        public static GoogleAuthenticator Auth;
        public async void LoginDo()
        {

            var oathToken = await SecureStorage.GetAsync("token");
            var tt = await SecureStorage.GetAsync("picture");
            if (oathToken != null && tt != null)
            {
                StaticInfo.AccountEmail = oathToken;
                StaticInfo.AccountPicture = tt;
            }
            else
            {
                Auth = new GoogleAuthenticator(Configuration.ClientId, Configuration.Scope, Configuration.RedirectUrl, this);
                var authenticator = Auth.GetAuthenticator();
                var intent = authenticator.GetUI(this);
                intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
                CustomTabsConfiguration.CustomTabsClosingMessage = null;
                StartActivity(intent);
            }
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
            //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
            //{
            //    Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //}
        }
        protected override void OnPause()
        {
            fl_wait = true;
            base.OnPause();
        }
        protected override void OnStop()
        {
            base.OnStop();
        }
        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}