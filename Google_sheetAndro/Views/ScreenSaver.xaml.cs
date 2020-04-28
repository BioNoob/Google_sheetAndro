using Android.Widget;
using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScreenSaver : ContentPage
    {
        public ScreenSaver(string name_proc)
        {
            InitializeComponent();
            StatusLoading.Text = name_proc;
            //this.BindingContext = new ScreenViewModel();
        }
    }
    public class ScreenViewModel : INotifyPropertyChanged
    {
        public ScreenViewModel()
        {
            LoaderFunction.DoStatusPush += StaticInfo_DoStatusPush;
        }
        private void StaticInfo_DoStatusPush(string status)
        {
            //Device.BeginInvokeOnMainThread(() => Toast.MakeText(Android.App.Application.Context, status, ToastLength.Long).Show());
            HelloWorld = status;
        }
        private string kek;
        public string HelloWorld { get => kek; set { kek = value; OnPropertyChanged("HelloWorld"); } }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}