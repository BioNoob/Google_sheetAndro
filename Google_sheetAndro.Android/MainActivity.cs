﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Google_sheetAndro.Class;
using Plugin.CurrentActivity;
using Refractored.XamForms.PullToRefresh.Droid;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Essentials;

namespace Google_sheetAndro.Droid
{
    [Activity(Label = "База полётов", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)] //MainLauncher = false, 
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        //public static GoogleOauth Auth;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            //TabLayoutResource = Resource.Layout.Tabbar;
            //ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);

            //Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            //global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            //Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            //CrossCurrentActivity.Current.Init(this, savedInstanceState);
            //Xamarin.FormsGoogleMaps.Init(this, savedInstanceState);
            //PullToRefreshLayoutRenderer.Init();
            //XFGloss.Droid.Library.Init(this, savedInstanceState);

            //global::Xamarin.Auth.Presenters.XamarinAndroid.AuthenticationConfiguration.Init(this, savedInstanceState);
            var network = Connectivity.NetworkAccess;
            if (network == NetworkAccess.None)
            {
                Toast.MakeText(Android.App.Application.Context, "Отсутствует интернет соединение", ToastLength.Long).Show();
                await Task.Delay(1000);
                Toast.MakeText(Android.App.Application.Context, "Приложение пока не поддерживает редактирование офлайн", ToastLength.Long).Show();
            }
            else
            {
                LoginDo();
            }
        }
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
                var authenticator = SplashScreen.Auth.GetAuthenticator();
                var intent = authenticator.GetUI(this);
                intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
                CustomTabsConfiguration.CustomTabsClosingMessage = null;
                StartActivity(intent);
        }
    }
        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        //{
        //    Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}
    }
}