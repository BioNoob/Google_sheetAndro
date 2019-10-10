using Android.Hardware;
using Google_sheetAndro.Class;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
/*
* ПОРЯДОК СБОРКИ
* https://docs.microsoft.com/ru-ru/xamarin/android/platform/maps-and-location/maps/maps-api#install-gpsmaps-nuget
* 1. ЗАШЛИ НА СТРАНИЦУ. КНОПКА ПОЛУЧИТЬ ПОГОДУ
* 2. КНОПКА СТАРТ. АКТИВИРУЕТ ЗАПИСЬ ТАЙМЕРА (ВЫВОД)
* 3. КАЖДУЮ МИНУТУ СТАВИТСЯ ПОЛУЧАЕМ ГЕОМЕТКУ ПО ТЕКУЩЕЙ ПОЗИЦИИ
* 3.1 АНАЛОГИЧНО СНИМАЕМ ПОКАЗАТЕЛЬ БАРОМЕТРА ЗА КАЖДУЮ МИНУТУ. (ЛИСТ ДАВЛЕНИЙ) (ИЛИ НЕ МИНУТУ А ПО АВТОМАТИЧЕСКОМУ ЗАНОСУ)
* 4. СЧИТАЕМ ДИСТАНЦИЮ МЕЖДУ ТОЧКАМИ. ЕСЛИ ДИСТАНЦИЯ БОЛЬШЕ 100М ДОБАВЛЯЕМ ТОЧКУ НА КАРТУ (ЛИСТ ТОЧЕК) double dist = location.CalculateDistance(new Location(), DistanceUnits.Kilometers) * 1000;
* 5. РИСУЕМ МАРКЕР,ЛИНИЮ ОТ ПОСЛЕДНЕЙ ДОБАВЛЕННОЙ ДО НОВОЙ
* 6. ЖМЕМ СТОП
* 7. СЧИТАЕМ ДИСТАНЦИЮ ОТ КАЖДОЙ ДО КАЖДОЙ ТОЧКИ ПО ПРЯМОЙ
* 8. ПОЛУЧАЕМ ВРЕМЯ НАЛЕТА КАК РАЗНИЦУ МЕЖДУ ДВУМЯ ОБЪЕКТАМИ КЛАССА ТАЙМЕР_Р
* 9. ПОЛУЧАЕМ ВЫСОТУ ПО РАЗНИЦЕ ДАВЛЕНИЯ С ПОГОДЫ И МАКСИМАЛЬНОЙ ИЗ ЛИСТА ДАВЛЕНИЙ
* 9. НАЖИМАЕМ КНОПКУ *К ЗАПИСИ* ПЕРЕНОСИМ ПУЛ ДАННЫХ НА ИТЕМС ПЕЙДЖ ДЛЯ ЗАПИСИ
*/

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {
            InitializeComponent();
            InitializeUiSettingsOnMap();

        }


        void InitializeUiSettingsOnMap()
        {
            map.UiSettings.MyLocationButtonEnabled = true;
            map.UiSettings.CompassEnabled = true;
            map.UiSettings.ZoomControlsEnabled = true;
            map.MyLocationEnabled = true;
            map.UiSettings.ZoomGesturesEnabled = true;
            map.UiSettings.MapToolbarEnabled = true;
        }





        public async Task<Location> getGEOAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    return location;//$"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}";
                }
                else
                    return null;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                return null;
                //return fnsEx.Message;
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                return null;
                //return fneEx.Message;
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                return null;
                //return pEx.Message;
            }
            catch (Exception ex)
            {
                return null;
                // Unable to get location
                //return ex.Message;
            }
        }

        public async Task<string> GetWeatherReqAsync(Location coord)
        {
            string api_key = "42b983a01370d4d851e3fccc2b3cfd4b";
            HttpClient client = new HttpClient();
            string req = $"?apikey={api_key}&q={coord.Latitude},{coord.Longitude}&language=ru-ru&details=true HTTP/1.1";
            HttpResponseMessage response = await client.GetAsync($"https://api.darksky.net/forecast/{api_key}/{coord.Latitude},{coord.Longitude}?&lang=ru&units=si&exclude=hourly,daily,minutely,flags");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                HttpContent responseContent = response.Content;
                var json = await responseContent.ReadAsStringAsync();
                dynamic stuff = JsonConvert.DeserializeObject(json);
                ResponsedData rpd = JsonConvert.DeserializeObject<ResponsedData>(json);
                //string pressure = stuff.currently.pressure; // давление, для рассчета высоты
                //string temperature = stuff.currently.temperature;//температура
                //string windspeed = stuff.currently.windSpeed;//скорость ветра
                //string cloud = stuff.currently.summary;//облачность
                //string time = stuff.currently.time; //время формата UNIX
                return "";
            }
            else
                return "";
        }
        void Barometer_ReadingChanged(object sender, BarometerChangedEventArgs e)
        {
            data = e.Reading;
            // Process Pressure
            Console.WriteLine($"Reading: Pressure: {data.PressureInHectopascals} hectopascals");
        }

        public void ToggleBarometer()
        {
            try
            {
                if (Barometer.IsMonitoring)
                    Barometer.Stop();
                else
                    Barometer.Start(speed);
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Other error has occurred.
            }
        }
        SensorSpeed speed = SensorSpeed.UI;
        BarometerData data;
        private async void GetBtn_Clicked(object sender, EventArgs e)
        {
            float myPres = 1016;
            Barometer.ReadingChanged += Barometer_ReadingChanged;
            //Barometer.Start(speed);
            //по нажатию кнопки старт просто запоминается время
            DateTime dt = DateTime.Now;
            //dt.ToUniversalTime делаем еще раз по нажатию, и получаем разницу по времени в секундах. Делим на 60, получаем минуты
            var answ = SensorManager.GetAltitude(950/*(float)data.PressureInHectopascals*/, myPres);
            //Barometer.Stop();
            //Location scoord = await getGEOAsync();
            //if(scoord != null)
            //{
            //    string kk = await GetWeatherReqAsync(scoord);
            //}

        }
    }
}