using Google_sheetAndro.Class;
using Newtonsoft.Json;
using RefreshSample.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
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

    }
}