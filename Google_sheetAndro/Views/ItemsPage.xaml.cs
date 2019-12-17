using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using TableAndro;
using Xamarin.Forms;

namespace Google_sheetAndro.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    //ЧУВАК создал страницу, зависимую от контента и переопредилил её. и типо использует, надо перепроверить https://forums.xamarin.com/discussion/88646/detecting-page-orientation-change-for-contentpages
    //ПС Смотри комментарии
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ItemsPage : ContentPage
    {
        private double _width;
        private double _height;
        private TableItem ti_local;
        public event EventHandler<PageOrientationEventArgs> OnOrientationChanged = (e, a) => { };
        public void setter(TableItem ti)
        {
            ti_local = ti;
            Date_pick.Date = ti.date;
            Time_pick.Text = ti.time;
            Wind_Num.Text = ti.wind.ToString();
            if (!cloud_list.Contains(ti.cloud))
                cloud_list.Add(ti.cloud);
            CloudPicker.SelectedItem = ti.cloud;
            if (ti.temp > 0)
                Temp_Num.Text = "+" + ti.temp.ToString();
            else
                Temp_Num.Text = ti.temp.ToString();
            Task_txt.Text = ti.task;
            Hight_txt_num.Text = ti.height.ToString();
            Range_txt.Text = ti.range.ToString();
            if (!place_list.Contains(ti.plase))
                place_list.Add(ti.plase);
            Place_txt.SelectedItem = ti.plase;
            Comment_txt.Text = ti.comment;
        }
        private void InitEvent()
        {
            StaticInfo.DoSetNalet += SetNal;
            StaticInfo.DoSetHeight += SetHeight;
            StaticInfo.DoSetDist += SetDist;
            StaticInfo.DoSetTemp += SetTemp;
            StaticInfo.DoSetWind += SetWind;
            StaticInfo.DoSetCloud += SetCloud;
            LoaderFunction.DoCreateRow += CreateRow;
            //StaticInfo.DoActiveAI += StaticInfo_DoActiveAI;
        }
        //private void StaticInfo_DoActiveAI(bool status)
        //{
        //    AcInd.IsEnabled = status;
        //    AcInd.IsRunning = status;
        //    AcInd.IsVisible = status;
        //    if(status)
        //        AcInd.Focus();
        //}

        private void SetTemp(string temp)
        {
            Temp_Num.Text = temp;
        }
        private void SetCloud(string cloud)
        {
            CloudPicker.SelectedItem = cloud;
        }
        private void SetWind(int wind)
        {
            if (WindSlider.Maximum < wind)
                WindSlider.Maximum = wind + 1;
            WindSlider.Value = wind;
        }
        private void SetNal(string nal)
        {
            Time_pick.Text = nal;
        }
        public void SetDist(double dist)
        {
            Range_txt.Text = string.Format(CultureInfo.InvariantCulture, "{0:#0.#}", dist);
        }
        public void SetHeight(int height)
        {
            Hight_txt_num.Text = height.ToString();
        }
        public TableItem getter()
        {
            //TableItem tb = new TableItem();
            if (ti_local == null)
            {
                ti_local = new TableItem();
            }
            var offset = TimeZoneInfo.Local.GetUtcOffset(Date_pick.Date.ToUniversalTime());
            ti_local.date = Date_pick.Date.ToUniversalTime() + offset;
            ti_local.time = Time_pick.Text;
            ti_local.wind = Convert.ToDouble(Wind_Num.Text);
            ti_local.cloud = CloudPicker.SelectedItem.ToString();
            ti_local.temp = Convert.ToDouble(Temp_Num.Text);
            ti_local.task = Task_txt.Text;
            ti_local.height = Convert.ToDouble(Hight_txt_num.Text);
            ti_local.range = Convert.ToDouble(Range_txt.Text);
            ti_local.plase = Place_txt.SelectedItem.ToString();
            ti_local.comment = Comment_txt.Text;
            return ti_local;
        }
        public ItemsPage(bool fl_single = false)
        {
            InitializeComponent();
            Init();
            if (fl_single)
                InitEvent();
            this.IsBusy = false;
            Date_pick.Format = "dd.MM.yyyy";
            Date_pick.Date = DateTime.Now;
            if (Options.opt.dateTime != new DateTime()) Date_pick.Date = Options.opt.dateTime;
            CloudPicker.ItemsSource = cloud_list;
            Place_txt.ItemsSource = place_list;
            CloudPicker.SelectedIndex = 0;
            Place_txt.SelectedIndex = 0;
            //Time_pick.Time = DateTime.Now.TimeOfDay;
            OnOrientationChanged += DeviceRotated;
            BindingContext = this;
        }

        private void Init()
        {
            _width = this.Width;
            _height = this.Height;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            var oldWidth = _width;
            const double sizenotallocated = -1;

            base.OnSizeAllocated(width, height);
            if (Equals(_width, width) && Equals(_height, height)) return;

            _width = width;
            _height = height;

            // ignore if the previous height was size unallocated
            if (Equals(oldWidth, sizenotallocated)) return;

            // Has the device been rotated ?
            if (!Equals(width, oldWidth))
                OnOrientationChanged.Invoke(this, new PageOrientationEventArgs((width < height) ? PageOrientation.Vertical : PageOrientation.Horizontal));
        }

        /*
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if(fl_resize)
            {
                if (width != this.width || height != this.height)
                {
                    this.width = width;
                    this.height = height;
                    if (width > height)
                    {
                        innerGrid.RowDefinitions.Clear();
                        innerGrid.ColumnDefinitions.Clear();
                        innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(210, GridUnitType.Star) });
                        innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180, GridUnitType.Star) });
                        innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80, GridUnitType.Star) });
                        innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(52.5, GridUnitType.Star) });
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
                        innerGrid.Children.Remove(gr_1);
                        innerGrid.Children.Remove(gr_2);
                        innerGrid.Children.Remove(gr_3);
                        innerGrid.Children.Remove(gr_4);
                        innerGrid.Children.Remove(gr_5);
                        innerGrid.Children.Remove(gr_6);
                        innerGrid.Children.Remove(gr_7);
                        innerGrid.Children.Add(gr_1, 0, 0);
                        innerGrid.Children.Add(gr_3, 1, 0);
                        innerGrid.Children.Add(gr_2, 0, 1);
                        innerGrid.Children.Add(gr_4, 1, 1);
                        innerGrid.Children.Add(gr_5, 0, 2);
                        innerGrid.Children.Add(gr_6, 1, 2);
                        innerGrid.Children.Add(gr_7, 0, 3);
                        Grid.SetColumnSpan(gr_7, 2);
                    }
                    else
                    {
                        innerGrid.RowDefinitions.Clear();
                        innerGrid.ColumnDefinitions.Clear();
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(210, GridUnitType.Star) });
                        innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180, GridUnitType.Star) });
                        innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(210, GridUnitType.Star) });
                        innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180, GridUnitType.Star) });
                        innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80, GridUnitType.Star) });
                        innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80, GridUnitType.Star) });
                        innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(52.5, GridUnitType.Star) });
                        innerGrid.Children.Remove(gr_1);
                        innerGrid.Children.Remove(gr_2);
                        innerGrid.Children.Remove(gr_3);
                        innerGrid.Children.Remove(gr_4);
                        innerGrid.Children.Remove(gr_5);
                        innerGrid.Children.Remove(gr_6);
                        innerGrid.Children.Remove(gr_7);
                        innerGrid.Children.Add(gr_1, 0, 0);
                        innerGrid.Children.Add(gr_2, 0, 1);
                        innerGrid.Children.Add(gr_3, 0, 2);
                        innerGrid.Children.Add(gr_4, 0, 3);
                        innerGrid.Children.Add(gr_5, 0, 4);
                        innerGrid.Children.Add(gr_6, 0, 5);
                        innerGrid.Children.Add(gr_7, 0, 6);
                        Grid.SetColumnSpan(gr_7, 1);
                    }
                }
            }
            fl_resize = false;
        }
        */
        private void DeviceRotated(object s, PageOrientationEventArgs e)
        {

            switch (e.Orientation)
            {
                case PageOrientation.Horizontal:
                    OrientAlbum();
                    break;
                case PageOrientation.Vertical:
                    OrientBook();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void OrientAlbum()
        {
            innerGrid.RowDefinitions.Clear();
            innerGrid.ColumnDefinitions.Clear();
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(210, GridUnitType.Star) });
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180, GridUnitType.Star) });
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80, GridUnitType.Star) });
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(52.5, GridUnitType.Star) });
            innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
            innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
            innerGrid.Children.Remove(gr_1);
            innerGrid.Children.Remove(gr_2);
            innerGrid.Children.Remove(gr_3);
            innerGrid.Children.Remove(gr_4);
            innerGrid.Children.Remove(gr_5);
            innerGrid.Children.Remove(gr_6);
            //innerGrid.Children.Remove(gr_7);
            innerGrid.Children.Add(gr_1, 0, 0);
            innerGrid.Children.Add(gr_3, 1, 0);
            innerGrid.Children.Add(gr_2, 0, 1);
            innerGrid.Children.Add(gr_4, 1, 1);
            innerGrid.Children.Add(gr_5, 0, 2);
            innerGrid.Children.Add(gr_6, 1, 2);
            //innerGrid.Children.Add(gr_7, 0, 3);
            //Grid.SetColumnSpan(gr_7, 2);
        }
        private void OrientBook()
        {
            innerGrid.RowDefinitions.Clear();
            innerGrid.ColumnDefinitions.Clear();
            innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(210, GridUnitType.Star) });
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180, GridUnitType.Star) });
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(210, GridUnitType.Star) });
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180, GridUnitType.Star) });
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80, GridUnitType.Star) });
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80, GridUnitType.Star) });
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(52.5, GridUnitType.Star) });
            innerGrid.Children.Remove(gr_1);
            innerGrid.Children.Remove(gr_2);
            innerGrid.Children.Remove(gr_3);
            innerGrid.Children.Remove(gr_4);
            innerGrid.Children.Remove(gr_5);
            innerGrid.Children.Remove(gr_6);
            //innerGrid.Children.Remove(gr_7);
            innerGrid.Children.Add(gr_1, 0, 0);
            innerGrid.Children.Add(gr_2, 0, 1);
            innerGrid.Children.Add(gr_3, 0, 2);
            innerGrid.Children.Add(gr_4, 0, 3);
            innerGrid.Children.Add(gr_5, 0, 4);
            innerGrid.Children.Add(gr_6, 0, 5);
            //innerGrid.Children.Add(gr_7, 0, 6);
            //Grid.SetColumnSpan(gr_7, 1);
        }



        private void Temp_Num_TextChanged(object sender, TextChangedEventArgs e)
        {
            TempSlider.ValueChanged -= TempSlider_ValueChanged;
            Regex rg = new Regex(@"^(?<sign>(\+|\-)?)(?<value>\d+(\.\d+)?)$");
            Match match = rg.Match(Temp_Num.Text);
            if (rg.Match(Temp_Num.Text).Success)
            {
                double val = Convert.ToDouble(match.Groups["value"].Value);
                //string dob = "";
                switch (match.Groups["sign"].Value)
                {
                    case "+":
                    case "":
                        if (val > 50.0)
                        {
                            val = 50.0;
                        }
                        TempSlider.Value = 50 + val;
                        //dob = "+";
                        break;
                    case "-":
                        if (val > 50.0)
                        {
                            val = 50.0;
                        }
                        TempSlider.Value = 50 - val;
                        //dob = "-";
                        break;
                }
            }
            TempSlider.ValueChanged += TempSlider_ValueChanged;
        }
        private void TempSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Temp_Num.TextChanged -= Temp_Num_TextChanged;
            string dob = "";
            double val = TempSlider.Value;
            if (val > 50)
            {
                dob = "+";
                val -= 50.0;
            }
            else if (val < 50)
            {
                dob = "-";
                val = 50.0 - val;
            }
            Temp_Num.Text = dob + string.Format("{0:#0.#}", val);
            Temp_Num.TextChanged += Temp_Num_TextChanged;
        }

        private void Confirm_btn_Clicked(object sender, EventArgs e)
        {
            CreateRow();
        }
        private List<string> cloud_list = new List<string>
        {
            "низкая",
            "высокая",
            "нет"
        };
        private List<string> place_list = new List<string>
        {
            "Моя деревня",
            "Река-Река",
            "Кузяево",
            "Спас-Загорье"
        };
        //private Dictionary<string, object> GetEditedValue()
        //{
        //    Dictionary<string, object> list = new Dictionary<string, object>();
        //    list.Add("date", Date_pick.Date.ToUniversalTime() /*+ TimeZoneInfo.GetUtcOffset(Date_pick.Date.ToUniversalTime())*/); //фикс для времени по локальному
        //    list.Add("time", Time_pick.Text);
        //    list.Add("wind", Convert.ToDouble(Wind_Num.Text));
        //    list.Add("cloud", CloudPicker.SelectedItem.ToString());
        //    list.Add("temp", Convert.ToDouble(Temp_Num.Text));
        //    list.Add("task", Task_txt.Text);
        //    list.Add("height", Convert.ToDouble(Hight_txt_num.Text));
        //    list.Add("range", Convert.ToDouble(Range_txt.Text));
        //    list.Add("plase", Place_txt.SelectedItem.ToString());
        //    list.Add("comment", Comment_txt.Text);
        //    return list;

        //}
        public void CreateRow()
        {
            IsBusy = true;
            TableItem ti = getter();
            ti.author = StaticInfo.AccountEmail;
            ti.route = LoaderFunction.MapPage.MapObj.SerializableLine;
            ti.points = LoaderFunction.MapPage.MapObj.SerializablePins;
            if (Googles.ReadEntriesAsync(ti))
            {

            }
            IsBusy = false;
        }

        private void Time_pick_TextChanged(object sender, TextChangedEventArgs e)
        {
            string val = Time_pick.Text;
            double[] form_val = new double[3];
            form_val[0] = Convert.ToDouble(val.Split(':')[0]);
            form_val[1] = Convert.ToDouble(val.Split(':')[1]);
            form_val[2] = Convert.ToDouble(val.Split(':')[2]);
            Time_r tm = new Time_r(form_val[0], form_val[1], form_val[2]);
            if (tm.Sec >= 359999)
                tm.Sec = 359999;
            val = tm.ToString();
            Time_pick.Text = val;
        }
        private void WindSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Wind_Num.Text = string.Format("{0:0}", WindSlider.Value);
        }

        private async void Btn_plus_time_Clicked(object sender, EventArgs e)
        {
            Time_pick.TextChanged -= Time_pick_TextChanged;
            var btn = (TagButton)sender;
            await btn.FadeTo(0, 10);
            TagButton tb = (TagButton)sender;
            if (Time_pick.Text == "")
                Time_pick.Text = "00:00:00";
            string val = Time_pick.Text;
            double[] form_val = new double[3];
            form_val[0] = Convert.ToDouble(val.Split(':')[0]);
            form_val[1] = Convert.ToDouble(val.Split(':')[1]);
            form_val[2] = Convert.ToDouble(val.Split(':')[2]);
            Time_r tm = new Time_r(form_val[0], form_val[1], form_val[2]);
            switch (tb.Tag)
            {
                case "++":
                    tm.Min += 10;
                    break;
                case "--":
                    if (tm.Min < 10)
                        tm.Min = 0;
                    else tm.Min -= 10;
                    break;
                case "+":
                    tm.Sec += 10;
                    break;
                case "-":
                    if (tm.Sec < 10)
                        tm.Sec = 0;
                    else tm.Sec -= 10;
                    break;
            }
            if (tm.Sec >= 359999)
                tm.Sec = 359999;
            Time_pick.Text = tm.ToString();
            Time_pick.TextChanged += Time_pick_TextChanged;
            await btn.FadeTo(1, 100);
        }



        private async void Btn_plus_hi_Clicked(object sender, EventArgs e)
        {
            TagButton tb = (TagButton)sender;
            await tb.FadeTo(0, 100);

            if (Hight_txt_num.Text == "")
                Hight_txt_num.Text = "0";
            switch (tb.Tag)
            {
                case "++":
                    Hight_txt_num.Text = string.Format("{0:000}", Convert.ToDouble(Hight_txt_num.Text, CultureInfo.InvariantCulture) + 100);
                    break;
                case "--":
                    Hight_txt_num.Text = string.Format("{0:000}", Convert.ToDouble(Hight_txt_num.Text, CultureInfo.InvariantCulture) - 100);
                    break;
                case "+":
                    Hight_txt_num.Text = string.Format("{0:000}", Convert.ToDouble(Hight_txt_num.Text, CultureInfo.InvariantCulture) + 50);
                    break;
                case "-":
                    Hight_txt_num.Text = string.Format("{0:000}", Convert.ToDouble(Hight_txt_num.Text, CultureInfo.InvariantCulture) - 50);
                    break;
            }
            await tb.FadeTo(1, 100);
        }
        private async void Btn_plus_len_Clicked(object sender, EventArgs e)
        {
            TagButton tb = (TagButton)sender;
            await tb.FadeTo(0, 100);

            if (Range_txt.Text == "")
                Range_txt.Text = "0";
            switch (tb.Tag)
            {
                case "++":
                    Range_txt.Text = string.Format("{0:#0.#}", Convert.ToDouble(Range_txt.Text, CultureInfo.InvariantCulture) + 10);
                    break;
                case "--":
                    Range_txt.Text = string.Format("{0:#0.#}", Convert.ToDouble(Range_txt.Text, CultureInfo.InvariantCulture) - 10);
                    break;
                case "+":
                    Range_txt.Text = string.Format("{0:#0.#}", Convert.ToDouble(Range_txt.Text, CultureInfo.InvariantCulture) + 5);
                    break;
                case "-":
                    Range_txt.Text = string.Format("{0:#0.#}", Convert.ToDouble(Range_txt.Text, CultureInfo.InvariantCulture) - 5);
                    break;
            }
            await tb.FadeTo(1, 100);
        }


        private async void Btn_temp_plus_Clicked(object sender, EventArgs e)
        {
            Temp_Num.TextChanged -= Temp_Num_TextChanged;
            TagButton tb = (TagButton)sender;
            await tb.FadeTo(0, 100);
            if (Temp_Num.Text == "")
                Temp_Num.Text = "0";
            double val = 0;
            string dob = "";

            switch (tb.Tag)
            {
                case "++":
                    Temp_Num.Text = string.Format("{0:#0.#}", Convert.ToDouble(Temp_Num.Text, CultureInfo.InvariantCulture) + 10);
                    break;
                case "--":
                    Temp_Num.Text = string.Format("{0:#0.#}", Convert.ToDouble(Temp_Num.Text, CultureInfo.InvariantCulture) - 10);
                    break;
                case "+":
                    Temp_Num.Text = string.Format("{0:#0.#}", Convert.ToDouble(Temp_Num.Text, CultureInfo.InvariantCulture) + 5);
                    break;
                case "-":
                    Temp_Num.Text = string.Format("{0:#0.#}", Convert.ToDouble(Temp_Num.Text, CultureInfo.InvariantCulture) - 5);
                    break;
            }
            if (double.TryParse(Temp_Num.Text, out val))
            {
                if (val > 0)
                    dob = "+";
                Temp_Num.Text = Temp_Num.Text.Insert(0, dob);
            }
            else
                Temp_Num.Text = "0";
            await tb.FadeTo(1, 100);
            Temp_Num.TextChanged += Temp_Num_TextChanged;
        }
        TaskSelectPage Tsp = LoaderFunction.TaskSelectPage;
        private async void Task_txt_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Task_txt.Text))
                Tsp.SetSelected(Task_txt.Text);
            Tsp.tasksetsucs += Tsp_tasksetsucs;
            await Navigation.PushModalAsync(Tsp);
            //Task_txt.Text = Tsp.OutTask;
        }

        private void Tsp_tasksetsucs(object sender, EventArgs e)
        {
            Task_txt.Text = Tsp.OutTask;
        }
    }
}