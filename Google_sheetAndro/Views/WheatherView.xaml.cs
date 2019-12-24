using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using RefreshSample.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WheatherView : ContentPage
    {
        public WheatherView()
        {
            InitializeComponent();
            BindingContext = new WheatherViewModel();
            //StatusError.PropertyChanging += StatusError_PropertyChanging;
        }
        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var q = (windout)e.SelectedItem;
            if (q != null)
            {
                LoaderFunction.SimpPage.ActualWind = q.Wind;
                if (LoaderFunction.SimpPage.ToolbarItems.Count < 1)
                {
                    LoaderFunction.SimpPage.ToolbarItems.Add(new ToolbarItem("Назад", "CancelLast", new System.Action(() => { backpress(); })));
                }
                NavigationPage navigationPage = new NavigationPage(LoaderFunction.SimpPage);
                await Navigation.PushModalAsync(navigationPage);
            }
            ((Xamarin.Forms.ListView)sender).SelectedItem = null;
        }
        async void backpress()
        {
            await Navigation.PopModalAsync(true);
        }
        private async void PopSettings_Clicked(object sender, System.EventArgs e)
        {
            await PopSettings.FadeTo(0, 100);
            ((WheatherViewModel)this.BindingContext).ExecuteRefreshCommand();
            await PopSettings.FadeTo(1, 100);
        }

        private void ActivityIndicator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }
    }
}