using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {
            InitializeComponent();
            Date_actual.Text = DateTime.Now.ToString("dddd \tdd MMMM yyyy");
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("mailto:bigjarik@gmail.com"));
        }

        private void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
        {
            TagLabel tagSpan = (TagLabel)sender;
            Device.OpenUri(new Uri(tagSpan.Tag));
        }
    }
}