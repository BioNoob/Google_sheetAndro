using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System;
using System.ComponentModel;
using System.Reflection;
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

        private void TabbedPage_Appearing(object sender, EventArgs e)
        {
            map = new MapPage();
            item = new ItemsPage();
            NavigationPage navMap = new NavigationPage(map);
            navMap.Title = "Навигация";
            navMap.IconImageSource = "info1.png";
            NavigationPage navItem = new NavigationPage(item);
            navItem.Title = "Запись";
            navItem.IconImageSource = "new_one.png";
            NavigationPage.SetTitleIconImageSource(navItem, "info1.png");
            this.Children.Add(new NavigationPage(item));
            this.Children.Add(new NavigationPage(map));
        }
    }
}