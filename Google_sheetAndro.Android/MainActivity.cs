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
using Newtonsoft.Json;
using Plugin.CurrentActivity;
using System;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Google_sheetAndro.Droid
{
    [Activity(Label = "Небо для всех", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)] //MainLauncher = false, 
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IGoogleAuthenticationDelegate
    {
        static bool fl_wait = false;
        static App ap;
        //public static GoogleOauth Auth;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            CrossCurrentActivity.Current.Activity = this;
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            base.OnCreate(savedInstanceState);
            if (!fl_wait)
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
            var name = await SecureStorage.GetAsync("name");
            if (oathToken != null && tt != null && name != null)
            {
                StaticInfo.AccountEmail = oathToken;
                StaticInfo.AccountPicture = tt;
                StaticInfo.AccountFullName = name;
                while (true)
                {
                    if (LoaderFunction.is_Loaded)
                    {
                        LoaderFunction.SetterStatus("Завершаем загрузку...");
                        LoaderFunction.EndLoad();
                        break;
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                }
            }
            else
            {
                Auth = new GoogleAuthenticator(Configuration.ClientId, Configuration.Scope, Configuration.RedirectUrl, this);
                var authenticator = Auth.GetAuthenticator();
                var intent = authenticator.GetUI(this);
                intent.SetFlags(ActivityFlags.NewTask);//ActivityFlags.ReorderToFront | ActivityFlags.SingleTop
                CustomTabsConfiguration.CustomTabsClosingMessage = null;
                StartActivity(intent);
                //Finish();
            }
        }
        public async void OnAuthenticationCompleted(GoogleOAuthToken token)
        {
            // Retrieve the user's email address
            var googleService = new GoogleService();
            var email = await googleService.GetEmailAsync(token.TokenType, token.AccessToken);
            await Xamarin.Essentials.SecureStorage.SetAsync("token", email.email);
            await Xamarin.Essentials.SecureStorage.SetAsync("picture", email.picture);
            await Xamarin.Essentials.SecureStorage.SetAsync("name", email.name);
            StaticInfo.AccountEmail = email.email;
            StaticInfo.AccountPicture = email.picture;
            StaticInfo.AccountFullName = email.name;
            while (true)
            {
                if(LoaderFunction.is_Loaded)
                {
                    LoaderFunction.SetterStatus("Завершаем загрузку...");
                    LoaderFunction.EndLoad();
                    break;
                }
                else
                {
                    await Task.Delay(500);
                }
            }
            //StaticInfo.SetMenuUserAct();
            //Finish();
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
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }
        protected override void JavaFinalize()
        {
            base.JavaFinalize();    
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