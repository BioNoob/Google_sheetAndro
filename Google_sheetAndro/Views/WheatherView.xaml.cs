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
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

        }

        private async void PopSettings_Clicked(object sender, System.EventArgs e)
        {
            await PopSettings.FadeTo(0, 100);
            await ((WheatherViewModel)this.BindingContext).ExecuteRefreshCommand();


            await PopSettings.FadeTo(1, 100);
        }

        private void ActivityIndicator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }
    }
}