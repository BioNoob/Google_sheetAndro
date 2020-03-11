
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
            Status = "Запуск";
            InitializeComponent();
            BindingContext = this;
            LoaderFunction.DoSetStatus += LoaderFunction_DoSetStatus;
        }

        private void LoaderFunction_DoSetStatus(string s)
        {
            Status = s;
        }
        private string _st;
        public string Status { get; set; }//get=>_st; set { _st = value; } }
    }
}