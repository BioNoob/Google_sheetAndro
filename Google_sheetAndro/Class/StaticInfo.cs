using Android.Hardware;
using Android.Widget;
using Google_sheetAndro.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Google_sheetAndro.Class
{
    public static class StaticInfo
    {
        // // OAuth
        // // For Google login, configure at https://console.developers.google.com/
        // //public static string iOSClientId = "<insert IOS client ID here>";
        // public static string AndroidClientId = "502015475506-0g3ru4ej6h5h7vpv20m18rv5qapmo4vb.apps.googleusercontent.com";
        // public static string AppName = "Yasma";
        // // These values do not need changing
        // public static string Scope = "https://www.googleapis.com/auth/userinfo.email";
        // public static string AuthorizeUrl = "https://accounts.google.com/o/oauth2/auth";
        // public static string AccessTokenUrl = "https://www.googleapis.com/oauth2/v4/token";
        // public static string UserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";

        // // Set these to reversed iOS/Android client ids, with :/oauth2redirect appended
        //// public static string iOSRedirectUrl = "<insert IOS redirect URL here>:/oauth2redirect";
        // public static string AndroidRedirectUrl = "https://android-sheets-flybase.firebaseapp.com/__/auth/handler";



        private static string nalet;
        private static float height;
        private static double dist;
        private static ResponsedData wheather = null;
        private static string accountEmail;

        public delegate void MenuSetPage(Page pg);
        public static event MenuSetPage SetDetailPage;

        public delegate void ParamSetterN(string nalet);
        public static event ParamSetterN DoSetNalet;
        public delegate void ParamSetterH(int height);
        public static event ParamSetterH DoSetHeight;
        public delegate void ParamSetterD(double dist);
        public static event ParamSetterD DoSetDist;

        public delegate void ParamSetterC(string cloud_s);
        public static event ParamSetterC DoSetCloud;
        public delegate void ParamSetterW(int wind);
        public static event ParamSetterW DoSetWind;
        public delegate void ParamSetterT(string temp);
        public static event ParamSetterT DoSetTemp;
        public delegate void ActivityEnabler(bool status);
        public static event ActivityEnabler DoActiveAI;
        public static string AccountEmail { get => accountEmail; set => accountEmail = value; }
        private static string accountPicture;
        public static float BarWheather { get; set; } = 0;
        public static string AccountPicture { get => accountPicture; set { accountPicture = value; } }
        public static Location Pos { get; set; }
        public static string Place { get; set; }
        public static void SetPage(Page pg)
        {
            SetDetailPage?.Invoke(pg);
        }
        public static ResponsedData Wheather
        {
            get { return wheather; }
            set
            {
                //if (wheather == null)
                {
                    wheather = value;
                    DoSetCloud?.Invoke(CloudConverter());
                    DoSetWind?.Invoke((int)Math.Round(wheather.windSpeed));
                    DoSetTemp?.Invoke(wheather.temperature.ToString());
                }
            }
        }
        private static string CloudConverter()
        {
            if (wheather.cloudCover <= 0.0)
                return "нет";
            else if (wheather.cloudCover >= 0.1 && wheather.cloudCover <= 0.51)
                return "низкая";
            else
                return "высокая";
        }
        public static string Nalet
        {
            get { return nalet; }
            set
            {
                DoSetNalet?.Invoke(value);
                nalet = value;
            }
        }
        public static float Height
        {
            get { return height; }
            set { DoSetHeight?.Invoke((int)Math.Round(value)); height = value; }
        }
        public static double Dist
        {
            get { return dist; }
            set { DoSetDist?.Invoke(value); dist = value; }
        }


        public static float GetHeight(double bardata, bool fl = false)
        {
            if (bardata != 0)
            {
                if(!fl)
                {
                    Height = Math.Abs(SensorManager.GetAltitude(BarWheather, (float)bardata));
                }
                else
                {
                    return Math.Abs(SensorManager.GetAltitude(BarWheather, (float)bardata));
                }
                return Height;
            }
            else
                return 0;
        }

        public static async Task GetWeatherReqAsync(Location coord, CancellationToken cts )
        {
            string api_key = "42b983a01370d4d851e3fccc2b3cfd4b";
            HttpClient client = new HttpClient();
            string lat = coord.Latitude.ToString(CultureInfo.InvariantCulture);
            string lon = coord.Longitude.ToString(CultureInfo.InvariantCulture);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"https://api.darksky.net/forecast/{api_key}/{lat},{lon}?&lang=ru&units=si&exclude=hourly,daily,minutely,flags", cts);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    HttpContent responseContent = response.Content;
                    var json = await responseContent.ReadAsStringAsync();
                    JObject obj = JObject.Parse(json);
                    var t = obj.SelectToken("currently");
                    Wheather = t.ToObject<ResponsedData>();
                    if (Wheather == null)
                        throw new Exception("Ошибка погоды");
                    BarWheather = Wheather.pressure;
                    LoaderFunction.EndWheatherLoad();
                }
            }
            catch (Exception)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Toast.MakeText(Android.App.Application.Context, "Сервис погоды недоступен", ToastLength.Long).Show();
                });
            }
        }
        public static async Task GetWeatherReqAsync(Location coord)
        {
            string api_key = "42b983a01370d4d851e3fccc2b3cfd4b";
            HttpClient client = new HttpClient();
            string lat = coord.Latitude.ToString(CultureInfo.InvariantCulture);
            string lon = coord.Longitude.ToString(CultureInfo.InvariantCulture);
            HttpResponseMessage response = await client.GetAsync($"https://api.darksky.net/forecast/{api_key}/{lat},{lon}?&lang=ru&units=si&exclude=hourly,daily,minutely,flags");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                HttpContent responseContent = response.Content;
                var json = await responseContent.ReadAsStringAsync();
                JObject obj = JObject.Parse(json);
                var t = obj.SelectToken("currently");
                Wheather = t.ToObject<ResponsedData>();
                if (Wheather == null)
                    throw new Exception("Ошибка погоды");
                BarWheather = Wheather.pressure;
                LoaderFunction.EndWheatherLoad();
            }
        }
        public static async Task GetPlace(double lat, double lon)
        {

            string api_key = "29ed5507-cb7a-4652-8163-813f6637f991";
            HttpClient client = new HttpClient();
            string req = $"?apikey={api_key}&geocode={lat.ToString().Replace(',', '.')},{lon.ToString().Replace(',', '.')}&sco=latlong&kind=locality&results=1&format=json";
            HttpResponseMessage response = await client.GetAsync($"https://geocode-maps.yandex.ru/1.x/{req}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                HttpContent responseContent = response.Content;
                var json = await responseContent.ReadAsStringAsync();
                dynamic stuff = JsonConvert.DeserializeObject(json);
                JObject obj = JObject.Parse(json);
                try
                {
                    var a = obj["response"]["GeoObjectCollection"]["featureMember"].ToObject<IList<JObject>>()[0].SelectToken("GeoObject");
                    Place = a.SelectToken("description").ToString() + Environment.NewLine + a.SelectToken("name").ToString();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }

            }
        }
        public static void EnableAI(bool status)
        {
            DoActiveAI?.Invoke(status);
        }
    }
}
