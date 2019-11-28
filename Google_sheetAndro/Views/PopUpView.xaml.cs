using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PopUpView : ContentPage
    {
        public PopUpView()
        {
            InitializeComponent();
        }

        private async void BackBtn_Clicked(object sender, EventArgs e)
        {
        }
    }
}