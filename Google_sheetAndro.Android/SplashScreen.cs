using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google_sheetAndro.Models;
using Plugin.CurrentActivity;
using Refractored.XamForms.PullToRefresh.Droid;
using Xamarin.Essentials;

namespace Google_sheetAndro.Droid
{
    [Activity(Label = "Небо для всех", Theme = "@style/Theme.Splash", Icon = "@mipmap/icon",
        MainLauncher = true, NoHistory = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SplashScreen : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity//Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
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
            global::Xamarin.Auth.CustomTabsConfiguration.CustomTabsClosingMessage = null;
            var network = Connectivity.NetworkAccess;
            if (network == NetworkAccess.None)
            {
                Toast.MakeText(Android.App.Application.Context, "Отсутствует интернет соединение", ToastLength.Long).Show();
                LoaderFunction.fl_offline = true;
                //StartActivity(typeof(MainActivity));
            }
            else
            {
                LoaderFunction.fl_offline = false;
                //StartActivity(typeof(MainActivity));
            }
            var intent = new Intent(this, typeof(MainActivity));
            //intent.AddFlags(ActivityFlags.ClearTop);
            intent.AddFlags(ActivityFlags.SingleTop);
            intent.AddFlags(ActivityFlags.NewTask);
            StartActivity(intent);
            Finish();
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
    }
}