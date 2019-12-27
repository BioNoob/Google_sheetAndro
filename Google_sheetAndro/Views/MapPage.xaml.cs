using Android.Widget;
using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using Newtonsoft.Json;
using Plugin.DeviceSensors;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
        const double OffsetPos = 0.001347153801;
        private double OffsetCalc
        {
            get
            {
                return ((OffsetPos * 0.5) * 15/map.CameraPosition.Zoom);
                //return (OffsetPos * (map.CameraPosition.Zoom * 0.5) / 15);
            }
        }

        private MapObjects mapObjects;
        public bool Is_base { get; set; }
        public MapObjects MapObj { get { if(mapObjects==null)ClearMap(); SerToJsonMapData(); return mapObjects; } }
        private MapObjects[] History { get; set; }
        bool? fl_handle_ok_to_edit { get; set; }
        public MapPage(bool single = false)
        {
            InitializeComponent();
            Is_base = single;
            map.PinClicked += Map_PinClicked;
            init();
            mapObjects = new MapObjects();
            fl_handle_ok_to_edit = null;
            History = new MapObjects[10];
            //b2.IsEnabled = false;
            LoaderFunction.DoSetView += SetInitVew;
            LoaderFunction.DoClearMap += ClearMap;
            //PopSettings.Clicked += PopSettings_Clicked;
        }
        Xamarin.Forms.GoogleMaps.Position ToinitPos = new Xamarin.Forms.GoogleMaps.Position();
        bool fl = false;
        double cur_pos_w1;
        double cur_pos_w2;
        double cur_pos_h2;
        private double _dist;
        private double _height;
        private double bar;
        bool isLoaded;
        public bool fl_run = false;
        bool fl_USE_MAP_CLICK = true; // в настройки добавить чекбокс использовать маркеры в маршрутах
        bool fl_route = true;
        public string address = string.Empty;
        Time_r t = new Time_r();
        private bool alife = false;
        public double height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = StaticInfo.GetHeight(value, Is_base);
                if (Is_base)
                {
                    switch (fl_handle_ok_to_edit)
                    {
                        case null:
                            DispMes(false);
                            break;
                        case false:
                            LoaderFunction.ItemsPageAlone.SetHeight((int)_height);
                            StatusH.Text = string.Format("{0:#0.0 м}", _height);
                            break;
                        case true:
                            StatusH.Text = string.Format("{0:#0.0 м}", _height);
                            break;
                    }
                }
                else
                {
                    StatusH.Text = string.Format("{0:#0.0 м}", _height);
                }

            }
        }
        //КНОПКА ПЕРЕРАСЧЕТ ПУТИ?
        public double dist
        {
            get
            {
                return _dist;
            }
            set
            {
                _dist = value;
                if (!Is_base)
                {
                    StaticInfo.Dist = _dist;
                    StatusD.Text = string.Format("{0:#0.0} км", _dist);
                }
                else
                {
                    switch (fl_handle_ok_to_edit)
                    {
                        case null:
                            DispMes(true);
                            break;
                        case false:
                            LoaderFunction.ItemsPageAlone.SetDist(_dist);
                            StatusD.Text = string.Format("{0:#0.0} км", _dist);
                            break;
                        case true:
                            StatusD.Text = string.Format("{0:#0.0} км", _dist);
                            break;
                    }
                }
            }
        }
        public void ClearMap()
        {
            //if (Is_base)
            {
                mapObjects = new MapObjects();
                if (map.Polylines.Count > 0)
                {
                    mapObjects.Pins = map.Pins.ToList();
                    mapObjects.Polyline = map.Polylines.First();
                }
                map.Pins.Clear();
                map.Polylines.Clear();
                pl = new Polyline() { Tag = "Line", StrokeWidth = 10, StrokeColor = Color.Blue };
                SetDSetH(0, 0);
                History = new MapObjects[10];
                ToinitPos = new Xamarin.Forms.GoogleMaps.Position();
            }
        }
        private async void init()
        {
            var route_type = await SecureStorage.GetAsync("route");
            var map_type = await SecureStorage.GetAsync("map");
            var switch_s = await SecureStorage.GetAsync("switch");
            bool kk = Preferences.Get("SwitchValue", false);
            SetToPinRoute.IsToggled = kk;
            MapTypePick.Items.Add("Гибридная");
            MapTypePick.Items.Add("Схема");
            RouteTypePick.Items.Add("Маршрут");
            RouteTypePick.Items.Add("Точки");
            if (map_type != null && route_type != null)
            {
                MapTypePick.SelectedIndex = Convert.ToInt32(map_type);
                RouteTypePick.SelectedIndex = Convert.ToInt32(route_type);
            }
            else
            {
                await SecureStorage.SetAsync("route", "0");
                await SecureStorage.SetAsync("map", "0");
                MapTypePick.SelectedIndex = 0;
                RouteTypePick.SelectedIndex = 0;
            }
            map.PinDragEnd += Map_PinDragEnd;
            map.PinDragStart += Map_PinDragStart;
            map.PinDragging += Map_PinDragging;
        }

        private void Map_PinDragging(object sender, PinDragEventArgs e)
        {
            //map.

            int m = map.Pins.IndexOf(e.Pin);
            map.Pins.ElementAt(m).Position = new Xamarin.Forms.GoogleMaps.Position(e.Pin.Position.Latitude - (OffsetCalc), e.Pin.Position.Longitude);
            //if(map.Pins.Contains(DragPin))
            //{
            //    map.Pins.Remove(DragPin);
            //}
            //map.Pins.Add(pin);
        }

        private void Map_PinDragStart(object sender, PinDragEventArgs e)
        {
            //DragPin = e.Pin;

            int m = map.Pins.IndexOf(e.Pin);
            var l = map.Pins.Where(t => t.Label == e.Pin.Label).SingleOrDefault();
            var p = new Xamarin.Forms.GoogleMaps.Position(e.Pin.Position.Latitude - (OffsetCalc), e.Pin.Position.Longitude);

            //map.Pins.ElementAt(m).Position = p;

            if (map.Polylines.Count > 0)
            {
                //int k = map.Polylines.First().Positions.IndexOf(DragPin.Position);
                //if (k >= 0)
                //{
                //    map.Polylines.First().Positions.RemoveAt(k);
                //    map.Polylines.First().Positions.Insert(k, e.Pin.Position);
                //}
            }


        }
        private void Map_PinDragEnd(object sender, PinDragEventArgs e)
        {
            int m = map.Pins.IndexOf(e.Pin);
            map.Pins.ElementAt(m).Position = new Xamarin.Forms.GoogleMaps.Position(e.Pin.Position.Latitude - (OffsetCalc), e.Pin.Position.Longitude);
            //map.Pins.Select(t => DragPin);
            //throw new NotImplementedException();
        }

        //проверка на нулли
        private void SerToJsonMapData()
        {
            //mapObjects = new MapObjects();
            if (map.Polylines.Count > 0)
            {
                mapObjects.Polyline = map.Polylines.First();
            }
            if (map.Pins.Count > 0)
            {
                mapObjects.Pins = map.Pins.ToList();
            }

        }
        private bool setter_route(string Route)
        {
            mapObjects.Polyline = JsonConvert.DeserializeObject<Polyline>(Route);
            if (mapObjects.Polyline != null)
            {
                foreach (var item in MapObj.Polyline.Positions)
                {
                    pl.Positions.Add(item);
                }
                map.Polylines.Add(pl);
                //pl.Positions = mapObjects.Polyline.Positions;
                //dist = CalcDistForLine(pl);
            }
            else
                return false;
            return true;
        }
        public void AbsSetter(string Route, string Points)
        {
            MapObjects mo;
            setter_point(Points);
            if (setter_route(Route))
            {
                mo = new MapObjects(map.Pins.ToList(), map.Polylines.First());
            }
            else
                mo = new MapObjects() { Pins = map.Pins.ToList() };
            SaveToHist(mo);

        }
        private bool setter_point(string Point)
        {
            mapObjects.Pins = JsonConvert.DeserializeObject<List<Pin>>(Point);
            if (mapObjects.Pins != null)
            {
                foreach (var item in mapObjects.Pins)
                {
                    map.Pins.Add(item);
                }
                ToinitPos = map.Pins.Last().Position;
            }
            return true;
        }
        private double CalcDistForLine(Polyline ple)
        {
            for (int i = 0; i < ple.Positions.Count; i++)
            {
                var q = ple.Positions[i];
                var tt = ple.Positions[i++];
                dist += GeolocatorUtils.CalculateDistance(q.Latitude, q.Longitude, tt.Latitude, tt.Longitude, GeolocatorUtils.DistanceUnits.Kilometers);
            }
            return dist;
        }


        public void SetDSetH(double D, double H)
        {
            _dist = D;
            StatusD.Text = string.Format("{0:#0.0} км", _dist);
            _height = H;
            StatusH.Text = string.Format("{0:#0.0 м}", _height);
        }
        private void DispMes(bool fl_dist)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                fl_handle_ok_to_edit = await DisplayAlert("Предупреждение", "Сохранить имеющиеся данные о дистанции и высоте?", "Да", "Нет");
                if (fl_handle_ok_to_edit == false)
                {
                    if (fl_dist)
                    {
                        LoaderFunction.ItemsPageAlone.SetDist(_dist);
                        StatusD.Text = string.Format("{0:#0.0} км", _dist);
                    }
                    else
                    {
                        LoaderFunction.ItemsPageAlone.SetHeight((int)_height);
                        StatusH.Text = string.Format("{0:#0.0 м}", _height);
                    }

                }
            }
            );
        }
        //АНИМИРУЕТ ПО ШИРИНЕ
        private async void AnimateIn()
        {
            var animate = new Animation(d => r1.WidthRequest = d, r1.Width, SL.Width / 2, Easing.SinInOut);
            animate.Commit(r1, "BarGraph", 16, 500);
            var animate2 = new Animation(d => r1.HeightRequest = d, r1.Height, 260, Easing.SinInOut);
            animate2.Commit(r1, "BarGraph1", 16, 500);
            await PopSettings.TranslateTo(SL.Width / 2 - cur_pos_w2/*- r1.Bounds.Left - cur_pos_w2*/, PopSettings.Y /*- PopSettings.Height*/, 500, Easing.SinInOut);
            var animate3 = new Animation(d => Buttons.WidthRequest = d, Buttons.Width, SL.Width / 2, Easing.SinInOut);
            animate3.Commit(Buttons, "BarGraph2", 16, 100);
            var animate4 = new Animation(d => Buttons.HeightRequest = d, Buttons.Height, 260, Easing.SinInOut);
            animate4.Commit(Buttons, "BarGraph3", 16, 100);
            await Buttons.FadeTo(1, 1000, Easing.SinInOut);
            fl = !fl;
            //r1.TranslateTo(0, 0, 1200, Easing.SpringOut);
            //MainImage.LayoutTo(detailsRect, 1200, Easing.SpringOut);
            //BottomFrame.TranslateTo(0, 0, 1200, Easing.SpringOut);
            //Title.TranslateTo(0, 0, 1200, Easing.SpringOut);
            //ExpandBar.FadeTo(.01, 250, Easing.SinInOut);
        }
        private async void AnimateOut()
        {
            await Buttons.FadeTo(0, 700, Easing.SinInOut);
            var animate = new Animation(d => r1.WidthRequest = d, SL.Width / 2, cur_pos_w1 - r1.Margin.Right, Easing.SinInOut);
            animate.Commit(r1, "BarGraph", 16, 500);
            var animate2 = new Animation(d => r1.HeightRequest = d, 260, cur_pos_h2, Easing.SinInOut);
            animate2.Commit(r1, "BarGraph1", 16, 500);
            await PopSettings.TranslateTo(cur_pos_w2 - PopSettings.Width, PopSettings.Y, 500, Easing.SinInOut);
            var animate3 = new Animation(d => Buttons.WidthRequest = d, SL.Width / 2, 0, Easing.SinInOut);
            animate3.Commit(Buttons, "BarGraph2", 16, 100);
            var animate4 = new Animation(d => Buttons.HeightRequest = d, 260, 0, Easing.SinInOut);
            animate4.Commit(Buttons, "BarGraph3", 16, 100);


            fl = !fl;
            //MainImage.LayoutTo(expandedRect, 1200, Easing.SpringOut);
            //BottomFrame.TranslateTo(0, BottomFrame.Height, 1200, Easing.SpringOut);
            //Title.TranslateTo(-Title.Width, 0, 1200, Easing.SpringOut);
            //ExpandBar.FadeTo(1, 250, Easing.SinInOut);
        }
        //void PopSettings_Clicked(object sender, EventArgs e) => Popup?.ShowPopup(sender as View);
        private void PopSettings_Clicked(object sender, EventArgs e)
        {
            if (fl)
                AnimateOut();
            else
            {
                if (cur_pos_w1 == 0)
                {
                    cur_pos_w1 = r1.Bounds.Right;
                    cur_pos_w2 = PopSettings.Bounds.Right;
                    cur_pos_h2 = r1.Bounds.Bottom - 10; //10 = margin
                }
                AnimateIn();
            }
        }
        protected override async void OnAppearing()
        {

            if (!isLoaded)
            {
                InitializeUiSettingsOnMap();
                isLoaded = true;
            }
            if (Is_base)
            {
                SetInitVew(new Location(ToinitPos.Latitude, ToinitPos.Longitude));
            }
            else if (StaticInfo.Pos != null)
            {
                await Task.Delay(1000);
                SetInitVew(StaticInfo.Pos);
                //map.MoveToRegion(MapSpan.FromCenterAndRadius(new Xamarin.Forms.GoogleMaps.Position(StaticInfo.Pos.Latitude, StaticInfo.Pos.Longitude), Xamarin.Forms.GoogleMaps.Distance.FromMiles(5)));
                //map.MoveCamera(CameraUpdateFactory.NewPositionZoom(new Xamarin.Forms.GoogleMaps.Position(StaticInfo.Pos.Latitude,StaticInfo.Pos.Longitude), map.CameraPosition.Zoom));
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

        private void PositionChanged(object sender, PositionEventArgs e)
        {
            Plugin.Geolocator.Abstractions.Position poss = new Plugin.Geolocator.Abstractions.Position(map.CameraPosition.Target.Latitude, map.CameraPosition.Target.Longitude);
            if (GeolocatorUtils.CalculateDistance(poss, e.Position, GeolocatorUtils.DistanceUnits.Kilometers) < 25)//was 5 set 25 to Lost GeoPos
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
                string p = string.Format("{0:#0.#};{1:#0.#}", e.Position.Latitude, e.Position.Longitude, CultureInfo.InvariantCulture);
                Preferences.Set("LastKnownPosition", p);
            }


            //map.InitialCameraUpdate = CameraUpdateFactory.NewPosition(new Xamarin.Forms.GoogleMaps.Position(position.Latitude, position.Longitude));
        }

        private void PositionError(object sender, PositionErrorEventArgs e)
        {
            //Handle event here for errors
        }

        async Task<bool> StopListening()
        {
            if (!CrossGeolocator.Current.IsListening)
                return false;
            CrossDeviceSensors.Current.Barometer.StopReading();
            bool l = await CrossGeolocator.Current.StopListeningAsync();
            CrossGeolocator.Current.PositionChanged -= PositionChanged;
            CrossGeolocator.Current.PositionError -= PositionError;
            return l;
        }

        void InitializeUiSettingsOnMap()
        {
            map.UiSettings.MyLocationButtonEnabled = true;
            map.UiSettings.CompassEnabled = true;
            map.UiSettings.ZoomControlsEnabled = true;
            map.MyLocationEnabled = true;
            map.UiSettings.ZoomGesturesEnabled = true;
            map.UiSettings.MapToolbarEnabled = true;
            var pos = new Xamarin.Forms.GoogleMaps.Position();
            Debug.WriteLine($"Start pos init");
            Debug.WriteLine($"new point");
            Debug.WriteLine($"{pos.Latitude};{pos.Longitude}");
            if (ToinitPos != new Xamarin.Forms.GoogleMaps.Position())
            {
                pos = ToinitPos;
            }
            else
            {
                string buf = Preferences.Get("LastKnownPosition", "55.751316;37.620915");
                var op = buf.Split(';');
                pos = new Xamarin.Forms.GoogleMaps.Position(Convert.ToDouble(op[0]), Convert.ToDouble(op[1]));
            }
            Debug.WriteLine($"before go to new point");
            Debug.WriteLine($"{pos.Latitude};{pos.Longitude}");
            map.InitialCameraUpdate = CameraUpdateFactory.NewPositionZoom(pos, 11);
            map.MapLongClicked += map_MapLongClicked;
            pl.Tag = "Line";
            pl.StrokeWidth = 10;
            pl.StrokeColor = Color.Blue;
            //GetGEOAsync();
            //map.MoveToRegion(new Xamarin.Forms.GoogleMaps.MapSpan(new Xamarin.Forms.GoogleMaps.Position(),loc.Latitude,loc.Longitude));
        }

        private void map_MapLongClicked(object sender, MapLongClickedEventArgs e)
        {
            if (fl_route)
            {
                SetLine(e.Point);
            }
            else
            {
                SetPoint(e.Point);
            }
        }

        private void Map_PinClicked(object sender, PinClickedEventArgs e)
        {
            Xamarin.Forms.GoogleMaps.Position t;
            t = e.Pin.Position;
            if (fl_route)
            {
                if (fl_USE_MAP_CLICK)
                {
                    if (pl.Positions.Count >= 1)
                    {
                        dist += GeolocatorUtils.CalculateDistance(new Plugin.Geolocator.Abstractions.Position(pl.Positions[pl.Positions.Count - 1].Latitude, pl.Positions[pl.Positions.Count - 1].Longitude),
                       new Plugin.Geolocator.Abstractions.Position(t.Latitude, t.Longitude), GeolocatorUtils.DistanceUnits.Kilometers);
                        pl.Positions.Add(t);
                        map.Polylines.Clear();
                        map.Polylines.Add(pl);
                    }
                    else
                    {
                        pl.Positions.Add(t);
                    }
                }
            }
        }
        Polyline pl = new Polyline();

        private void SetPoint(Xamarin.Forms.GoogleMaps.Position e)
        {
            if (map.Pins.Count >= 1)
            {
                map.Pins.Add(new Pin() { Label = $"{map.Pins.Count - 1}", Position = e, IsDraggable = true, Icon = BitmapDescriptorFactory.DefaultMarker(Xamarin.Forms.Color.Blue) });
            }
            else
            {
                map.Pins.Add(new Pin() { Label = $"Start", Position = e, IsDraggable = true });
            }
            MapObjects mo = new MapObjects() { Pins = map.Pins.ToList() };
            if (map.Polylines.Count > 0)
            {
                mo.Polyline = map.Polylines.First();
            }
            SaveToHist(mo);
        }

        private void SetLine(Xamarin.Forms.GoogleMaps.Position e)
        {
            if (pl.Positions.Count >= 1)
            {
                dist += GeolocatorUtils.CalculateDistance(new Plugin.Geolocator.Abstractions.Position(pl.Positions[pl.Positions.Count - 1].Latitude, pl.Positions[pl.Positions.Count - 1].Longitude),
                new Plugin.Geolocator.Abstractions.Position(e.Latitude, e.Longitude), GeolocatorUtils.DistanceUnits.Kilometers);
                pl.Positions.Add(e);
                map.Polylines.Clear();
                map.Polylines.Add(pl);
                Pin pn;
                if (map.Pins.Count >= 1)
                {
                    if (map.Pins.Any(q => q.Label == "End"))
                    {
                        pn = map.Pins.Where(i => i.Label == "End").First();
                        map.Pins.Remove(pn);
                        pn.Position = e;
                        map.Pins.Add(pn);
                    }
                    else
                        map.Pins.Add(new Pin() { Label = "End", Position = e, IsDraggable = true });
                }
            }
            else
            {
                pl.Positions.Add(e);
                map.Pins.Add(new Pin() { Label = "Start", Position = e, IsDraggable = true });
            }
            MapObjects mo = new MapObjects();
            if (map.Polylines.Count > 0)
            {
                mo.Polyline = map.Polylines.First();
            }
            mo.Pins = map.Pins.ToList();
            SaveToHist(mo);
            //Polyline plm = ((List<Polyline>)map.Polylines).Find(t => t.Tag.ToString() == "Line");
            //Xamarin.Forms.GoogleMaps.Position pt = new Xamarin.Forms.GoogleMaps.Position(pl.Positions[pl.Positions.Count - 1].Latitude, pl.Positions[pl.Positions.Count - 1].Longitude);
            //((List<Polyline>)map.Polylines).Find(t => t.Tag.ToString() == "Line").Positions.Add(e);

        }


        public async void SetInitVew(Location location)
        {
            if (ToinitPos != new Xamarin.Forms.GoogleMaps.Position())
            {
                var animState = await map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(
                    new CameraPosition(
                        ToinitPos,//StaticInfo.Pos.Latitude, StaticInfo.Pos.Longitude), // Tokyo
                        15d, // zoom
                        0)),
                        TimeSpan.FromSeconds(2));
            }
            else
            {
                if (StaticInfo.Pos != null)
                {
                    var animState = await map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(
                    new CameraPosition(
                        new Xamarin.Forms.GoogleMaps.Position(location.Latitude, location.Longitude),//StaticInfo.Pos.Latitude, StaticInfo.Pos.Longitude), // Tokyo
                        15d, // zoom
                        0)),
                    TimeSpan.FromSeconds(2));
                }
            }

        }
        int chet_active_hist = 8;
        private MapObjects LoadFromHist()
        {

            var q = History[chet_active_hist];
            if (q== null)
            {
                return null;
            }
            else
            {
                chet_active_hist--;
                return q;
            }
        }
        private bool OnTimerTick()
        {
            t.Sec++;
            StatusTime.Text = t.ToString();
            //StaticInfo.Nalet = t.ToString();
            return alife;
        }
        private void SaveToHist(MapObjects obj)
        {
            Array.Copy(History, 1, History, 0, History.Length - 1);
            History[History.Length - 1] = obj;
            chet_active_hist = 8;
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
                //SwManual.Toggled -= SwManual_Toggled;
                //SwManual.IsToggled = !e.Value;
                //SwManual.Toggled += SwManual_Toggled;
            }
        }
        private async void b1_Clicked(object sender, EventArgs e)
        {
            //start();
            if (fl_run == false)
            {
                if (!string.IsNullOrWhiteSpace(StaticInfo.Nalet))
                {
                    bool kek = await DisplayAlert("Предупреждение", "Новая запись?", "Да", "Нет");
                    if (kek)
                    {
                        t.Sec = 0;
                        StaticInfo.Nalet = string.Empty;
                        map.Pins.Clear();
                        map.Polylines.Clear();
                    }
                }
                b1.Text = "Стоп";
                alife = true;
                fl_run = true;
                MapObjects mo = new MapObjects();
                if (map.Polylines.Count > 0)
                {
                    mo.Polyline = map.Polylines.First();
                }
                mo.Pins = map.Pins.ToList();
                SaveToHist(mo);
                Device.StartTimer(TimeSpan.FromSeconds(1), () => OnTimerTick());
                await StartListening();
            }
            else
            {
                bool kek2 = await StopListening();
                if(kek2)
                {
                    fl_run = false;
                    alife = false;
                    b1.Text = "Старт";
                    StaticInfo.Nalet = t.ToString();
                    if(map.Polylines.Count > 0)
                    {
                        Xamarin.Forms.GoogleMaps.Position pp = map.Polylines.First().Positions.Last();
                        map.Pins.Add(new Pin() { Label = "End", Position = pp, IsDraggable = true });
                        SaveToHist(new MapObjects(map.Pins.ToList(), map.Polylines.First()));
                    }
                }
            }
            //b2.IsEnabled = true;
            //b1.IsEnabled = false;
            //Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
        }
        protected override bool OnBackButtonPressed()
        {
            if(fl_run)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Toast.MakeText(Android.App.Application.Context, "Идет запись маршрута", ToastLength.Long).Show();
                });
                return true;
            }
            else
            {
                return base.OnBackButtonPressed();
            }
        }
        //private async void Button_Clicked(object sender, EventArgs e)
        //{
        //    await StopListening();
        //    //b2.IsEnabled = false;
        //    alife = false;
        //    //b1.IsEnabled = true;
        //    StaticInfo.Nalet = t.ToString();
        //    //tt.Stop();
        //    //TimeSt();
        //    //StaticInfo.Nalet = t.ToString();
        //}
        private async void CancelBtn_Clicked(object sender, EventArgs e)
        {
            await CancelBtn.FadeTo(0, 100);
            MapObjects mi = LoadFromHist();
            if(mi == null)
                Toast.MakeText(Android.App.Application.Context, "Нет сохранений в буфере", ToastLength.Long).Show();
            else
            {
                map.Pins.Clear();
                map.Polylines.Clear();
                if (mi.Polyline != null)
                    map.Polylines.Add(mi.Polyline);
                foreach (var item in mi.Pins)
                {
                    map.Pins.Add(item);
                }
            }

            //if (fl_route)
            //{
            //    if (map.Polylines.Count > 0)
            //    {
            //        if (map.Polylines.First().Positions.Count > 0) // ЕСЛИ СОВСЕМ НЕТУ ТО ФЕРСТ НЕ СПАСЕТ!
            //        {
            //            map.Polylines.First().Positions.RemoveAt(map.Polylines.First().Positions.Count - 1);
            //            pl.Positions.RemoveAt(pl.Positions.Count - 1);
            //            dist = CalcDistForLine(pl);
            //        }
            //    }
            //    if (map.Pins.Count > 1)
            //    {
            //        Pin pn = map.Pins.Where(i => i.Label == "End").First();
            //        map.Pins.Remove(pn);
            //        if (map.Polylines.Count > 0)
            //        {
            //            if (map.Polylines.First().Positions.Count > 0)
            //            {
            //                pn.Position = map.Polylines.First().Positions[map.Polylines.First().Positions.Count - 1];
            //                map.Pins.Add(pn);
            //            }
            //        }
            //    }
            //    else if (map.Pins.Count == 1)
            //    {
            //        map.Pins.Remove(map.Pins.Last());
            //    }
            //}
            //else
            //{
            //    if (map.Pins.Count > 0)
            //        map.Pins.Remove(map.Pins.Last());
            //}

            await CancelBtn.FadeTo(1, 100);
        }

        private async void ClearBtn_Clicked(object sender, EventArgs e)
        {
            await ClearBtn.FadeTo(0, 100);
            if (await DisplayAlert("Предупреждение", "Очистить карту", "Да", "Нет"))
            {
                map.Pins.Clear();
                map.Polylines.Clear();
                pl.Positions.Clear();
                dist = 0;
            }
            await ClearBtn.FadeTo(1, 100);
        }

        private async void RouteTypePick_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SecureStorage.SetAsync("route", RouteTypePick.SelectedIndex.ToString());
            switch (RouteTypePick.SelectedIndex)
            {
                case 0:
                    fl_route = true;
                    break;
                case 1:
                    fl_route = false;
                    break;
            }
        }

        private async void MapTypePick_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SecureStorage.SetAsync("map", MapTypePick.SelectedIndex.ToString());
            switch (MapTypePick.SelectedIndex)
            {
                case 0:
                    map.MapType = MapType.Hybrid;
                    break;
                case 1:
                    map.MapType = MapType.Street;
                    break;
            }
        }

        private void ReCalcDist_Clicked(object sender, EventArgs e)
        {
            if(map.Polylines.Count > 0)
                dist = CalcDistForLine(map.Polylines.First());
            else
                Toast.MakeText(Android.App.Application.Context, "Нет пути для рассчета", ToastLength.Long).Show();
        }

        private void SetToPinRoute_Toggled(object sender, ToggledEventArgs e)
        {
            fl_USE_MAP_CLICK = e.Value;
            Preferences.Set("SwitchValue",e.Value);
        }
        private void ColorSettings(object sender, EventArgs e)
        {
            PopUpDialog.ShowDialog();
            PopUpDialog.IsVisible = true;
            PopUpDialog.DialogClosed += PopUpDialog_DialogClosed;
        }

        private void PopUpDialog_DialogClosed(object sender, EventArgs e)
        {
            PopUpDialog.IsVisible = false;
        }
    }
}