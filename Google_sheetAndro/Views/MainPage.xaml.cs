using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
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
        //MapPage map;
        //ItemsPage item;
        //WheatherView wheather;
        public MainPage()
        {
            InitializeComponent();
        }


        private async void ToolbarItem_Clicked(object sender, System.EventArgs e)
        {
            //item.CreateRow();
            LoaderFunction.CreRow();
            await Navigation.PopModalAsync();
        }

        private async void ToolbarItem_Clicked_1(object sender, System.EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void TabbedPage_Appearing(object sender, EventArgs e)
        {
            //ItemsPage ip = LoaderFunction.ItemsPageAlone;//new ItemsPage(true);
            //MapPage mp = LoaderFunction.MapPage;
            //WheatherView wp = LoaderFunction.WheatherPage;
            //mp = new MapPage();
            //wp = new WheatherView();
            //map = new MapPage();
            //item = new ItemsPage(true);
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    this.Children.Add(LoaderFunction.ItAlNavPage)/*new NavigationPage(ip) { Title = "Запись", IconImageSource = "new_one.png" })*/;
            //    
            //    this.Children.Add(LoaderFunction.MapNavPage);//new NavigationPage(mp) { Title = "Навигация", IconImageSource = "info1.png" });
            //    this.Children.Add(LoaderFunction.WheNavPage);//new NavigationPage(wp) { Title = "Погода", IconImageSource = "info1.png" });

            //});

            //this.Children.Add(new NavigationPage(item) { Title = "Запись", IconImageSource = "new_one.png" });
            //this.Children.Add(new NavigationPage(map) { Title = "Навигация", IconImageSource = "info1.png" });
            //if(StaticInfo.Pos == null)
            //{
            //    await map.GetGEOAsync();
            //}
            //await init();
            //this.Children.Add(new NavigationPage(new WheatherView()) { Title = "Погода", IconImageSource = "info1.png" });
            //StaticInfo.AI.IsEnabled = false;
            //StaticInfo.AI.IsRunning = false;
            //StaticInfo.AI.IsVisible = false;
        }
        private void TabbedPage_CurrentPageChanged(object sender, EventArgs e)
        {
            NavigationPage.SetHasNavigationBar(((NavigationPage)CurrentPage).RootPage, false);
            if (((NavigationPage)CurrentPage).RootPage.GetType() == typeof(MapPage))// == map)
            {
                LoaderFunction.RunSetter();
                //map.SetInitVew();
            }
        }
    }
}