using Android.App;
using Android.Widget;
using Google_sheetAndro.Class;
using Google_sheetAndro.Services;
using Google_sheetAndro.Views;
using Newtonsoft.Json;
using Plugin.DeviceSensors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TableAndro;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Google_sheetAndro.Models
{
    public static class LoaderFunction
    {
        public delegate void SetView(Location location);
        public static event SetView DoSetView;
        public delegate void CreateRow();
        public static event CreateRow DoCreateRow;
        public delegate void WheatherLoad();
        public static event WheatherLoad DoWheatherLoad;
        public delegate void SetStatus(string s);
        public static event SetStatus DoSetStatus;
        public delegate void StatusPush(string status);
        public static event StatusPush DoStatusPush;
        public delegate void ClearMap();
        public static event ClearMap DoClearMap;
        public delegate void BackToUpload();
        public static event BackToUpload DoBackToUpload;

        public static void DoBack()
        {
            DoBackToUpload?.Invoke();
        }

        public static bool fl_offline { get; set; }
        public static void callClearMap()
        {
            LoaderFunction.DoClearMap?.Invoke();
        }
        public static async Task<bool> GetGEOAsync()
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(1000);
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                SetterStatus("Получение текущих координат...");
                var s = await Geolocation.GetLocationAsync(request,cts.Token);
                //var ssd = await Geolocation.GetLastKnownLocationAsync();
                StaticInfo.Pos = s;
            }
            #region catch
            catch (FeatureNotSupportedException)
            {
                // Handle not supported on device exception
                SetterStatus("Ошибка в получении координат...");
                //return fnsEx.Message;
            }
            catch (FeatureNotEnabledException)
            {
                // Handle not enabled on device exception
                SetterStatus("Ошибка в получении координат...");
                //await Task.Delay(1000);
                //Toast.MakeText(Android.App.Application.Context, "Геолокация выключена, пожалуйста включите!", ToastLength.Long).Show();
                //await Task.Delay(1000);
                //Android.App.Application.Context.StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionLocat‌​ionSourceSettings));
                //return fneEx.Message;
            }
            catch (PermissionException)
            {
                // Handle permission exception
                SetterStatus("Ошибка в получении координат...");
                //return pEx.Message;
            }
            catch (Exception)
            {
                SetterStatus("Ошибка в получении координат...");
                // Unable to get location
                //return ex.Message;
            }
            #endregion
            return true;
        }
        public static async Task<bool> init()
        {
            bool qqw;
            try
            {
                SetterStatus("Получение информации о текущей локации...");
                if (string.IsNullOrEmpty(StaticInfo.Place))
                    qqw = await StaticInfo.GetPlace(StaticInfo.Pos.Latitude, StaticInfo.Pos.Longitude);
                SetterStatus("Загрузка погоды...");
                if (StaticInfo.Wheather == null)
                    qqw = await StaticInfo.GetWeatherReqAsync(StaticInfo.Pos);
                CrossDeviceSensors.Current.Barometer.OnReadingChanged += MapPage.Barometer_OnReadingChanged;
                CrossDeviceSensors.Current.Barometer.OnReadingChanged += MapPageAlone.Barometer_OnReadingChanged;
                CrossDeviceSensors.Current.Barometer.StartReading();
                return true;
            }
            catch (Exception)
            {
                return false;
            }


            //Place = StaticInfo.Place;
            //gpp = StaticInfo.Wheather;
        }
        public static void RunSetter(Location pos)
        {
            DoSetView?.Invoke(pos);
        }
        public static void SetterStatus(string st)
        {
            Debug.WriteLine(st);
            DoSetStatus?.Invoke(st);
        }
        public static void DostatPush(string tat)
        {
            //var assembly = Assembly.GetExecutingAssembly();
            //var resourceName = "Google_sheetAndro.LogFile.txt";
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var filename = Path.Combine(path, "LogFile.txt");
            using (var reader = new StreamWriter(filename, false, System.Text.Encoding.UTF8))
            {
                {
                    reader.WriteLine(tat);
                    reader.Close();
                }
            }

            //string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LogFile.txt");
            //File.WriteAllText(fileName,tat + "\n");
            //using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Write))
            //{
            //    fileStream.
            //    File.WriteAllText()
            //    image.CopyTo(fileStream);
            //}
            //Device.BeginInvokeOnMainThread(() => Toast.MakeText(Android.App.Application.Context, tat, ToastLength.Long).Show());
            // Device.InvokeOnMainThreadAsync(()=>DoStatusPush?.Invoke(tat));
        }
        public static void CreRow()
        {
            DoCreateRow?.Invoke();
        }
        public static bool LoadLastState()
        {
            try
            {
                var q = Xamarin.Essentials.Preferences.Get("last_known_state", "");
                SaveService ss = SaveService.Deserialize(q);
                if (!string.IsNullOrEmpty(q) && ss != null & ss.ti.date.Year != 1)
                {
                    switch (ss.CurrentMode)
                    {
                        case SaveService.ActiveMode.newpage:
                            LoaderFunction.MapPage.AbsSetter(ss.ti.route, ss.ti.points);
                            LoaderFunction.ItemsPage.setter(ss.ti);
                            LoaderFunction.ItemsInfoPage.ToolbarItem_Clicked(null, new System.EventArgs());
                            switch (ss.CurrentPage)
                            {
                                case SaveService.ActivePage.map:
                                    LoaderFunction.MainPage.CurrentPage = LoaderFunction.MainPage.Children[1];
                                    break;
                                case SaveService.ActivePage.item:
                                    LoaderFunction.MainPage.CurrentPage = LoaderFunction.MainPage.Children[0];
                                    break;
                                case SaveService.ActivePage.wheather:
                                    LoaderFunction.MainPage.CurrentPage = LoaderFunction.MainPage.Children[2];
                                    break;
                                case SaveService.ActivePage.items:
                                    break;
                            }
                            break;
                        case SaveService.ActiveMode.watchpage:
                            LoaderFunction.ItemsInfoPage.SetItembyTap(ss.ti);
                            switch (ss.CurrentPage)
                            {
                                case SaveService.ActivePage.map:
                                    LoaderFunction.ExtItemsViewer.CurrentPage = LoaderFunction.ExtItemsViewer.Children[1];
                                    break;
                                case SaveService.ActivePage.item:
                                    LoaderFunction.ExtItemsViewer.CurrentPage = LoaderFunction.ExtItemsViewer.Children[0];
                                    break;
                                case SaveService.ActivePage.wheather:
                                    break;
                                case SaveService.ActivePage.items:
                                    break;
                            }
                            break;
                    }
                    return true;
                }
                else
                    return false;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

        }
        public static void EndLoad()
        {
            ItemsInfoPage.Title = "Записи";
            if(!is_offline)
            {
                LoaderFunction.MenuPage.sett(LoaderFunction.ItemsInfoPage);
                if (LoadLastState())
                {
                    Xamarin.Essentials.Preferences.Set("last_known_state", "");
                }
            }
            //DoWheatherLoad?.Invoke();
            if (MenuPage != null && StaticInfo.AccountEmail != null)
            {
                MenuPage.setImg(StaticInfo.AccountPicture, StaticInfo.AccountEmail, StaticInfo.AccountFullName);
                StaticInfo.SetMenuUserAct();
            }
        }
        public static InfoPage InfoPage;
        public static ItemsInfo ItemsInfoPage;
        public static ItemsPage ItemsPage;
        public static ItemsPage ItemsPageAlone;
        public static MainPage MainPage;
        public static MainPage ExtItemsViewer;
        public static MapPage MapPage;
        public static MapPage MapPageAlone;
        public static MenuPage MenuPage;
        public static Page_out OutPage;
        public static SimpleListView SimpPage;
        public static TaskSelectPage TaskSelectPage;
        public static WheatherView WheatherPage;
        public static NavigationPage ItAlNavPage;
        public static NavigationPage MapNavPage;
        public static NavigationPage MapAlNavPage;
        public static NavigationPage WheNavPage;
        public static NavigationPage ItNavPage;
        public static NavigationPage AbNavPage;
        public static NavigationPage OutNavPage;
        public static NavigationPage MAINNavPage;
        public static NavigationPage ItInfoNavPage;
        public static NavigationPage ExtItNavPage;
        //NavigationPage itemspg;
        public static readonly string GetAopaPath = "https://maps.aopa.ru/export/exportFormRequest/?exportType=standart&exportAll%5B%5D=airport&exportAll%5B%5D=vert&exportFormat=csv&csv_options%5Bcharset%5D=utf8&csv_options%5Bdata%5D=objects_data&f%5B%5D=index&f%5B%5D=kta_lon&f%5B%5D=kta_lat&api_key=7380-9xJ8zG";
        public static void EndWheatherLoad()
        {
            DoWheatherLoad?.Invoke();
        }
        public static bool is_Loaded;
        public static bool is_offline = false;
        public static async Task<bool> InitialiserPage()
        {
            SetterStatus("Загрузка базы данных...");
            var qzi = await Googles.InitService();
            SetterStatus("База загружена...");
            var q = await GetGEOAsync();
            var qq = await init();
            while (true)
            {
                if (q == true && qq == true)
                {
                    //EndLoad();
                    SetterStatus("Проверка наличия неотправленного...");
                    string kk = Preferences.Get("Offline_data", "");
                    List<TableItem> ti = JsonConvert.DeserializeObject<List<TableItem>>(kk);
                    if (ti != null)
                    {
                        if (ti.Count > 0)
                        {
                            OfflineList ofl = new OfflineList(true);
                            ofl.SetTableData(ti);
                            StaticInfo.SetPage(ofl);
                            is_offline = true;
                        }
                    }
                    is_Loaded = true;
                    break;
                }
            }
            return true;
        }
        static public Dictionary<string, Location> GetCSV()
        {


            Dictionary<string, Location> data_import = new Dictionary<string, Location>();
            string results = string.Empty;

            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var filename = Path.Combine(path, "aopa-points-export.csv");
            var t = Preferences.Get("AopaTable", DateTime.MinValue);
            var q = (DateTime.Now - t).TotalDays;
            if (t == null || q > 60)
            {
                Preferences.Set("AopaTable", DateTime.Now);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(GetAopaPath);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                using (var reader = new StreamWriter(filename, false, System.Text.Encoding.UTF8))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        reader.WriteLine(line);
                        var values = line.Split(';');
                        double lat = 0;
                        double lon = 0;
                        lat = Convert.ToDouble(values[2], CultureInfo.InvariantCulture);
                        lon = Convert.ToDouble(values[1], CultureInfo.InvariantCulture);
                        data_import.Add(values[0], new Location(lat, lon));
                    }
                    reader.Close();
                    sr.Close();
                }
            }
            else
            {
                using (var reader = new StreamReader(filename, System.Text.Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(';');
                        double lat = 0;
                        double lon = 0;
                        lat = Convert.ToDouble(values[2], CultureInfo.InvariantCulture);
                        lon = Convert.ToDouble(values[1], CultureInfo.InvariantCulture);
                        data_import.Add(values[0], new Location(lat, lon));
                    }
                    reader.Close();
                }
            }
            return data_import;
        }
    }
}
