using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google_sheetAndro.Models;
using Refractored.XamForms.PullToRefresh.Droid;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Google_sheetAndro.Droid
{
    [Activity(Label = "Небо для всех", Theme = "@style/Theme.Splash", Icon = "@mipmap/icon",
        MainLauncher = true, NoHistory = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SplashScreen : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity//Activity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
            await Task.Delay(200);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            //CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Xamarin.FormsGoogleMaps.Init(this, savedInstanceState);
            PullToRefreshLayoutRenderer.Init();
            XFGloss.Droid.Library.Init(this, savedInstanceState);
            global::Xamarin.Auth.Presenters.XamarinAndroid.AuthenticationConfiguration.Init(this, savedInstanceState);
            global::Xamarin.Auth.CustomTabsConfiguration.CustomTabsClosingMessage = null;
            List<PermissionStatus> ListPerm = new List<PermissionStatus>();
            var stsatus = await Permissions.CheckStatusAsync<Permissions.Media>();
            var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationAlways>();
            }
            ListPerm.Add(status);
            status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            ListPerm.Add(status);
            status = await Permissions.CheckStatusAsync<Permissions.NetworkState>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.NetworkState>();
            }
            ListPerm.Add(status);
            status = await Permissions.CheckStatusAsync<Permissions.Maps>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Maps>();
            }
            ListPerm.Add(status);
            if (ListPerm.Any(o => o != PermissionStatus.Granted))
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    Toast.MakeText(Android.App.Application.Context, "Не получены разрешения", ToastLength.Long).Show();
                    await Task.Delay(1000);
                    Toast.MakeText(Android.App.Application.Context, "Приложение будет закрыто", ToastLength.Long).Show();
                    await Task.Delay(1000);
                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                });
            }
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(1000);
                var request = new GeolocationRequest(GeolocationAccuracy.Lowest);
                var s = await Geolocation.GetLocationAsync(request, cts.Token);
            }
            catch (FeatureNotEnabledException)
            {
                Toast.MakeText(Android.App.Application.Context, "Геолокация выключена, пожалуйста включите!", ToastLength.Long).Show();
                await Task.Delay(1000);
                Toast.MakeText(Android.App.Application.Context, "И перезапустите приложение", ToastLength.Long).Show();
                await Task.Delay(1000);
                StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionLocat‌​ionSourceSettings));
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                //StartActivityForResult(new Android.Content.Intent(Android.Provider.Settings.ActionLocat‌​ionSourceSettings), 0);
            }

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
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}