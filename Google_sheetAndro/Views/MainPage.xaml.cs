using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google_sheetAndro.Class;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using TableAndro;
using Xamarin.Forms;

namespace Google_sheetAndro.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : TabbedPage
    {
        MapPage map;
        ItemsPage item;
        
        public MainPage()
        {
            InitializeComponent();
        }


        private async void ToolbarItem_Clicked(object sender, System.EventArgs e)
        {
            item.CreateRow();
            await Navigation.PopModalAsync();
        }

        private async void ToolbarItem_Clicked_1(object sender, System.EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void TabbedPage_Appearing(object sender, EventArgs e)
        {
            map = new MapPage();
            item = new ItemsPage(true);
            this.Children.Add(new NavigationPage(item) { Title = "Запись", IconImageSource = "new_one.png" });
            this.Children.Add(new NavigationPage(map) { Title = "Навигация", IconImageSource = "info1.png" });
            if(StaticInfo.Pos == null)
            {
                await map.GetGEOAsync();
            }
            await init();
            this.Children.Add(new NavigationPage(new WheatherView()) { Title = "Погода", IconImageSource = "info1.png" });
        }
        private async Task init()
        {
            if (string.IsNullOrEmpty(StaticInfo.Place))
                await StaticInfo.GetPlace(StaticInfo.Pos.Latitude, StaticInfo.Pos.Longitude);
            if (StaticInfo.Wheather == null)
                await StaticInfo.GetWeatherReqAsync(StaticInfo.Pos);

            //Place = StaticInfo.Place;
            //gpp = StaticInfo.Wheather;
        }
        private void TabbedPage_CurrentPageChanged(object sender, EventArgs e)
        {
            if(((NavigationPage)CurrentPage).RootPage == map)
            {
                map.SetInitVew();
            }
        }
    }
}