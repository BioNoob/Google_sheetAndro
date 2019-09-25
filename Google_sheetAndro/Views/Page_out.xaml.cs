using Google.Apis.Sheets.v4.Data;
using RefreshSample.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TableAndro;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Page_out : ContentPage
    {
        private double _width;
        private double _height;

        public event EventHandler<PageOrientationEventArgs> OnOrientationChanged = (e, a) => { };
        public Page_out()
        {
            InitializeComponent();
            Init();
            OnOrientationChanged += DeviceRotated;
            Graph_create.IsEnabled = false;
            VariantView.ItemsSource = new List<string> { "Ожидание выбора параметров.." };
            BindingContext = new TestViewModel(this);
            Mounth_pick.ItemsSource = new List<string>()
            {
                "Все доступные","январь", "февраль", "март", "апрель", "май", "июнь", "июль", "август", "сентябрь", "октябрь", "ноябрь", "декабрь"
            };
            Mounth_pick.SelectedIndex = 0;
        }
        Dictionary<string, ValueRange> tabelValue;
        Dictionary<string, List<Selectore>> Variant = new Dictionary<string, List<Selectore>>
        {
            { "AllYear",
                new List<Selectore> {
                    new Selectore("Сумма за год",OptionsBuild.SortingEnum.AllYearMidEvery),
                    new Selectore("Среднее за год",OptionsBuild.SortingEnum.AllYearMid),
                    new Selectore("Максимум за год",OptionsBuild.SortingEnum.AllYearMax),
                    new Selectore("Минимум за год",OptionsBuild.SortingEnum.AllYearMin) }
                    //new Selectore("Количество за год",OptionsBuild.SortingEnum.AllYearCount)}
            },
            {"Year",
                new List<Selectore> {
                    new Selectore("Среднее за месяц",OptionsBuild.SortingEnum.YearMid),
                    new Selectore("Каждый в месяце",OptionsBuild.SortingEnum.YearEvery),
                    new Selectore("Максимум за месяц",OptionsBuild.SortingEnum.YearMax),
                    new Selectore("Минимум за месяц",OptionsBuild.SortingEnum.YearMin),
                    new Selectore("Количество в месяце",OptionsBuild.SortingEnum.YearCount)}
            }
        };
        Dictionary<string, List<Selectore_sorting>> Variant_sorting = new Dictionary<string, List<Selectore_sorting>>
        {
            { "Summ",
                new List<Selectore_sorting> {
                    new Selectore_sorting("Налёт",OptionsBuild.EnumTyp.Time),
                    new Selectore_sorting("Дальность",OptionsBuild.EnumTyp.Range) }
            },
            { "Count",
                new List<Selectore_sorting> {
                    new Selectore_sorting("Задание",OptionsBuild.EnumTyp.Task),
                new Selectore_sorting("Примечание",OptionsBuild.EnumTyp.Primech) }
            },
            {"etc",
                new List<Selectore_sorting> {
                    new Selectore_sorting("Налёт",OptionsBuild.EnumTyp.Time),
                    new Selectore_sorting("Ветер",OptionsBuild.EnumTyp.Vind),
                    new Selectore_sorting("Температура",OptionsBuild.EnumTyp.Temperature),
                    new Selectore_sorting("Дальность",OptionsBuild.EnumTyp.Range)
                }
            }
        };
        private void Init()
        {
            _width = this.Width;
            _height = this.Height;
        }
        private void Graph_pick_date_SelectedIndexChanged(object sender, EventArgs e)
        {
            VariantView.SelectedIndexChanged -= VariantView_SelectedIndexChanged;
            Graph_pick.SelectedIndexChanged -= Graph_pick_SelectedIndexChanged;
            Graph_pick.SelectedItem = null;
            VariantView.SelectedItem = null;
            Graph_create.IsEnabled = false;
            Graph_pick.IsEnabled = false;
            if (Graph_pick_date.SelectedItem != null)
            {
                if (Graph_pick_date.SelectedItem.ToString() == "Все года")
                    VariantView.ItemsSource = Variant["AllYear"];
                else
                    VariantView.ItemsSource = Variant["Year"];
            }
            VariantView.IsEnabled = true;
            VariantView.SelectedIndexChanged += VariantView_SelectedIndexChanged;
            Graph_pick.SelectedIndexChanged += Graph_pick_SelectedIndexChanged;
        }
        private void VariantView_SelectedIndexChanged(object sender, EventArgs e)
        {
            Graph_pick.SelectedIndexChanged -= Graph_pick_SelectedIndexChanged;
            if (VariantView.SelectedItem != null)
            {
                if (((Selectore)VariantView.SelectedItem).Enumer == OptionsBuild.SortingEnum.AllYearMidEvery ||
                    ((Selectore)VariantView.SelectedItem).Enumer == OptionsBuild.SortingEnum.YearEvery)
                    Graph_pick.ItemsSource = Variant_sorting["Summ"];
                else if (((Selectore)VariantView.SelectedItem).Enumer == OptionsBuild.SortingEnum.YearCount ||
                    ((Selectore)VariantView.SelectedItem).Enumer == OptionsBuild.SortingEnum.AllYearCount)
                    Graph_pick.ItemsSource = Variant_sorting["Count"];
                else
                    Graph_pick.ItemsSource = Variant_sorting["etc"];
                Graph_pick.SelectedItem = null;
                Graph_pick.IsEnabled = true;
            }
            else
                Graph_pick.IsEnabled = false;
            Graph_pick.SelectedIndexChanged += Graph_pick_SelectedIndexChanged;
        }
        private void DeviceRotated(object s, PageOrientationEventArgs e)
        {
            if(Out.Source != null)
            {
            string sourse = Out.Source.ToString();
            Out.Source = null;
            sourse = sourse.Replace($"{Options.opt.Width}x{Options.opt.Height}", $"{(int)Out.Width}x{(int)Out.Height}");
            sourse = sourse.Replace("Uri: ", "");
            Out.Source = sourse;
            }
            Options.opt.Height = (int)Out.Height;
            Options.opt.Width = (int)Out.Width;
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
        private void Graph_pick_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(Graph_pick.SelectedItem != null)
                Graph_create.IsEnabled = true;
            else
                Graph_pick.IsEnabled = false;
        }
        //TO DO
        //Каждый в месяце не работает. Количество в месяце не работает. Привести налет к понятному, передавать размер элемента управления, проверить за все года
        private void Graph_create_Clicked(object sender, EventArgs e)
        {
            string Year = Graph_pick_date.SelectedItem.ToString();
            string image_source = string.Empty;
            //var sss = tabelValue[lll];
            Options.opt.Height = (int)Out.Height;
            Options.opt.Width = (int)Out.Width;
            Options.opt.ActiveSort = (int)((Selectore)VariantView.SelectedItem).Enumer;
            Options.opt.ActiveType = (int)((Selectore_sorting)Graph_pick.SelectedItem).Enumer;
            if (Graph_pick_date.SelectedItem.ToString() == "Все года")
            {
                image_source = ChartBuilder.GetBar(tabelValue);
            }
            else
                image_source = ChartBuilder.GetBar(tabelValue, Year);
            //Chart1.Chart = ChartBuilder.GetBar(tabelValue[lll]);
            //Chart1.Chart.LabelTextSize = 50;
            //Chart1.Chart.LabelTextSize = 15;
            Out.Source = image_source;
            Debug.WriteLine(Out.Source);
        }
        public void SetDateFields()
        {
            //Graph_pick_date.Items.Clear();
            tabelValue = Googles.GetValueTabel();
            foreach (string item in tabelValue.Keys)
            {
                if (!Graph_pick_date.Items.Contains(item))
                {
                    if (item != "Общий налет")
                        Graph_pick_date.Items.Add(item);
                }
            }
            Graph_pick_date.IsEnabled = true;
        }
        bool fl_fst_init = true;
        protected override void OnAppearing()
        {
            if (fl_fst_init)
            {
                tabelValue = Googles.GetValueTabel();
                foreach (string item in tabelValue.Keys)
                {
                    if (item != "Общий налет")
                        Graph_pick_date.Items.Add(item);
                }
                Graph_pick_date.IsEnabled = true;
                fl_fst_init = false;
            }
        }

        private void Mounth_pick_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.opt.ActiveMounth = Mounth_pick.SelectedIndex;
        }
    }
}