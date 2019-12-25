using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    public class MenuItems
    {
        public string IconSource { get; set; }
        public string BtnLabel { get; set; }
        public MenuItems()
        {

        }
        public MenuItems(string ico, string label)
        {
            IconSource = ico;
            BtnLabel = label;
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : MasterDetailPage
    {
        //NavigationPage aboutpg;
        //NavigationPage itemspg;
        //NavigationPage outpg;
        public List<MenuItems> Items { get; set; }
        public MenuPage()
        {
            InitializeComponent();
            if (StaticInfo.AccountEmail != null && StaticInfo.AccountPicture != null)
                setImg(StaticInfo.AccountPicture, StaticInfo.AccountEmail);
            Items = new List<MenuItems>()
            {
                {new MenuItems("table_menu.png","Просмотр записей")},
                {new MenuItems("chart_menu.png","Граф Отчёт")},
                {new MenuItems("map_menu.png","Картография")},
                {new MenuItems("whe_menu.png","Погода")},
                {new MenuItems("info_menu.png","Информация")}
            };
            StaticInfo.SetDetailPage += sett;
            BindingContext = this;
        }
        private async void ItemPageView_Clicked()
        {
            LoaderFunction.ItemsInfoPage.Title = "Записи";
            await Task.Run(() => StaticInfo.SetPage(LoaderFunction.ItemsInfoPage));//_mp.sett(LoaderFunction.ItemsInfoPage));

        }

        private async void GetItems_Clicked()
        {
            LoaderFunction.OutPage.Title = "График";
            await Task.Run(() => StaticInfo.SetPage(LoaderFunction.OutPage)); //_mp.sett(LoaderFunction.OutPage));
        }

        private async void GetItems_Clicked_1()
        {
            LoaderFunction.InfoPage.Title = "Информация";
            await Task.Run(() => StaticInfo.SetPage(LoaderFunction.InfoPage));//_mp.sett(LoaderFunction.InfoPage));
        }
        private async void GetItems_Clicked_2()
        {
            LoaderFunction.MapPage.Title = "Карта";
            await Task.Run(() => StaticInfo.SetPage(LoaderFunction.MapPage));//_mp.sett(LoaderFunction.MapPage));
        }
        private async void GetItems_Clicked_3()
        {
            LoaderFunction.WheatherPage.Title = "Погода";
            await Task.Run(() => StaticInfo.SetPage(LoaderFunction.WheatherPage));//_mp.sett(LoaderFunction.WheatherPage));
        }
        public bool mailIsSet = false;
        public void setImg(string source, string email)
        {
            profileImg.Source = source;
            profileEmail.Text = email;
            mailIsSet = true;
        }
        public void sett(Page pg)
        {
            NavigationPage.SetHasNavigationBar(pg, true);
            if (Detail == null)
            {
                Detail = new Page();
            }
            if (Detail.Title == null)
                Detail = new NavigationPage(pg) { Title = pg.Title };
            else if (Detail.Title != pg.Title)
                Detail = new NavigationPage(pg) { Title = pg.Title };
            IsPresented = false;
        }

        private void TableItems_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            switch (((MenuItems)e.Item).BtnLabel)
            {
                case "Просмотр записей":
                    ItemPageView_Clicked();
                    break;
                case "Граф отчёт":
                    GetItems_Clicked();
                    break;
                case "Картография":
                    GetItems_Clicked_2();
                    break;
                case "Погода":
                    GetItems_Clicked_3();
                    break;
                case "Информация":
                    GetItems_Clicked_1();
                    break;
            }
        }
    }
}