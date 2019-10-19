using Android.Hardware;
using Google_sheetAndro.Class;
using Newtonsoft.Json;
using Plugin.DeviceSensors;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
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
            b2.IsEnabled = false;
        }
        bool isLoaded;
        protected async override void OnAppearing()
        {
            if (!isLoaded)
            {
                InitializeUiSettingsOnMap();
                isLoaded = true;
            }
        }
        async Task StartListening()
        {
            if (CrossDeviceSensors.Current.Barometer.IsSupported)
            {
                CrossDeviceSensors.Current.Barometer.OnReadingChanged += Barometer_OnReadingChanged;
                //CrossDeviceSensors.Current.Barometer.OnReadingChanged += (s, a) => {

                //};
                CrossDeviceSensors.Current.Barometer.StartReading();
            }
            else
            {
                StatusH.Text = "Нет барометра";
            }
            if (CrossGeolocator.Current.IsListening)
                return;
            CrossGeolocator.Current.DesiredAccuracy = 25;
           await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 10, true, new ListenerSettings
            {
                ActivityType = ActivityType.OtherNavigation,
                AllowBackgroundUpdates = true,
                DeferLocationUpdates = true,
                DeferralDistanceMeters = 10,
                DeferralTime = TimeSpan.FromSeconds(1),
                ListenForSignificantChanges = true,
                PauseLocationUpdatesAutomatically = false
            });

            CrossGeolocator.Current.PositionChanged += PositionChanged;
            CrossGeolocator.Current.PositionError += PositionError;
        }
        private void Barometer_OnReadingChanged(object sender, Plugin.DeviceSensors.Shared.DeviceSensorReadingEventArgs<double> e)
        {
            //hig = SensorManager.GetAltitude(,);
            //Plugin.Geolocator.Abstractions.Position s= new Plugin.Geolocator.Abstractions.Position();

            //Status.Text = Status.Text.Replace("Выс:", $"Выс: {} м");
            if (bar != e.Reading)
            {
                height = e.Reading;
                bar = e.Reading;
            }
            else
                bar = e.Reading;

            //System.Diagnostics.Debug.WriteLine(e.Reading);
        }
        public string address = string.Empty;
        private void PositionChanged(object sender, PositionEventArgs e)
        {
            Plugin.Geolocator.Abstractions.Position poss = new Plugin.Geolocator.Abstractions.Position(map.CameraPosition.Target.Latitude, map.CameraPosition.Target.Longitude);
            if (GeolocatorUtils.CalculateDistance(poss, e.Position, GeolocatorUtils.DistanceUnits.Kilometers) < 5)
            {
                var zoom = map.CameraPosition.Zoom;
                Xamarin.Forms.GoogleMaps.Position pos = new Xamarin.Forms.GoogleMaps.Position(e.Position.Latitude, e.Position.Longitude);
                if (pl.Positions.Count >= 1)
                {
                    Plugin.Geolocator.Abstractions.Position pss = new Plugin.Geolocator.Abstractions.Position(pl.Positions[pl.Positions.Count - 1].Latitude, pl.Positions[pl.Positions.Count - 1].Longitude);
                    if (GeolocatorUtils.CalculateDistance(pss, e.Position, GeolocatorUtils.DistanceUnits.Kilometers) * 1000 > 10)
                        SetLine(pos);

                }
                else if (pl.Positions.Count == 0)
                {
                    SetLine(pos);
                }
                map.MoveCamera(CameraUpdateFactory.NewPositionZoom(new Xamarin.Forms.GoogleMaps.Position(e.Position.Latitude, e.Position.Longitude), zoom));
            }

            //map.InitialCameraUpdate = CameraUpdateFactory.NewPosition(new Xamarin.Forms.GoogleMaps.Position(position.Latitude, position.Longitude));
        }

        private void PositionError(object sender, PositionErrorEventArgs e)
        {
            //Handle event here for errors
        }

        async Task StopListening()
        {
            if (!CrossGeolocator.Current.IsListening)
                return;
            CrossDeviceSensors.Current.Barometer.StopReading();
            await CrossGeolocator.Current.StopListeningAsync();
            CrossGeolocator.Current.PositionChanged -= PositionChanged;
            CrossGeolocator.Current.PositionError -= PositionError;
        }

        void InitializeUiSettingsOnMap()
        {
            map.UiSettings.MyLocationButtonEnabled = true;
            map.UiSettings.CompassEnabled = true;
            map.UiSettings.ZoomControlsEnabled = true;
            map.MyLocationEnabled = true;
            map.UiSettings.ZoomGesturesEnabled = true;
            map.UiSettings.MapToolbarEnabled = true;
            map.InitialCameraUpdate = CameraUpdateFactory.NewPositionZoom(new Xamarin.Forms.GoogleMaps.Position(55.751316, 37.620915), 11);

            map.MapLongClicked += map_MapLongClicked;
            pl.Tag = "Line";
            pl.StrokeWidth = 10;
            pl.StrokeColor = Color.Blue;
            //GetGEOAsync();
            //map.MoveToRegion(new Xamarin.Forms.GoogleMaps.MapSpan(new Xamarin.Forms.GoogleMaps.Position(),loc.Latitude,loc.Longitude));
        }
        private double _dist;
        public double dist 
        { 
            get
            {
                return _dist;
            }
            set
            {
                _dist = value;
                StaticInfo.Dist = dist;
            }
        }
        private double _height;
        private double bar;
        public double height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = StaticInfo.GetHeight(value);
                StatusH.Text = string.Format("{0:#0.0 м}", _height);
            }
        }
        private void map_MapLongClicked(object sender, MapLongClickedEventArgs e)
        {
            if (SwManual.IsToggled)
            {
                SetLine(e.Point);
            }
        }
        Polyline pl = new Polyline();
        private void SetLine(Xamarin.Forms.GoogleMaps.Position e)
        {
            if (pl.Positions.Count >= 1)
            {
                dist += GeolocatorUtils.CalculateDistance(new Plugin.Geolocator.Abstractions.Position(pl.Positions[pl.Positions.Count - 1].Latitude, pl.Positions[pl.Positions.Count - 1].Longitude),
                new Plugin.Geolocator.Abstractions.Position(e.Latitude, e.Longitude), GeolocatorUtils.DistanceUnits.Kilometers);
                StatusD.Text = $"{dist} км";
                pl.Positions.Add(e);
                map.Polylines.Clear();
                map.Polylines.Add(pl);
            }
            else
            {
                pl.Positions.Add(e);
                map.Pins.Add(new Pin() { Label = "Старт", Position = e });
            }
            //Polyline plm = ((List<Polyline>)map.Polylines).Find(t => t.Tag.ToString() == "Line");
            //Xamarin.Forms.GoogleMaps.Position pt = new Xamarin.Forms.GoogleMaps.Position(pl.Positions[pl.Positions.Count - 1].Latitude, pl.Positions[pl.Positions.Count - 1].Longitude);
            //((List<Polyline>)map.Polylines).Find(t => t.Tag.ToString() == "Line").Positions.Add(e);

        }


        public async void SetInitVew()
        {
            if (StaticInfo.Pos != null)
            {
                var animState = await map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(
                new CameraPosition(
                    new Xamarin.Forms.GoogleMaps.Position(StaticInfo.Pos.Latitude, StaticInfo.Pos.Longitude), // Tokyo
                    15d, // zoom
                    0)),
                TimeSpan.FromSeconds(2));
            }
        }
        public async Task<bool> GetGEOAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                var s = await Geolocation.GetLocationAsync(request);
                StaticInfo.Pos = s;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception

                //return fnsEx.Message;
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception

                //return fneEx.Message;
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception

                //return pEx.Message;
            }
            catch (Exception ex)
            {

                // Unable to get location
                //return ex.Message;
            }
            return true;
        }
        private async void SwManual_Toggled(object sender, ToggledEventArgs e)
        {
            if (await DisplayAlert("Предупреждение", "Текущий маршрут будет стёрт", "ОК", "Отммена"))
            {
                pl.Positions.Clear();
                map.Pins.Clear();
                map.Polylines.Clear();
            }
            else
            {
                SwManual.Toggled -= SwManual_Toggled;
                SwManual.IsToggled = !e.Value;
                SwManual.Toggled += SwManual_Toggled;
            }
        }
        private void b1_Clicked(object sender, EventArgs e)
        {
            start();
            b2.IsEnabled = true;
            b1.IsEnabled = false;
            Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
        }
        bool alive = true;
        Time_r t = new Time_r();
        private bool OnTimerTick()
        {
                t.Sec++;
                StatusTime.Text = t.ToString();
                return alive;
        }

        private void TimeSt()
        {
            if (alive == true)
            {
                alive = false;
            }
            else
            {
                alive = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
            }
        }
        private async void start()
        {
            await StartListening();
            TimeSt();
        }
        private async void Button_Clicked(object sender, EventArgs e)
        {
           await StopListening();
            b2.IsEnabled = false;
            b1.IsEnabled = true;
            TimeSt();
            StaticInfo.Nalet = t.ToString();
        }
    }
}