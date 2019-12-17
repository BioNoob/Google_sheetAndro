using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using System;
using System.ComponentModel;
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
        protected override bool OnBackButtonPressed()
        {
            LoaderFunction.callClearMap();
            Navigation.PopToRootAsync();
            return true;
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

        private void TabbedPage_Appearing(object sender, EventArgs e)
        {
        }
        private void TabbedPage_CurrentPageChanged(object sender, EventArgs e)
        {
            NavigationPage.SetHasNavigationBar(((NavigationPage)CurrentPage).RootPage, false);
            if (((NavigationPage)CurrentPage).RootPage.GetType() == typeof(MapPage))// == map)
            {
                LoaderFunction.RunSetter(StaticInfo.Pos);
                //map.SetInitVew();
            }
        }
    }
}