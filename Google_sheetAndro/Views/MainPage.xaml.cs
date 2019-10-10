using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
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
            map = new MapPage();
            map.Title = "Навигация";
            map.IconImageSource = "info1.png";
            item = new ItemsPage();
            item.Title = "Запись";
            item.IconImageSource = "new_one.png";
            this.Children.Add(new NavigationPage(item));

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
    }
}