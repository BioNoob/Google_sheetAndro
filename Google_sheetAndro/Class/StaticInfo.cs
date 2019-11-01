using Android.Hardware;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Google_sheetAndro.Class
{
    public static class StaticInfo
    {
        private static string nalet;
        private static float height;
        private static double dist;
        private static ResponsedData wheather = null;

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

        public static Location Pos { get; set; }
        public static string Place { get; set; }
        public static ResponsedData Wheather
        {
            get { return wheather; }
            set
            {
                if (wheather == null)
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
        public static float BarWheather { get; set; } = 0;
        public static float GetHeight(double bardata)
        {
            if (bardata != 0)
            {
                Height = Math.Abs(SensorManager.GetAltitude(BarWheather, (float)bardata));
                return Height;
            }
            else
                return 0;
        }

        public static async Task GetWeatherReqAsync(Location coord)
        {
            string api_key = "42b983a01370d4d851e3fccc2b3cfd4b";
            HttpClient client = new HttpClient();
            string req = $"?apikey={api_key}&q={coord.Latitude},{coord.Longitude}&language=ru-ru&details=true HTTP/1.1";
            HttpResponseMessage response = await client.GetAsync($"https://api.darksky.net/forecast/{api_key}/{coord.Latitude},{coord.Longitude}?&lang=ru&units=si&exclude=hourly,daily,minutely,flags");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                HttpContent responseContent = response.Content;
                var json = await responseContent.ReadAsStringAsync();
                //dynamic stuff = JsonConvert.DeserializeObject(json);
                JObject obj = JObject.Parse(json);
                var t = obj.SelectToken("currently");
                //ResponsedData dd = new ResponsedData();
                //dd = t.ToObject<ResponsedData>();
                //Wheather = new ResponsedData();
                //var q = t.SelectToken("temperature");
                Wheather = t.ToObject<ResponsedData>();
                BarWheather = Wheather.pressure;
                //Wheather = JsonConvert.DeserializeObject<ResponsedData>(json);
                //string pressure = stuff.currently.pressure; // давление, для рассчета высоты
                //string temperature = stuff.currently.temperature;//температура
                //string windspeed = stuff.currently.windSpeed;//скорость ветра
                //string cloud = stuff.currently.summary;//облачность
                //string time = stuff.currently.time; //время формата UNIX
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
