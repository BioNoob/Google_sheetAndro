using Plugin.DeviceOrientation;
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
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;
using Plugin.DeviceOrientation.Abstractions;

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
//По умолчанию всегда две линии. Пустые с тагом хэндл и листнер
//сериализуем обе. Добавляем позиции в каждую линию сразу на карту. Привязка?
namespace Google_sheetAndro.Views
{
    public static class ShiftList
    {
        public static List<T> ShiftLeft<T>(this List<T> list, int shiftBy)
        {
            if (list.Count <= shiftBy)
            {
                return list;
            }

            var result = list.GetRange(shiftBy, list.Count - shiftBy);
            result.AddRange(list.GetRange(0, shiftBy));
            return result;
        }

        public static List<T> ShiftRight<T>(this List<T> list, int shiftBy)
        {
            if (list.Count <= shiftBy)
            {
                return list;
            }

            var result = list.GetRange(list.Count - shiftBy, shiftBy);
            result.AddRange(list.GetRange(0, list.Count - shiftBy));
            return result;
        }
        public static List<T> RepeatedDefault<T>(int count)
        {
            return Repeated(default(T), count);
        }
        public static List<T> Repeated<T>(T value, int count)
        {
            List<T> ret = new List<T>(count);
            ret.AddRange(Enumerable.Repeat(value, count));
            return ret;
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        const double OffsetPos = 0.001347153801;
        private double OffsetCalc
        {
            get
            {
                return ((OffsetPos * 0.5) * 15 / map.CameraPosition.Zoom);
                //return (OffsetPos * (map.CameraPosition.Zoom * 0.5) / 15);
            }
        }

        private MapObjects mapObjects;
        public bool Is_base { get; set; }
        public MapObjects MapObj { get { /*if (mapObjects == null) */SerToJsonMapData(); return mapObjects; } }
        private List<string> History { get; set; }
        bool? _fl_handle_ok_to_edit;
        bool? fl_handle_ok_to_edit { get => _fl_handle_ok_to_edit; set { _fl_handle_ok_to_edit = value; if (value != null) { savevalswitsh.IsToggled = (bool)value; } } }
        public MapPage(bool single = false)
        {
            InitializeComponent();
            this.BindingContext = this;
            Is_base = single;
            map.PinClicked += Map_PinClicked;
            init();
            mapObjects = new MapObjects();
            fl_handle_ok_to_edit = null;
            History = ShiftList.RepeatedDefault<string>(10);
            //b2.IsEnabled = false;
            LoaderFunction.DoSetView += SetInitVew;
            LoaderFunction.DoClearMap += ClearMap;
            //PopSettings.Clicked += PopSettings_Clicked;
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (s, e) => TapGestureRecognizer_Tapped(s, e);
            StatusD.GestureRecognizers.Add(tgr);
            Status_D.GestureRecognizers.Add(tgr);
            Status_D_handle.GestureRecognizers.Add(tgr);
            StatusD_handle.GestureRecognizers.Add(tgr);
            StatusH_av.GestureRecognizers.Add(tgr);
            Status_H_av.GestureRecognizers.Add(tgr);
            StatusH_m.GestureRecognizers.Add(tgr);
            Status_H_m.GestureRecognizers.Add(tgr);
            //StatusH.GestureRecognizers.Add(tgr);
            //Status_H.GestureRecognizers.Add(tgr);
            TapGestureRecognizer_Tapped(StatusD, null);
            TapGestureRecognizer_Tapped(StatusH_av, null);
            bufferpos = new Plugin.Geolocator.Abstractions.Position();
            this.Appearing += MapPage_Appearing;
            this.Disappearing += MapPage_Disappearing;
            StaticInfo.DoActiveAI += StaticInfo_DoActiveAI;
        }

        private void StaticInfo_DoActiveAI(bool status)
        {
            IsBusy = status;
        }

        private void MapPage_Disappearing(object sender, EventArgs e)
        {
            //if(CrossDeviceSensors.Current.Barometer.IsActive)
            //CrossDeviceSensors.Current.Barometer.StopReading();
        }
        private bool has_barometr = false;
        private void MapPage_Appearing(object sender, EventArgs e)
        {
            //if (CrossDeviceSensors.Current.Barometer.IsSupported)
            //{
            //    has_barometr = true;
            //    //CrossDeviceSensors.Current.Barometer.OnReadingChanged += Barometer_OnReadingChanged;
            //    //CrossDeviceSensors.Current.Barometer.StartReading();
            //}
            //else
            //{
            //    StatusH.Text = "Нет барометра";
            //}
        }
        private Plugin.Geolocator.Abstractions.Position bufferpos { get; set; }
        private double _speed = 0;
        public double speed { get { return _speed; } set { _speed = value; StatusS.Text = string.Format("{0:#0.0} км/ч", _speed); } }
        private enum TypeInput
        {
            HANDLE = 0,
            LISTEN
        }
        private TypeInput TypeLine { get; set; }
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Label tagSpan = (Label)sender;
            bool fl = false;
            switch (tagSpan.AutomationId)
            {
                case "Listen":
                    TypeLine = TypeInput.LISTEN;
                    fl = false;
                    Status_D.BackgroundColor = Color.FromHex("#900040ff");
                    StatusD.BackgroundColor = Color.FromHex("#900040ff");
                    StatusD_handle.BackgroundColor = Color.FromHex("#70000000");
                    Status_D_handle.BackgroundColor = Color.FromHex("#70000000");
                    //if (!Is_base)
                    //    StaticInfo.Dist = _dist;
                    //else
                    //    LoaderFunction.ItemsPageAlone.SetDist(_dist);
                    //map.PinDragEnd -= Map_PinDragEnd;
                    //map.PinDragStart -= Map_PinDragStart;
                    //map.PinDragging -= Map_PinDragging;
                    ActiveDistanse = tagSpan.AutomationId;
                    SetActiveDistLbl(fl);
                    break;
                case "Handle":
                    TypeLine = TypeInput.HANDLE;
                    StatusD_handle.BackgroundColor = Color.FromHex("#900040ff");
                    Status_D_handle.BackgroundColor = Color.FromHex("#900040ff");
                    StatusD.BackgroundColor = Color.FromHex("#70000000");
                    Status_D.BackgroundColor = Color.FromHex("#70000000");
                    fl = true;
                    //if (!Is_base)
                    //    StaticInfo.Dist = _dist_handle;
                    //else
                    //    LoaderFunction.ItemsPageAlone.SetDist(_dist_handle);
                    //map.PinDragEnd += Map_PinDragEnd;
                    //map.PinDragStart += Map_PinDragStart;
                    //map.PinDragging += Map_PinDragging;
                    ActiveDistanse = tagSpan.AutomationId;
                    SetActiveDistLbl(fl);
                    break;
                case "Nor_H":
                    //Status_H.BackgroundColor = Color.FromHex("#900040ff");
                    //StatusH.BackgroundColor = Color.FromHex("#900040ff");
                    StatusH_av.BackgroundColor = Color.FromHex("#70000000");
                    Status_H_av.BackgroundColor = Color.FromHex("#70000000");
                    Status_H_m.BackgroundColor = Color.FromHex("#70000000");
                    StatusH_m.BackgroundColor = Color.FromHex("#70000000");
                    SetActiveHightLbl(1);
                    break;
                case "Max_H":
                    //Status_H.BackgroundColor = Color.FromHex("#70000000");
                    //StatusH.BackgroundColor = Color.FromHex("#70000000");
                    StatusH_av.BackgroundColor = Color.FromHex("#70000000");
                    Status_H_av.BackgroundColor = Color.FromHex("#70000000");
                    Status_H_m.BackgroundColor = Color.FromHex("#900040ff");
                    StatusH_m.BackgroundColor = Color.FromHex("#900040ff");
                    SetActiveHightLbl(3);
                    break;
                case "Av_H":
                    //Status_H.BackgroundColor = Color.FromHex("#70000000");
                    //StatusH.BackgroundColor = Color.FromHex("#70000000");
                    StatusH_av.BackgroundColor = Color.FromHex("#900040ff");
                    Status_H_av.BackgroundColor = Color.FromHex("#900040ff");
                    Status_H_m.BackgroundColor = Color.FromHex("#70000000");
                    StatusH_m.BackgroundColor = Color.FromHex("#70000000");
                    SetActiveHightLbl(2);
                    break;
            }

        }
        string ActiveDistanse = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flag"> 1= handle 0= listen</param>
        private async void SetActiveDistLbl(bool flag)
        {
            if (flag)
            {
                setactiveDist(2);
                //Handle_imgbtn.IsVisible = true;
                await Handle_imgbtn.FadeTo(1, 700, Easing.SinInOut);
                await Handle_imgbtn.FadeTo(0, 700, Easing.SinInOut);

                //Handle_imgbtn.IsVisible = false;
            }
            else
            {
                setactiveDist(1);
                //Listen_imgbtn.IsVisible = true;
                await Listen_imgbtn.FadeTo(1, 700, Easing.SinInOut);
                await Listen_imgbtn.FadeTo(0, 700, Easing.SinInOut);

                //Listen_imgbtn.IsVisible = false;
            }
        }
        private async void SetActiveHightLbl(int flag)
        {
            setactiveHeight(flag);
            switch (flag)
            {
                case 1:
                    await Height_curr_imgbtn.FadeTo(1, 700, Easing.SinInOut);
                    await Height_curr_imgbtn.FadeTo(0, 700, Easing.SinInOut);
                    break;
                case 2:
                    await Height_max_imgbtn.FadeTo(1, 700, Easing.SinInOut);
                    await Height_max_imgbtn.FadeTo(0, 700, Easing.SinInOut);
                    //await Height_av_imgbtn.FadeTo(1, 700, Easing.SinInOut);
                    //await Height_av_imgbtn.FadeTo(0, 700, Easing.SinInOut);
                    break;
                case 3:
                    await Height_curr_imgbtn.FadeTo(1, 700, Easing.SinInOut);
                    await Height_curr_imgbtn.FadeTo(0, 700, Easing.SinInOut);
                    break;
            }


        }
        bool fl_already_shown_2 = false;
        string active_dist = string.Empty;
        enum ActHeght
        {
            current = 1,
            otnos = 2,
            max = 3
        }
        ActHeght CurHeight { get; set; }
        /// <summary>
        /// Выставить активную дистанцию
        /// </summary>
        /// <param name="_flag">1 = запись, 2 = ручн</param>
        private void setactiveDist(int _flag)
        {
            double act_d = 0;
            switch (_flag)
            {
                case 1:
                    act_d = dist;
                    active_dist = "Listen";
                    break;
                case 2:
                    act_d = dist_handle;
                    active_dist = "Handle";
                    break;
            }
            if (Is_base)
            {
                switch (fl_handle_ok_to_edit)
                {
                    case false:
                        LoaderFunction.ItemsPageAlone.SetDist(act_d);
                        break;
                    case true:
                        if (!fl_already_shown_2)
                        {
                            fl_already_shown_2 = true;
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                var t = await DisplayAlert("Предупреждение", "Включено сохранение дистанции\nВыставить всё равно?", "Да", "Нет");
                                if (t)
                                {
                                    LoaderFunction.ItemsPageAlone.SetDist(act_d);
                                }
                                fl_already_shown_2 = false;
                            }
                            );
                        }
                        break;
                }
            }
            else
            {
                StaticInfo.Dist = act_d;
                //StatusH.Text = string.Format("{0:#0.0 м}", _height);
            }
        }
        private void setactiveHeight(int _flag)
        {
            double act_h = 0;
            switch (_flag)
            {
                case 1:
                    act_h = height;
                    CurHeight = ActHeght.current;
                    break;
                case 2:
                    act_h = height_middle;
                    CurHeight = ActHeght.otnos;
                    break;
                case 3:
                    act_h = height_max;
                    CurHeight = ActHeght.max;
                    break;
            }
            if (Is_base)
            {
                switch (fl_handle_ok_to_edit)
                {
                    case false:
                        LoaderFunction.ItemsPageAlone.SetHeight((int)act_h);
                        break;
                    case true:
                        if (!fl_already_shown_2)
                        {
                            fl_already_shown_2 = true;
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                var t = await DisplayAlert("Предупреждение", "Включено сохранение высоты\nВыставить всё равно?", "Да", "Нет");
                                if (t)
                                {
                                    LoaderFunction.ItemsPageAlone.SetHeight((int)act_h);
                                }
                                fl_already_shown_2 = false;
                            }
                            );
                        }
                        break;
                }
            }
            else
            {
                StaticInfo.Height = act_h;
                //StatusH.Text = string.Format("{0:#0.0 м}", _height);
            }
        }

        Xamarin.Forms.GoogleMaps.Position ToinitPos = new Xamarin.Forms.GoogleMaps.Position();
        double Toinitzoom = 15d;
        double ToinitBear = 0;

        bool fl = false;
        double cur_pos_w1;
        double cur_pos_w2;
        double cur_pos_h2;
        private double _dist;
        private double _dist_handle;
        private double _height;
        private double bar;
        bool isLoaded;
        //int chet_active_hist = 8;
        public bool fl_run = false;
        bool fl_USE_MAP_CLICK = false; // в настройки добавить чекбокс использовать маркеры в маршрутах
        bool fl_route = true;
        public string address = string.Empty;
        public List<Polyline> MapLines
        {
            get
            {
                return map.Polylines.ToList();
            }
        }
        private Polyline _pl_handle;
        Polyline pl_handle
        {
            get
            {
                return _pl_handle;
            }
            set
            {
                _pl_handle = value;
            }
        }
        private Polyline _pl_listner;
        Polyline pl_transparent { get; set; }
        Polyline pl_listner
        {
            get
            {
                return _pl_listner;
            }
            set
            {
                _pl_listner = value;
            }
        }
        public Time_r times = new Time_r();
        private bool alife = false;
        private bool height_coord = false;
        public double height
        {
            get
            {
                return _height;
            }
            set
            {
                if (!height_coord)
                    _height = StaticInfo.GetHeight(value, Is_base);
                else
                    _height = value;
                if (Is_base)
                {
                    switch (fl_handle_ok_to_edit)
                    {
                        case null:
                            if (!fl_already_shown)
                            {
                                DispMes(false);
                            }
                            break;
                        case false:
                            if (CurHeight == ActHeght.max)
                                LoaderFunction.ItemsPageAlone.SetHeight((int)height_max);
                            else if (CurHeight == ActHeght.otnos)
                                LoaderFunction.ItemsPageAlone.SetHeight((int)height_middle);
                            //StatusH.Text = string.Format("{0:#0.0 м}", _height);
                            break;
                        case true:
                            //StatusH.Text = string.Format("{0:#0.0 м}", _height);
                            break;
                    }
                }
                else
                {
                    if (CurHeight == ActHeght.max)
                        StaticInfo.Height = (int)height_max;
                    else if (CurHeight == ActHeght.otnos)
                        StaticInfo.Height = (int)height_middle;
                    //StatusH.Text = string.Format("{0:#0.0 м}", _height);
                    height_list.Add(_height);
                }
                height_list.Add(_height);
                var a = height_middle;
                var b = height_max;
            }
        }
        List<double> height_list = new List<double>();
        double height_corective = 0;
        ///теперь это выоста относительная
        public double height_middle
        {
            get
            {
                StatusH_av.Text = string.Format("{0:#0.0 м}", height - height_corective);
                return (height - height_corective);
                //if (height_list.Count > 0)
                //{

                //    //StatusH_av.Text = string.Format("{0:#0.0 м}", height_list.Average());
                //    //return height_list.Average();
                //}
                //else return height;
            }
        }
        public double height_max
        {
            get
            {
                if (height_list.Count > 0)
                {
                    StatusH_m.Text = string.Format("{0:#0.0 м}", height_list.Max());
                    return height_list.Max();
                }
                else return _height;

            }
        }
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
                            if (!fl_already_shown)
                            {
                                DispMes(true, 1);
                            }
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
        public double dist_handle
        {
            get
            {
                return _dist_handle;
            }
            set
            {
                _dist_handle = value;
                if (!Is_base)
                {
                    StaticInfo.Dist = _dist_handle;
                    StatusD_handle.Text = string.Format("{0:#0.0} км", _dist_handle);
                }
                else
                {
                    switch (fl_handle_ok_to_edit)
                    {
                        case null:
                            if (!fl_already_shown)
                            {
                                DispMes(true, 2);
                            }
                            break;
                        case false:
                            LoaderFunction.ItemsPageAlone.SetDist(_dist_handle);
                            StatusD_handle.Text = string.Format("{0:#0.0} км", _dist_handle);
                            break;
                        case true:
                            StatusD_handle.Text = string.Format("{0:#0.0} км", _dist_handle);
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
                //if (map.Polylines.Count > 0)
                //{
                //    mapObjects.Pins = map.Pins.ToList();
                //    mapObjects.Polylines = MapLines;
                //}
                ////map.Pins.Clear();
                //foreach (var item in MapLines)
                //{
                //    item.Positions.Clear();
                //}
                SetDSetH(0, 0);
                History = ShiftList.RepeatedDefault<string>(10);
                ToinitPos = new Xamarin.Forms.GoogleMaps.Position();
                ToinitBear = 0;
                Toinitzoom = 15d;
                times = new Time_r();
                StatusTime.Text = times.ToString();
                height_list.Clear();
                fl_handle_ok_to_edit = true;
                map.Pins.Clear();
                map.Polylines.Clear();
                pl_handle.Positions.Clear();
                pl_listner.Positions.Clear();
                pl_transparent.Positions.Clear();
                dist = 0;
                dist_handle = 0;
                height = 0;
                StatusTime.Text = string.Empty;
                fl_handle_ok_to_edit = null;
                //mapObjects = new MapObjects();
            }
        }
        private async void init()
        {
            var route_type = await SecureStorage.GetAsync("route");
            var map_type = await SecureStorage.GetAsync("map");
            var switch_s = await SecureStorage.GetAsync("switch");
            fl_Bearing = Preferences.Get("fl_Bearing", fl_Bearing);
            OrientBlock.IsToggled = Preferences.Get("OrientBlock", false);
            if (fl_Bearing)
            {
                BerFl.Rotation = 0;
                BerHelp.BackgroundColor = Color.Orange;
            }
            else
            {
                BerFl.Rotation = -45;
                BerHelp.BackgroundColor = Color.Gray;
            }
            pl_handle = new Polyline() { Tag = "Handle", StrokeColor = Color.Red, StrokeWidth = 7 };
            pl_listner = new Polyline() { Tag = "Listner", StrokeColor = Color.Blue, StrokeWidth = 7 };
            pl_transparent = new Polyline() { Tag = "Transparent", StrokeColor = Color.Transparent, StrokeWidth = 7 };
            //bool kk = Preferences.Get("SwitchValue", false);
            //SetToPinRoute.IsToggled = kk;
            MapTypePick.Items.Add("Гибридная");
            MapTypePick.Items.Add("Схема");
            Is_set = true;
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
            Is_set = false;
            //MapLines = new List<Polyline>() {  },

            map.PinDragEnd += Map_PinDragEnd;
            map.PinDragStart += Map_PinDragStart;
            map.PinDragging += Map_PinDragging;
        }
        private void Map_PinDragging(object sender, PinDragEventArgs e)
        {
            //map.

            //int m = map.Pins.IndexOf(e.Pin);
            //map.Pins.ElementAt(m).Position = new Xamarin.Forms.GoogleMaps.Position(e.Pin.Position.Latitude - (OffsetCalc), e.Pin.Position.Longitude);
            //if(map.Pins.Contains(DragPin))
            //{
            //    map.Pins.Remove(DragPin);
            //}
            //map.Pins.Add(pin);
        }
        private string PinBuffLbl;
        private Xamarin.Forms.GoogleMaps.Position PinDeffY = new Xamarin.Forms.GoogleMaps.Position();
        private void Map_PinDragStart(object sender, PinDragEventArgs e)
        {
            if (e.Pin.Tag.ToString() != null)
            {
                PinBuffLbl = e.Pin.Tag.ToString();
                //Для отключения перетаскивания ручного маршрута!
                //var t = LoadFromHistActual(); 
                //var l = t.Pins.Find(q => q.Tag.ToString() == PinBuffLbl);
                //PinDeffY = l.Position;
            }
        }
        //ИСКЛЮЧИТЬ ЗАПИСАНОЕ ПЕРЕТАСКИВАНИЕ?
        private void Map_PinDragEnd(object sender, PinDragEventArgs e)
        {
            //int m = map.Pins.IndexOf(e.Pin);
            var t = LoadFromHistActual();
            var l = t.Pins.Find(q => q.Tag.ToString() == PinBuffLbl);
            if (l != null)
            {
                int buff = pl_handle.Positions.IndexOf(l.Position);
                if (buff >= 0)
                {
                    map.Polylines.Remove(pl_handle);
                    pl_handle.Positions.RemoveAt(buff);
                    pl_handle.Positions.Insert(buff, e.Pin.Position);
                    if (pl_handle.Positions.Count >= 2)
                        map.Polylines.Add(pl_handle);
                    //селектед тип редактирования свич, если точки то
                    if (fl_route)
                        dist_handle = CalcDistForLine(pl_handle);
                    MapObjects mo = new MapObjects();
                    if (MapLines.Count > 0)
                    {
                        mo.Polylines = MapLines;
                    }
                    mo.Pins = map.Pins.ToList();
                    SaveToHist(mo);
                }
                buff = pl_listner.Positions.IndexOf(l.Position);
                if (buff >= 0)
                {
                    map.Polylines.Remove(pl_listner);
                    pl_listner.Positions.RemoveAt(buff);
                    pl_listner.Positions.Insert(buff, e.Pin.Position);
                    if (pl_listner.Positions.Count >= 2)
                        map.Polylines.Add(pl_listner);
                    dist = CalcDistForLine(pl_listner);
                    MapObjects mo = new MapObjects();
                    if (MapLines.Count > 0)
                    {
                        mo.Polylines = MapLines;
                    }
                    mo.Pins = map.Pins.ToList();
                    SaveToHist(mo);
                }
                buff = pl_transparent.Positions.IndexOf(l.Position);
                if (buff >= 0)
                {
                    pl_transparent.Positions.RemoveAt(buff);
                    pl_transparent.Positions.Insert(buff, e.Pin.Position);
                    MapObjects mo = new MapObjects();
                    mo.Pins = map.Pins.ToList();
                    if (MapLines.Count > 0)
                    {
                        mo.Polylines = MapLines;
                    }
                    SaveToHist(mo);
                    //селектед тип редактирования свич, если точки то
                    if (!fl_route)
                        dist_handle = CalcDistForLine(pl_transparent);
                }
            }
            //Для отключения перетаскивания ручного маршрута!
            //if (PinDeffY != new Xamarin.Forms.GoogleMaps.Position())
            //{
            //    l.Position = PinDeffY;
            //    PinDeffY = new Xamarin.Forms.GoogleMaps.Position();
            //    Toast.MakeText(Android.App.Application.Context, "Перемещение точек записанного маршрута не поддерживается", ToastLength.Short).Show();
            //}
        }
        private void SerToJsonMapData()
        {
            if (map.Polylines.Count > 0)
            {
                mapObjects.Polylines = MapLines;
            }
            if (map.Pins.Count > 0)
            {
                mapObjects.Pins = map.Pins.ToList();
            }

        }
        public void TimeSet(string val)
        {
            times = new Time_r(val);
            StatusTime.Text = times.ToString();
        }
        public void SetHeight(double val)
        {
            bool? buf = fl_handle_ok_to_edit;
            fl_handle_ok_to_edit = true;
            height_coord = true;

            //height = val;
            
            height_coord = false;
            fl_handle_ok_to_edit = buf;
        }
        public void SetDist(double val)
        {
            bool? buf = fl_handle_ok_to_edit;
            fl_handle_ok_to_edit = true;
            switch (active_dist)
            {
                case "Handle":
                    double dist_tr = 0;
                    double dist_hand = 0;
                    if (pl_transparent.Positions.Count > 1)
                        dist_tr = CalcDistForLine(pl_transparent);
                    if (pl_handle.Positions.Count > 1)
                        dist_hand = CalcDistForLine(pl_handle);
                    if (Math.Abs((val - dist_hand)) > Math.Abs((val - dist_tr)))
                        //RouteTypePick_SelectedIndexChanged(null, new EventArgs());
                        RouteTypePick.SelectedIndex = 1;
                    else
                        RouteTypePick.SelectedIndex = 0;
                    dist_handle = val;
                    break;
                case "Listen":
                    dist = val;
                    break;
            }
            fl_handle_ok_to_edit = buf;
        }
        public async void AbsSetter(string Route, string Points)
        {
            MapObjects mo;
            mo = await setter_point(Points, Route);
            //if (setter_route(Route))
            //{
            //    mo = new MapObjects(map.Pins.ToList(), MapLines);
            //}
            //else
            //    mo = new MapObjects() { Pins = map.Pins.ToList() };
            SaveToHist(mo);

        }
        private bool setter_route(string Route)
        {
            mapObjects.Polylines = JsonConvert.DeserializeObject<List<Polyline>>(Route);
            if (mapObjects.Polylines != null)
            {
                pl_handle.Positions.Clear();
                pl_listner.Positions.Clear();
                fl_handle_ok_to_edit = true;
                foreach (var item in mapObjects.Polylines)
                {
                    foreach (var pos in item.Positions)
                    {
                        switch (item.Tag.ToString())
                        {
                            case "Handle":
                                //pl_handle.Positions.Add(pos);
                                SetLine(pos, true);
                                break;
                            case "Listner":
                                SetLine(pos, false);
                                //pl_listner.Positions.Add(pos);
                                break;
                        }
                    }
                }
                fl_handle_ok_to_edit = null;
            }
            else
                return false;
            return true;
        }
        private void short_setline(Polyline pl)
        {
            foreach (var pos in pl.Positions)
            {
                //if (pl.Positions.Count >= 2)
                //{
                if (map.Polylines.Any(t => t.Tag.ToString() == pl.Tag.ToString()))
                {
                    map.Polylines.Remove(map.Polylines.Where(t => t.Tag.ToString() == pl.Tag.ToString()).First());
                }
                //  map.Polylines.Add(pl);
                //}
                //map.Polylines.Where(t => t.Tag.ToString() == pl.Tag.ToString()).First().Positions.Add(pos);
            }
        }
        private async Task<MapObjects> setter_point(string Point, string Route, bool not_del = false)
        {
            mapObjects.Pins = JsonConvert.DeserializeObject<List<Pin>>(Point);
            mapObjects.Polylines = JsonConvert.DeserializeObject<List<Polyline>>(Route);
            fl_handle_ok_to_edit = true;
            if (mapObjects.Polylines != null)
            {

                foreach (var item in mapObjects.Polylines)
                {
                    if (item.Tag.ToString() == "Handle")
                    {
                        if (!not_del)
                            pl_handle.Positions.Clear();
                        foreach (var pos in item.Positions)
                        {
                            pl_handle.Positions.Add(pos);
                        }
                        if (pl_handle.Positions.Count >= 2)
                            map.Polylines.Add(pl_handle);
                        //short_setline(pl_handle);

                        dist_handle = CalcDistForLine(pl_handle);
                        //await Task.Delay(100);

                    }
                    else if (item.Tag.ToString() == "Listner")
                    {
                        if (!not_del)
                            pl_listner.Positions.Clear();
                        //pl_listner = item;
                        foreach (var pos in item.Positions)
                        {
                            pl_listner.Positions.Add(pos);
                        }
                        if (pl_listner.Positions.Count >= 2)
                            map.Polylines.Add(pl_listner);
                        //foreach (var pos in item.Positions)
                        //{
                        //    map.Polylines.Last().Positions.Add(pos);
                        //}
                        dist = CalcDistForLine(pl_listner);
                        //await Task.Delay(100);
                    }
                }
            }
            if (mapObjects.Pins != null)
            {
                foreach (var item in mapObjects.Pins)
                {
                    var _icon = BitmapDescriptorFactory.DefaultMarker(Xamarin.Forms.Color.DeepSkyBlue);
                    if (item.Tag != null)
                        if (item.Tag.ToString().Contains("Start_") | item.Tag.ToString().Contains("End_"))
                        {
                            _icon = BitmapDescriptorFactory.DefaultMarker(Xamarin.Forms.Color.Red);
                        }
                    item.Icon = _icon;
                    map.Pins.Add(item);
                    if (item.Tag.ToString().Contains("Transparent"))
                    {
                        pl_transparent.Positions.Add(item.Position);
                    }
                }
                ToinitPos = map.Pins.Last().Position;
            }
            if (pl_transparent.Positions.Count > 0)
            {
                if (!fl_route)
                {
                    dist_handle = CalcDistForLine(pl_transparent);
                }
            }
            fl_handle_ok_to_edit = null;
            await Task.Delay(100);
            return mapObjects;
            //mapObjects.Pins = JsonConvert.DeserializeObject<List<Pin>>(Point);
            //List<Polyline> asdf = JsonConvert.DeserializeObject<List<Polyline>>(Route);
            //mapObjects.Polylines = asdf;
            //if (mapObjects.Polylines != null)
            //{
            //    pl_handle.Positions.Clear();
            //    pl_listner.Positions.Clear();
            //    fl_handle_ok_to_edit = true;
            //    foreach (var item in mapObjects.Polylines)
            //    {
            //        foreach (var pos in item.Positions)
            //        {
            //            switch (item.Tag.ToString())
            //            {
            //                case "Handle":
            //                    //pl_handle.Positions.Add(pos);
            //                    SetLine(pos, true);
            //                    break;
            //                case "Listner":
            //                    SetLine(pos, false);
            //                    //pl_listner.Positions.Add(pos);
            //                    break;
            //            }
            //        }
            //    }
            //    fl_handle_ok_to_edit = null;
            //}
            //if (mapObjects.Pins != null)
            //{
            //    foreach (var item in mapObjects.Pins)
            //    {
            //        if (asdf != null)
            //        {
            //            if (asdf.Count >= 2)
            //            {
            //                if (!asdf[0].Positions.Contains(item.Position) && !asdf[1].Positions.Contains(item.Position))
            //                {
            //                    map.Pins.Add(item);
            //                }
            //            }
            //            else if (asdf.Count == 1)
            //            {
            //                if (!asdf[0].Positions.Contains(item.Position))
            //                {
            //                    map.Pins.Add(item);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            map.Pins.Add(item);
            //        }
            //    }
            //    if (map.Pins.Count < 1)
            //    {
            //        if (asdf != null)
            //        {
            //            ToinitPos = asdf[0].Positions.Last();
            //        }
            //    }
            //    else
            //        ToinitPos = map.Pins.Last().Position;
            //}
            //return true;
        }
        private double CalcDistForLine(Polyline ple)
        {
            double buff = 0;
            int chet = 0;
            for (int i = 0; i < ple.Positions.Count - 1; i++)
            {
                var q = ple.Positions[chet];
                chet++;
                var tt = ple.Positions[chet];
                buff += GeolocatorUtils.CalculateDistance(q.Latitude, q.Longitude, tt.Latitude, tt.Longitude, GeolocatorUtils.DistanceUnits.Kilometers);
            }
            return buff;
        }
        public void SetDSetH(double D, double H)
        {
            //_dist = D;
            //StatusD.Text = string.Format("{0:#0.0} км", _dist);
            bool? buf = fl_handle_ok_to_edit;
            fl_handle_ok_to_edit = null;
            if (Math.Abs(_dist - D) > Math.Abs(_dist_handle - D))
            {
                TapGestureRecognizer_Tapped(StatusD_handle, null);

            }
            else if (_dist == _dist_handle)
            {
                TapGestureRecognizer_Tapped(StatusD, null);
            }
            else
            {
                TapGestureRecognizer_Tapped(StatusD, null);
                //setactiveDist(1);
            }
            SetDist(D);
            //fl_handle_ok_to_edit = buf;
            //
            fl_handle_ok_to_edit = true;
            SetHeight(H);
            //height_list.Add(H);
            fl_handle_ok_to_edit = buf;
            //StatusH.Text = string.Format("{0:#0.0 м}", _height);
        }
        private bool fl_already_shown = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fl_dist"></param>
        /// <param name="dist_type">1 = listen, 2 = handle</param>
        private async void DispMes(bool fl_dist, int dist_type = 1)
        {
            fl_already_shown = true;
            //Device.BeginInvokeOnMainThread(async () =>
            //{
            fl_handle_ok_to_edit = await DisplayAlert("Предупреждение", "Сохранять имеющиеся данные о дистанции/высоте?", "Да", "Нет");
            if (fl_handle_ok_to_edit == false)
            {
                if (fl_dist)
                {
                    switch (dist_type)
                    {
                        case 1:
                            LoaderFunction.ItemsPageAlone.SetDist(_dist);
                            StatusD.Text = string.Format("{0:#0.0} км", _dist);
                            break;
                        case 2:
                            LoaderFunction.ItemsPageAlone.SetDist(_dist_handle);
                            StatusD_handle.Text = string.Format("{0:#0.0} км", _dist_handle);
                            break;
                    }

                }
                else
                {
                    LoaderFunction.ItemsPageAlone.SetHeight((int)_height);
                    //StatusH.Text = string.Format("{0:#0.0 м}", _height);
                }

            }
            //}
            //);
            fl_already_shown = false;
        }
        private async void AnimateIn()
        {
            //var animate = new Animation(d => r1.WidthRequest = d, r1.Width, SL.Width / 2, Easing.SinInOut);
            //animate.Commit(r1, "BarGraph", 16, 500);
            //var animate2 = new Animation(d => r1.HeightRequest = d, r1.Height, 260, Easing.SinInOut);
            //animate2.Commit(r1, "BarGraph1", 16, 500);
            //await PopSettings.TranslateTo(SL.Width / 2 - cur_pos_w2/*- r1.Bounds.Left - cur_pos_w2*/, PopSettings.Y /*- PopSettings.Height*/, 500, Easing.SinInOut);
            //var animate3 = new Animation(d => Buttons.WidthRequest = d, Buttons.Width, SL.Width / 2, Easing.SinInOut);
            //animate3.Commit(Buttons, "BarGraph2", 16, 100);
            //var animate4 = new Animation(d => Buttons.HeightRequest = d, Buttons.Height, 260, Easing.SinInOut);
            //animate4.Commit(Buttons, "BarGraph3", 16, 100);
            //await Buttons.FadeTo(1, 1000, Easing.SinInOut);
            fl = !fl;
        }
        private async void AnimateOut()
        {
            //await Buttons.FadeTo(0, 700, Easing.SinInOut);
            //var animate = new Animation(d => r1.WidthRequest = d, SL.Width / 2, cur_pos_w1 - r1.Margin.Right, Easing.SinInOut);
            //animate.Commit(r1, "BarGraph", 16, 500);
            //var animate2 = new Animation(d => r1.HeightRequest = d, 260, cur_pos_h2, Easing.SinInOut);
            //animate2.Commit(r1, "BarGraph1", 16, 500);
            //await PopSettings.TranslateTo(cur_pos_w2 - PopSettings.Width, PopSettings.Y, 500, Easing.SinInOut);
            //var animate3 = new Animation(d => Buttons.WidthRequest = d, SL.Width / 2, 0, Easing.SinInOut);
            //animate3.Commit(Buttons, "BarGraph2", 16, 100);
            //var animate4 = new Animation(d => Buttons.HeightRequest = d, 260, 0, Easing.SinInOut);
            //animate4.Commit(Buttons, "BarGraph3", 16, 100);
            fl = !fl;
        }
        private void PopSettings_Clicked(object sender, EventArgs e)
        {
            if (fl)
                AnimateOut();
            else
            {
                if (cur_pos_w1 == 0)
                {
                    //cur_pos_w1 = r1.Bounds.Right;
                    //cur_pos_w2 = PopSettings.Bounds.Right;
                    //cur_pos_h2 = r1.Bounds.Bottom - 10; //10 = margin
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
                await Task.Delay(1000);
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

        public void Barometer_OnReadingChanged(object sender, Plugin.DeviceSensors.Shared.DeviceSensorReadingEventArgs<double> e)
        {
            //hig = SensorManager.GetAltitude(,);
            //Plugin.Geolocator.Abstractions.Position s= new Plugin.Geolocator.Abstractions.Position();
            if (!Is_base | fl_run)
            {
                if (CrossDeviceSensors.Current.Barometer.IsSupported)
                {
                    has_barometr = true;
                }
                else
                {
                    //StatusH.Text = "Нет барометра";
                }
                //Status.Text = Status.Text.Replace("Выс:", $"Выс: {} м");
                if (bar != e.Reading)
                {
                    height = e.Reading;
                    bar = e.Reading;
                }
                else
                    bar = e.Reading;
            }
            //System.Diagnostics.Debug.WriteLine(e.Reading);
        }

        private async void HelpMap()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Location location = new Location();
            cts.CancelAfter(900);
            try
            {
                if (!ps_listner)
                    throw new TaskCanceledException();
                var request = new GeolocationRequest(GeolocationAccuracy.High);
                location = await Geolocation.GetLocationAsync(request, cts.Token);
            }
            catch (TaskCanceledException)       // if the operation is cancelled, do nothing
            {
                ps_listner = false;
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                location = await Geolocation.GetLocationAsync(request);
                ps_listner = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), () => PositionChanged_timer());
            }
            finally
            {
                var zoom = map.CameraPosition.Zoom;
                Xamarin.Forms.GoogleMaps.Position pos = new Xamarin.Forms.GoogleMaps.Position(location.Latitude, location.Longitude);
                if (pl_listner.Positions.Count >= 1)
                {
                    //Plugin.Geolocator.Abstractions.Position pss = new Plugin.Geolocator.Abstractions.Position(pl_listner.Positions[pl_listner.Positions.Count - 1].Latitude,
                    //    pl_listner.Positions[pl_listner.Positions.Count - 1].Longitude);
                    //if (GeolocatorUtils.CalculateDistance(pss, e.Position, GeolocatorUtils.DistanceUnits.Kilometers) > 10) //* 1000 > 10)
                    var buf = pl_listner.Positions.Last();
                    var tt = Location.CalculateDistance(buf.Latitude, buf.Longitude, location, DistanceUnits.Kilometers) * 1000;
                    if (tt >= 10 && tt < 100)
                    {
                        if (!pl_listner.Positions.Contains(pos))
                            SetLine(pos, false);
                    }
                    //RefreshSpeed(poss);
                }
                else if (pl_listner.Positions.Count == 0)
                {
                    SetLine(pos, false);
                }
                if (location.Speed.HasValue)
                {
                    speed = (location.Speed.Value * 3600 / 1000);
                }
                //map.MoveCamera(CameraUpdateFactory.NewPositionZoom(new Xamarin.Forms.GoogleMaps.Position(e.Position.Latitude, e.Position.Longitude), zoom));
                var animState = await map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(
                   new CameraPosition(
                       pos,//StaticInfo.Pos.Latitude, StaticInfo.Pos.Longitude), // Tokyo
                       zoom, // zoom
                       0)),
                       TimeSpan.FromSeconds(1));
                string p = string.Format("{0:#0.#};{1:#0.#}", location.Latitude, location.Longitude, CultureInfo.InvariantCulture);
                Preferences.Set("LastKnownPosition", p);
            }

        }
        bool ps_listner = true;
        private async void StartListening()
        {
            //Device.StartTimer(TimeSpan.FromSeconds(1), () => PositionChanged_timer());
            if (CrossGeolocator.Current.IsListening)
                return;
            //CrossGeolocator.Current.DesiredAccuracy = 100;
            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromMilliseconds(500), 5, true, new ListenerSettings
            {
                ActivityType = ActivityType.OtherNavigation,
                AllowBackgroundUpdates = true,
                DeferLocationUpdates = true,
                DeferralDistanceMeters = 1,
                DeferralTime = TimeSpan.FromMilliseconds(250),
                ListenForSignificantChanges = true,
                PauseLocationUpdatesAutomatically = true//false
            });
            CrossGeolocator.Current.PositionChanged += PositionChanged;
            CrossGeolocator.Current.PositionError += PositionError;
        }

        private bool PositionChanged_timer()
        {
            HelpMap();
            return ps_listner;
        }
        private async void PositionChanged(object sender, PositionEventArgs e)
        {
            AnimationStatus animState;
            var zoom = map.CameraPosition.Zoom;
            Xamarin.Forms.GoogleMaps.Position pos = new Xamarin.Forms.GoogleMaps.Position(e.Position.Latitude, e.Position.Longitude);
            if (pl_listner.Positions.Count >= 1)
            {
                var spd = e.Position.Speed * 3600 / 1000;
                if (spd <= 500)
                {
                    speed = spd;
                    Debug.WriteLine("Speed = " + spd);
                }
                var buf = pl_listner.Positions.Last();
                var tt = Location.CalculateDistance(buf.Latitude, buf.Longitude, e.Position.Latitude, e.Position.Longitude, DistanceUnits.Kilometers) * 1000.0;
                if (tt > 5 && tt < 110 && spd != 0)
                {
                    if (LoaderFunction.is_sleep)
                        await LoaderFunction.Sleeping_pills(); //не работает дебаг в слипе
                    SetLine(pos, false);
                    if (!fl_Bearing)
                        animState = await map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(
                            new CameraPosition(
                                pos,
                                zoom,
                                0)),
                                TimeSpan.FromSeconds(1));
                    else
                        animState = await map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(
                            new CameraPosition(
                                pos,
                                zoom,
                                e.Position.Heading)),
                                TimeSpan.FromSeconds(1));
                    ToinitPos = pos;
                    if (!fl_Bearing)
                        ToinitBear = 0;
                    else
                        ToinitBear = e.Position.Heading;
                    Toinitzoom = zoom;
                }
                //RefreshSpeed(poss);
            }
            else if (pl_listner.Positions.Count == 0)
            {
                SetLine(pos, false);
                if (!fl_Bearing)
                    animState = await map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(
                        new CameraPosition(
                            pos,
                            zoom,
                            0)),
                            TimeSpan.FromSeconds(1));
                else
                    animState = await map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(
                        new CameraPosition(
                            pos,
                            zoom,
                            e.Position.Heading)),
                            TimeSpan.FromSeconds(1));
                ToinitPos = pos;
                if (!fl_Bearing)
                    ToinitBear = 0;
                else
                    ToinitBear = e.Position.Heading;
                Toinitzoom = zoom;
            }
            if (e.Position.Altitude != 0 && !has_barometr)
            {
                height_coord = true;
                height = e.Position.Altitude;
                height_coord = false;
            }
            string p = string.Format("{0:#0.#};{1:#0.#}", e.Position.Latitude, e.Position.Longitude, CultureInfo.InvariantCulture);
            Preferences.Set("LastKnownPosition", p);
        }

        private void PositionError(object sender, PositionErrorEventArgs e)
        {
            //Handle event here for errors
        }

        async Task<bool> StopListening()
        {
            //ps_listner = false;
            if (!CrossGeolocator.Current.IsListening)
                return false;
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
            if (ToinitPos != new Xamarin.Forms.GoogleMaps.Position())
            {
                pos = ToinitPos;
            }
            else
            {
                string buf = Preferences.Get("LastKnownPosition", "55.751316;37.620915");
                var op = buf.Split(';');
                pos = new Xamarin.Forms.GoogleMaps.Position(Convert.ToDouble(op[0], CultureInfo.InvariantCulture), Convert.ToDouble(op[1], CultureInfo.InvariantCulture));
            }
            Debug.WriteLine($"before go to new point");
            Debug.WriteLine($"{pos.Latitude};{pos.Longitude}");
            map.InitialCameraUpdate = CameraUpdateFactory.NewPositionZoom(pos, 11);
            map.MapLongClicked += map_MapLongClicked;
            //pl.Tag = "Line";
            //pl.StrokeWidth = 10;
            //pl.StrokeColor = Color.Blue;
            //GetGEOAsync();
            //map.MoveToRegion(new Xamarin.Forms.GoogleMaps.MapSpan(new Xamarin.Forms.GoogleMaps.Position(),loc.Latitude,loc.Longitude));
        }

        private void map_MapLongClicked(object sender, MapLongClickedEventArgs e)
        {
            if (!fl_run)
            {
                var qq = map.CameraPosition.Zoom;
                if (TypeLine == TypeInput.HANDLE)
                {
                    if (fl_route)
                    {
                        SetLine(e.Point, true);
                    }
                    else
                    {
                        SetPoint(e.Point, "Transparent");
                    }
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Toast.MakeText(Android.App.Application.Context, "Для редактирования карты, выберите ручной режим. (дист. руч.)", ToastLength.Long).Show();
                    }
                    );
                }
            }
        }
        private void Map_PinClicked(object sender, PinClickedEventArgs e)
        {
            //Debug.WriteLine("Pin clicked");
            //Xamarin.Forms.GoogleMaps.Position t;
            ActivePin = e.Pin;
            InfoPanelPin.IsVisible = true;
            if (!fl_run)
            {
                if (fl_route)
                {
                    if (fl_USE_MAP_CLICK)
                    {
                        var t = e.Pin;
                        int buff = map.Pins.IndexOf(e.Pin);
                        map.Pins.RemoveAt(buff);
                        if (pl_handle.Positions.Count >= 1)
                        {
                            dist_handle += GeolocatorUtils.CalculateDistance(new
                                Plugin.Geolocator.Abstractions.Position(pl_handle.Positions[pl_handle.Positions.Count - 1].Latitude,
                                pl_handle.Positions[pl_handle.Positions.Count - 1].Longitude),
                           new Plugin.Geolocator.Abstractions.Position(t.Position.Latitude, t.Position.Longitude), GeolocatorUtils.DistanceUnits.Kilometers);
                            SetLine(t.Position, true);
                            //pl_handle.Positions.Add(t);
                        }
                        else
                        {
                            pl_handle.Positions.Add(t.Position);
                            if (pl_handle.Positions.Count >= 1)
                            {
                                SetLine(t.Position, true);
                            }
                        }
                        //map.Polylines.Remove(pl_handle);
                        //map.Polylines.Add(pl_handle);
                    }
                }
            }

        }
        private void SetPoint(Xamarin.Forms.GoogleMaps.Position e, string Tag_line = "", bool fl_transp = false)
        {
            var _icon = BitmapDescriptorFactory.DefaultMarker(Xamarin.Forms.Color.DeepSkyBlue);
            if (Tag_line != "Transparent")
            {
                if (map.Pins.Count >= 1)
                {
                    if (!fl_transp)
                        _icon = BitmapDescriptorFactory.DefaultMarker(Xamarin.Forms.Color.DeepSkyBlue);
                    map.Pins.Add(new Pin() { Label = $"{map.Pins.Count - 1}", Position = e, IsDraggable = true, Icon = _icon, Tag = $"{map.Pins.Count - 1}_" + Tag_line });
                }
                else
                {
                    if (fl_route)
                        _icon = BitmapDescriptorFactory.DefaultMarker(Xamarin.Forms.Color.Red);
                    else
                        _icon = BitmapDescriptorFactory.DefaultMarker(Xamarin.Forms.Color.DeepSkyBlue);
                    map.Pins.Add(new Pin() { Label = $"Start", Position = e, IsDraggable = true, Icon = _icon, Tag = "Start_" + Tag_line });
                }
            }
            else
            {
                pl_transparent.Positions.Add(e);
                if (!fl_route)
                    dist_handle = CalcDistForLine(pl_transparent);
                //if (pl_transparent.Positions.Count >= 1)
                //{
                map.Pins.Add(new Pin() { Label = $"{pl_transparent.Positions.Count - 1}", Position = e, IsDraggable = true, Icon = _icon, Tag = $"{pl_transparent.Positions.Count - 1}_" + Tag_line });
                //}
                //else
                //{
                //    map.Pins.Add(new Pin() { Label = $"Start", Position = e, IsDraggable = true, Icon = _icon, Tag = "Start_" + Tag_line });
                //}
            }
            if (!fl_transp)
            {
                MapObjects mo = new MapObjects() { Pins = map.Pins.ToList() };
                if (map.Polylines.Count > 0)
                {
                    mo.Polylines = MapLines;
                }
                SaveToHist(mo);
            }

        }
        private void SetLineInner(Xamarin.Forms.GoogleMaps.Position e, Polyline pl)
        {
            string dop = pl.Tag.ToString();
            if (pl.Positions.Count >= 1)
            {
                double dist_buf;
                dist_buf = GeolocatorUtils.CalculateDistance(new Plugin.Geolocator.Abstractions.Position(pl.Positions[pl.Positions.Count - 1].Latitude, pl.Positions[pl.Positions.Count - 1].Longitude),
                new Plugin.Geolocator.Abstractions.Position(e.Latitude, e.Longitude), GeolocatorUtils.DistanceUnits.Kilometers);
                if (pl.Tag.ToString() == "Handle")
                {
                    dist_handle += dist_buf;
                }
                else
                {
                    dist += dist_buf;
                }
                pl.Positions.Add(e);
                Pin pn;
                if (map.Pins.Count >= 1)
                {
                    if (map.Pins.Any(q => q.Tag.ToString() == "End_" + dop))//q.Label == "End"))
                    {
                        pn = map.Pins.Where(i => i.Tag.ToString() == "End_" + dop).First();//i.Label == "End").First();
                        map.Pins.Remove(pn);
                        if (pl.Tag.ToString() == "Handle")
                            SetPoint(pn.Position, dop, true);
                        pn.Position = e;
                        map.Pins.Add(pn);
                    }
                    else
                        map.Pins.Add(new Pin() { Label = "End", Position = e, IsDraggable = true, Tag = "End_" + dop });
                }
            }
            else
            {
                pl.Positions.Add(e);
                map.Pins.Add(new Pin() { Label = "Start", Position = e, IsDraggable = true, Tag = "Start_" + dop });
            }
            if (pl.Positions.Count >= 2)
            {
                if (map.Polylines.Any(t => t.Tag.ToString() == pl.Tag.ToString()))
                {
                    map.Polylines.Remove(map.Polylines.Where(t => t.Tag.ToString() == pl.Tag.ToString()).First());
                }
                map.Polylines.Add(pl);
            }
        }
        private void SetLine(Xamarin.Forms.GoogleMaps.Position e, bool fl_handle, bool fl_from_hist = false)
        {
            if (fl_handle)
            {
                SetLineInner(e, pl_handle);
            }
            else
            {
                SetLineInner(e, pl_listner);
            }
            if (!fl_from_hist)
            {
                MapObjects mo = new MapObjects();
                if (MapLines.Count > 0)
                {
                    mo.Polylines = MapLines;
                }
                mo.Pins = map.Pins.ToList();
                SaveToHist(mo);
            }
        }
        public async void SetInitVew(Location location)
        {
            if (ToinitPos != new Xamarin.Forms.GoogleMaps.Position())
            {
                var animState = await map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(
                    new CameraPosition(
                        ToinitPos,//StaticInfo.Pos.Latitude, StaticInfo.Pos.Longitude), // Tokyo
                        Toinitzoom, // zoom
                        ToinitBear)),
                        TimeSpan.FromSeconds(2));
            }
            else
            {
                if (StaticInfo.Pos != null)
                {
                    await Task.Delay(1000);
                    var animState = await map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(
                    new CameraPosition(
                        new Xamarin.Forms.GoogleMaps.Position(location.Latitude, location.Longitude),//StaticInfo.Pos.Latitude, StaticInfo.Pos.Longitude), // Tokyo
                        Toinitzoom, // zoom
                        ToinitBear)),
                    TimeSpan.FromSeconds(2));
                }
            }

        }
        private MapObjects LoadFromHist()
        {
            History = ShiftList.ShiftRight(History, 1);
            History[0] = null;
            if (History.Last() != null)
            {
                return JsonConvert.DeserializeObject<MapObjects>(History.Last());
            }
            else
                return null;
        }
        private MapObjects LoadFromHistActual()
        {
            //History = ShiftList.ShiftRight(History, 1);
            //History[0] = null;
            return JsonConvert.DeserializeObject<MapObjects>(History.Last());
        }
        private bool OnTimerTick()
        {
            times.Sec++;
            StatusTime.Text = times.ToString();
            if (!Is_base)
            {
                StaticInfo.Nalet = times.ToString();
            }
            else
            {
                LoaderFunction.ItemsPageAlone.SetNal(times.ToString());
            }
            //StaticInfo.Nalet = t.ToString();
            return alife;
        }
        private void SaveToHist(MapObjects obj)
        {
            History.RemoveAt(0);
            History.Insert(0, null);
            History = ShiftList.ShiftLeft(History, 1);
            History[History.Count - 1] = JsonConvert.SerializeObject(obj);
            //var buf = new MapObjects[History.Length]; //пустой

            //Array.Copy(History, 1, buf, 0, History.Length - 1);
            //History[History.Length - 1] = obj;
        }
        private async void SwManual_Toggled(object sender, ToggledEventArgs e)
        {
            if (await DisplayAlert("Предупреждение", "Текущий маршрут будет стёрт", "ОК", "Отммена"))
            {
                //pl.Positions.Clear();
                //map.Pins.Clear();
                //map.Polylines.Clear();
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
                if (pl_listner.Positions.Count > 0)//!string.IsNullOrWhiteSpace(StaticInfo.Nalet))
                {
                    bool kek = await DisplayAlert("Предупреждение", "Новая запись? Записанное будет стёрто", "Да", "Нет");
                    if (kek)
                    {
                        //times.Sec = 0;
                        //StaticInfo.Nalet = times.ToString();
                        var asdf = new List<Pin>(map.Pins.Where(t => t.Tag.ToString() == "Start_" + "Listner")); //t.Label == "Start"
                        if (asdf.Count() > 0)
                        {
                            foreach (var item in asdf)
                            {
                                if (pl_listner.Positions.Contains(item.Position))
                                {
                                    map.Pins.Remove(item);
                                }
                            }
                        }
                        asdf = new List<Pin>(map.Pins.Where(t => t.Tag.ToString() == "End_" + "Listner"));//t.Label == "End"));
                        if (asdf.Count() > 0)
                        {
                            foreach (var item in asdf)
                            {
                                if (pl_listner.Positions.Contains(item.Position))
                                {
                                    map.Pins.Remove(item);
                                }
                            }
                        }
                        if (map.Polylines.Contains(pl_listner))
                        {
                            map.Polylines.Remove(pl_listner);
                        }
                        pl_listner.Positions.Clear();
                        times = new Time_r();
                    }
                    else
                    {
                        var request = new GeolocationRequest(GeolocationAccuracy.High);
                        Location loc = new Location();
                        try
                        {
                            CancellationTokenSource cts = new CancellationTokenSource();
                            cts.CancelAfter(5000);
                            loc = await Geolocation.GetLastKnownLocationAsync();
                            var location = await Geolocation.GetLocationAsync(request, cts.Token);
                            SetLine(new Xamarin.Forms.GoogleMaps.Position(location.Latitude, location.Longitude), false);
                        }
                        catch (Exception)
                        {
                            SetLine(new Xamarin.Forms.GoogleMaps.Position(loc.Latitude, loc.Longitude), false);
                        }
                        //pl_listner.Positions.Add(new Xamarin.Forms.GoogleMaps.Position(location.Latitude, location.Longitude));
                    }
                }
                b1.Text = "Стоп";
                alife = true;
                fl_run = true;
                MapObjects mo = new MapObjects();
                if (map.Polylines.Count > 0)
                {
                    mo.Polylines = MapLines;
                }
                mo.Pins = map.Pins.ToList();
                SaveToHist(mo);
                Device.StartTimer(TimeSpan.FromSeconds(1), () => OnTimerTick());
                StartListening();
                //await StartListening();
            }
            else
            {
                //bool kek2 = await StopListening();
                //if (kek2)
                var t = await StopListening();
                {
                    fl_run = false;
                    alife = false;
                    b1.Text = "Старт";
                    if (!Is_base)
                    {
                        StaticInfo.Nalet = times.ToString();
                    }
                    else
                    {
                        LoaderFunction.ItemsPageAlone.SetNal(times.ToString());
                    }
                    if (map.Polylines.Count > 0)
                    {
                        Xamarin.Forms.GoogleMaps.Position pp = pl_listner.Positions.Last();//map.Polylines.First().Positions.Last();
                        if (map.Pins.Any(q => q.Tag.ToString() == "End_Listner"))//q.Label == "End"))
                        {
                            var pn = map.Pins.Where(i => i.Tag.ToString() == "End_Listner").First();//i.Label == "End").First();
                            map.Pins.Remove(pn);
                        }
                        map.Pins.Add(new Pin() { Tag = "End_Listner", Label = "End", Position = pp, IsDraggable = true });
                        SaveToHist(new MapObjects(map.Pins.ToList(), MapLines));
                    }
                }
            }
            //b2.IsEnabled = true;
            //b1.IsEnabled = false;
            //Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
        }
        protected override bool OnBackButtonPressed()
        {
            if (fl_run)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Toast.MakeText(Android.App.Application.Context, "Идет запись маршрута", ToastLength.Long).Show();
                });
                return true;
            }
            else
            {
                fl_handle_ok_to_edit = null;
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
            //await CancelBtn.FadeTo(0, 100);
            //MapObjects mi = LoadFromHist();
            //if (mi != null)
            //{
            //    if (mi.Pins == null && mi.Polylines == null)
            //        Toast.MakeText(Android.App.Application.Context, "Нет сохранений в буфере", ToastLength.Long).Show();
            //    else
            //    {
            //        map.Pins.Clear();
            //        map.Polylines.Clear();
            //        if (mi.Polylines != null)
            //        {
            //            //var t = mi.Polylines.Where(qq => qq.Tag.ToString() == pl_handle.Tag.ToString()).ToList();
            //            if (mi.Polylines.SingleOrDefault(qq => qq.Tag.ToString() == pl_handle.Tag.ToString()) != null)
            //            {
            //                Polyline pl = mi.Polylines.Where(qq => qq.Tag.ToString() == pl_handle.Tag.ToString()).First();
            //                pl_handle.Positions.Clear();
            //                foreach (var item in pl.Positions)
            //                {
            //                    SetLine(item, true, true);
            //                }
            //            }
            //            if (mi.Polylines.SingleOrDefault(qq => qq.Tag.ToString() == pl_listner.Tag.ToString()) != null)
            //            {
            //                Polyline pl = mi.Polylines.Where(qq => qq.Tag.ToString() == pl_listner.Tag.ToString()).First();
            //                pl_listner.Positions.Clear();
            //                foreach (var item in pl.Positions)
            //                {
            //                    SetLine(item, false, true);
            //                }

            //            }
            //        }
            //        if (mi.Pins != null && mi.Pins.Count > 0)
            //        {
            //            foreach (var item in mi.Pins)
            //            {
            //                map.Pins.Add(item);
            //            }
            //        }
            //    }
            //}
            //else
            //    Toast.MakeText(Android.App.Application.Context, "Нет сохранений в буфере", ToastLength.Long).Show();
            //await CancelBtn.FadeTo(1, 100);
        }

        private async void ClearBtn_Clicked(object sender, EventArgs e)
        {
            await ClearBtn.FadeTo(0, 100);
            if (await DisplayAlert("Предупреждение", "Очистить карту? Данные карты будут очищены!", "Да", "Нет"))
            {
                ClearMap();

                map.IsVisible = false;
                await Task.Delay(100);
                map.IsVisible = true;
                //map.Pins.Clear();
                //map.Polylines.Clear();
                //pl_handle.Positions.Clear();
                //pl_listner.Positions.Clear();
                //var t = new Xamarin.Forms.GoogleMaps.Map() { MapType = map.MapType, MapStyle = map.MapStyle, MyLocationEnabled = map.MyLocationEnabled, HeightRequest = map.HeightRequest, VerticalOptions = map.VerticalOptions, HorizontalOptions = map.HorizontalOptions};
                //t.PinDragEnd += Map_PinDragEnd;
                //t.PinDragStart += Map_PinDragStart;
                //t.PinDragging += Map_PinDragging;
                //t.PinClicked += Map_PinClicked;
                //t.MapLongClicked += map_MapLongClicked;
                //t.MyLocationEnabled = map.MyLocationEnabled;
                //t.UiSettings.CompassEnabled = map.UiSettings.CompassEnabled;
                //t.UiSettings.ZoomControlsEnabled = map.UiSettings.ZoomControlsEnabled;
                //t.UiSettings.MyLocationButtonEnabled = map.UiSettings.MyLocationButtonEnabled;
                //t.UiSettings.ZoomGesturesEnabled = map.UiSettings.ZoomGesturesEnabled;
                //t.UiSettings.MapToolbarEnabled = map.UiSettings.MapToolbarEnabled;
                //map = t;
                //dist = 0;
                //dist_handle = 0;
                //height = 0;
                //height_list.Clear();
                //StatusTime.Text = string.Empty;
                //mapObjects = new MapObjects();
            }
            await ClearBtn.FadeTo(1, 100);
        }


        public bool Is_set { get; set; }

        private async void RouteTypePick_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SecureStorage.SetAsync("route", RouteTypePick.SelectedIndex.ToString());
            bool? buf = false;
            if (Is_set)
            {
                buf = fl_handle_ok_to_edit;
                fl_handle_ok_to_edit = true;
            }
            switch (RouteTypePick.SelectedIndex)
            {
                case 0:
                    fl_route = true;
                    if (pl_handle.Positions.Count > 1)
                        dist_handle = CalcDistForLine(pl_handle);
                    else
                        dist_handle = 0;
                    break;
                case 1:
                    fl_route = false;
                    if (pl_transparent.Positions.Count > 1)
                        dist_handle = CalcDistForLine(pl_transparent);
                    else
                        dist_handle = 0;
                    break;
            }
            if (Is_set)
            {
                fl_handle_ok_to_edit = buf;
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

        private async void ReCalcDist_Clicked(object sender, EventArgs e)
        {
            string buff = "";
            var q = pl_handle;
            switch (ActiveDistanse)
            {
                case "Listen":
                    buff = "записанного";
                    q = pl_listner;
                    break;
                case "Handle":
                    if (fl_route)
                    {
                        buff = "ручного (маршрут)";
                    }
                    else
                    {
                        q = pl_transparent;
                        buff = "ручного (точки)";
                    }
                    break;
            }
            if (await DisplayAlert("Предупреждение", $"Пересчитать дистанцию для {buff} маршрута", "Да", "Нет"))
            {
                if (q == pl_listner)
                    dist = CalcDistForLine(q);
                else
                    dist_handle = CalcDistForLine(q);
            }
            //if (map.Polylines.SingleOrDefault(qq => qq.Tag.ToString() == pl_handle.Tag.ToString()) != null)
            //    dist = CalcDistForLine(pl_handle);
            //else
            //Toast.MakeText(Android.App.Application.Context, "Нет пути для рассчета", ToastLength.Long).Show();
        }

        private void SetToPinRoute_Toggled(object sender, ToggledEventArgs e)
        {
            fl_USE_MAP_CLICK = e.Value;
            Preferences.Set("SwitchValue", e.Value);
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

        private async void DeletePin_Btn_Clicked(object sender, EventArgs e)
        {
            if (fl_handle_ok_to_edit == null)
            {
                fl_handle_ok_to_edit = await DisplayAlert("Предупреждение", "Дистанция будет пересчитана, заменять значения в записи?", "Да", "Нет");
            }//= //await DisplayAlert("Предупреждение", "Сохранять имеющиеся данные о дистанции/высоте?", "Да", "Нет");
            ad_vis.IsVisible = true;
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    Toast.MakeText(Android.App.Application.Context, "Перестроение линии, подождите", ToastLength.Short).Show();
            //}
            //);
            await Task.Delay(200);
            map.Pins.Remove(ActivePin);
            //ищем в линиях
            //ручн
            if (pl_handle.Positions.Contains(ActivePin.Position))
            {
                map.Polylines.Remove(pl_handle);
                pl_handle.Positions.Remove(ActivePin.Position);
                //Polyline tmp = pl_handle;
                var tmp = new List<Xamarin.Forms.GoogleMaps.Position>(pl_handle.Positions);
                pl_handle.Positions.Clear();
                _dist_handle = 0;
                foreach (var item in tmp)
                {
                    if (map.Pins.Any(t => t.Position == item))
                    {
                        var qq = map.Pins.Where(t => t.Position == item).FirstOrDefault();
                        map.Pins.Remove(qq);
                    }
                    SetLine(item, true);
                }
                ad_vis.IsVisible = false;
                //map.Polylines.Add(pl_handle);
                //dist_handle = CalcDistForLine(pl_handle);
                ClosePinInfo_Btn_Clicked(this, new EventArgs());
                return;
            }
            if (pl_listner.Positions.Contains(ActivePin.Position))
            {
                map.Polylines.Remove(pl_listner);
                pl_listner.Positions.Remove(ActivePin.Position);
                var tmp = new List<Xamarin.Forms.GoogleMaps.Position>(pl_listner.Positions);
                pl_listner.Positions.Clear();
                dist = 0;
                foreach (var item in tmp)
                {
                    SetLine(item, false);
                }
                //map.Polylines.Add(pl_listner);
                //dist = CalcDistForLine(pl_listner);
                ClosePinInfo_Btn_Clicked(this, new EventArgs());
            }
            if (pl_transparent.Positions.Contains(ActivePin.Position))
            {
                pl_transparent.Positions.Remove(ActivePin.Position);
                if (!fl_route)
                {
                    dist_handle = CalcDistForLine(pl_transparent);
                }
                ClosePinInfo_Btn_Clicked(this, new EventArgs());
            }
            ad_vis.IsVisible = false;
            await Task.Delay(200);
        }

        private async void ClosePinInfo_Btn_Clicked(object sender, EventArgs e)
        {
            bool q = await InfoPanelPin.FadeTo(0, 250);
            InfoPanelPin.IsVisible = false;
            InfoPanelPin.Opacity = 1;
        }
        private Pin _act_pin;
        public Pin ActivePin { get => _act_pin; set { _act_pin = value; entryPinLbl.Text = value.Label; } }

        private void RenamePin_Btn_Clicked(object sender, EventArgs e)
        {
            var tq = map.Pins.IndexOf(map.Pins.Where(t => t == ActivePin).First());
            map.Pins.RemoveAt(tq);
            ActivePin.Label = entryPinLbl.Text;
            map.Pins.Insert(tq, ActivePin);
            Toast.MakeText(Android.App.Application.Context, "Точка переименнована", ToastLength.Short).Show();
        }

        private void savevalswitsh_Toggled(object sender, ToggledEventArgs e)
        {
            _fl_handle_ok_to_edit = e.Value;
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            settest.IsVisible = !settest.IsVisible;
            //entryjson_points.Text = string.Empty;
            //entryjson_route.Text = string.Empty;
        }

        private async void DoneJson_Clicked(object sender, EventArgs e)
        {
            //var _route = entryjson_route.Text;
            //var _points = entryjson_points.Text;
            //await this.setter_point(_points, _route, true);
            await Task.Delay(1000);
            Toast.MakeText(Android.App.Application.Context, "Добавлено переименнована", ToastLength.Short).Show();
        }

        private void Height_av_imgbtn_Clicked(object sender, EventArgs e)
        {
            height_corective = height;
            setactiveHeight((int)CurHeight);
            StatusH_av.Text = string.Format("{0:#0.0 м}", 0);
        }

        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            var t = CrossDeviceOrientation.Current.CurrentOrientation;
            if (e.Value == true)
            {
                CrossDeviceOrientation.Current.LockOrientation(DeviceOrientations.Portrait);
            }
            else
            {
                CrossDeviceOrientation.Current.UnlockOrientation();
            }
            Preferences.Set("OrientBlock", e.Value);
        }
        bool fl_Bearing = false;
        private async void ImageButton_Clicked(object sender, EventArgs e)
        {
            var t = sender as Xamarin.Forms.ImageButton;
            await t.ScaleTo(1.1);
            if (fl_Bearing)
            {
                BerHelp.BackgroundColor = Color.Gray;
                await t.RotateTo(-45);
            }

            else
            {
                BerHelp.BackgroundColor = Color.Orange;
                await t.RotateTo(0);
            }

            fl_Bearing = !fl_Bearing;
            Preferences.Set("fl_Bearing", fl_Bearing);
            await t.ScaleTo(0.9);
        }
    }
}