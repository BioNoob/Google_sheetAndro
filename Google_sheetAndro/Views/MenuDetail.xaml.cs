
using Google_sheetAndro.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuDetail : ContentPage
    {
        public MenuDetail()
        {
            InitializeComponent();
            StTxt.Text = "Запуск...";
            LoaderFunction.DoSetStatus += LoaderFunction_DoSetStatus;
        }
        private void LoaderFunction_DoSetStatus(string s)
        {
            StTxt.Text = s;
        }

//get=>_st; set { _st = value; } }
    }

}