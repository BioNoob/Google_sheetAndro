using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : MasterDetailPage
    {
        //NavigationPage aboutpg;
        //NavigationPage itemspg;
        //NavigationPage outpg;
        public MenuPage()
        {
            InitializeComponent();
            StaticInfo.SetDetailPage += sett;
        }
        private async void ItemPageView_Clicked(object sender, EventArgs e)
        {
            LoaderFunction.ItemsInfoPage.Title = "Записи";
            await Task.Run(() => StaticInfo.SetPage(LoaderFunction.ItemsInfoPage));//_mp.sett(LoaderFunction.ItemsInfoPage));

        }

        private async void GetItems_Clicked(object sender, EventArgs e)
        {
            LoaderFunction.OutPage.Title = "График";
            await Task.Run(() => StaticInfo.SetPage(LoaderFunction.OutPage)); //_mp.sett(LoaderFunction.OutPage));
        }

        private async void GetItems_Clicked_1(object sender, EventArgs e)
        {
            LoaderFunction.InfoPage.Title = "Информация";
            await Task.Run(() => StaticInfo.SetPage(LoaderFunction.InfoPage));//_mp.sett(LoaderFunction.InfoPage));
        }
        private async void GetItems_Clicked_2(object sender, EventArgs e)
        {
            LoaderFunction.MapPage.Title = "Карта";
            await Task.Run(() => StaticInfo.SetPage(LoaderFunction.MapPage));//_mp.sett(LoaderFunction.MapPage));
        }
        private async void GetItems_Clicked_3(object sender, EventArgs e)
        {
            LoaderFunction.WheatherPage.Title = "Погода";
            await Task.Run(() => StaticInfo.SetPage(LoaderFunction.WheatherPage));//_mp.sett(LoaderFunction.WheatherPage));
        }
        public void sett(Page pg)
        {
            NavigationPage.SetHasNavigationBar(pg, true);
            if(Detail == null)
            {
                Detail = new Page();
            }
            if (Detail.Title == null)
                Detail = new NavigationPage(pg) { Title = pg.Title };
            else if (Detail.Title != pg.Title)
                Detail = new NavigationPage(pg) { Title = pg.Title };
            IsPresented = false;
        }
    }
}