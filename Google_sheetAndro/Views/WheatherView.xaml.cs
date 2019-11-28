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
    }
}