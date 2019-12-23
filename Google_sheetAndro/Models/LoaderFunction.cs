using Google_sheetAndro.Class;
using Google_sheetAndro.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
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
        public delegate void ClearMap();
        public static event ClearMap DoClearMap;
        public static bool fl_offline { get; set; }
        public static void callClearMap()
        {
            LoaderFunction.DoClearMap?.Invoke();
        }
        public static async Task<bool> GetGEOAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                var s = await Geolocation.GetLocationAsync(request);
                StaticInfo.Pos = s;
            }
            #region catch
            catch (FeatureNotSupportedException)
            {
                // Handle not supported on device exception

                //return fnsEx.Message;
            }
            catch (FeatureNotEnabledException)
            {
                // Handle not enabled on device exception

                //return fneEx.Message;
            }
            catch (PermissionException)
            {
                // Handle permission exception

                //return pEx.Message;
            }
            catch (Exception)
            {

                // Unable to get location
                //return ex.Message;
            }
            #endregion
            return true;
        }
        public static async Task<bool> init()
        {
            try
            {
                if (string.IsNullOrEmpty(StaticInfo.Place))
                    await StaticInfo.GetPlace(StaticInfo.Pos.Latitude, StaticInfo.Pos.Longitude);
                if (StaticInfo.Wheather == null)
                    await StaticInfo.GetWeatherReqAsync(StaticInfo.Pos);
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
        public static void CreRow()
        {
            DoCreateRow?.Invoke();
        }
        public static void EndLoad()
        {
            ItemsInfoPage.Title = "Записи";
            LoaderFunction.MenuPage.sett(LoaderFunction.ItemsInfoPage);
            //DoWheatherLoad?.Invoke();
            if(MenuPage != null && StaticInfo.AccountEmail != null)
            {
                MenuPage.setImg(StaticInfo.AccountPicture,StaticInfo.AccountEmail);
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
        //NavigationPage outpg;
        private static async Task Loader()
        {
        }
        public static async Task InitialiserPage()
        {
            Googles.InitService();
            var q = await GetGEOAsync();
            var qq = await init();
            while (true)
            {
                if(q == true && qq == true)
                {
                    EndLoad();
                    break;
                }


            }

            //Task.Run(async () => await Loader()).Wait();

            //Task t1 = GetGEOAsync();

            //Task t2 = t1.ContinueWith(ct => init());
            //Task tWload = new Task(() => { WheatherPage = new WheatherView(); });
            //Task t = Task.WhenAll(new List<Task>() { t2, tWload }).ContinueWith(ct => EndLoad());
            //Task BackGround = tWload.ContinueWith(ct =>
            //{
            //    //not ok out items info
            //});
            //await Task.WhenAll(new[] { t, BackGround });

            //return true;
            //then load all info wheather and map and etc/
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
