using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InfoPage : ContentPage
    {
        //https://www.instagram.com/blackorange.aero/channel/
        //https://www.facebook.com/BlackOrange.Aero.Pilot/
        //http://blackorange.aero/
        //https://www.youtube.com/user/parakhin/videos
        public InfoPage()
        {
            InitializeComponent();
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += (s, e) => TapGestureRecognizer_Tapped(s, e);
            MailLbl.GestureRecognizers.Add(tgr);
            var tgrs = new TapGestureRecognizer();
            tgrs.Tapped += (s, e) => TapGestureRecognizer_Tapped_1(s, e);
            GitLbl.GestureRecognizers.Add(tgrs);

            var tgrs1 = new TapGestureRecognizer();
            tgrs1.Tapped += (s, e) => GoYb();
            YbBtn.GestureRecognizers.Add(tgrs1);
            var tgrs2 = new TapGestureRecognizer();
            tgrs2.Tapped += (s, e) => GoFb();
            FbBtn.GestureRecognizers.Add(tgrs2);
            var tgrs3 = new TapGestureRecognizer();
            tgrs3.Tapped += (s, e) => GoInst();
            InstaBtn.GestureRecognizers.Add(tgrs3);
            var tgrs4 = new TapGestureRecognizer();
            tgrs4.Tapped += (s, e) => GoSite();
            SiteLbl.GestureRecognizers.Add(tgrs4);

            APlist = new List<ApiInfo>()
            {
                new ApiInfo("Яндекс Геокодер API","ya.png","https://tech.yandex.ru/maps/geocoder/"),
                new ApiInfo("Google Sheets API","gogshe.png","https://developers.google.com/sheets/api"),
                new ApiInfo("Google Maps API","gogMap.png","https://developers.google.com/maps/documentation/android-sdk/intro"),
                new ApiInfo("DarkSky API","darksky.png","https://darksky.net/dev"),
                new ApiInfo("АОПА Росиия","AOPA.png","https://aopa.ru/"),
                new ApiInfo("Meteocenter.asia","wh.png","http://meteocenter.asia/")
            };
            BindingContext = this;
        }
        private async void GoFb()
        {
            await Launcher.OpenAsync(new Uri("https://www.facebook.com/BlackOrange.Aero.Pilot/"));
        }
        private async void GoYb()
        {
            await Launcher.OpenAsync(new Uri("https://www.youtube.com/user/parakhin/videos"));
        }
        private async void GoInst()
        {
            await Launcher.OpenAsync(new Uri("https://www.instagram.com/blackorange.aero/channel/"));
        }
        private async void GoSite()
        {
            await Launcher.OpenAsync(new Uri("http://blackorange.aero/"));
        }
        public List<ApiInfo> APlist { get; set; }
        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Launcher.OpenAsync(new Uri("mailto:bigjarik@gmail.com"));
            //Device.OpenUri(new Uri("mailto:bigjarik@gmail.com"));
        }

        private async void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
        {
            TagLabel tagSpan = (TagLabel)sender;
            await Launcher.OpenAsync(new Uri(tagSpan.Tag));
            //Device.OpenUri(new Uri(tagSpan.Tag));
        }
    }
    public class ApiInfo
    {
        private string name;
        private string img;
        private string tag;

        public ApiInfo(string n, string i, string t)
        {
            Name = n;
            Img = i;
            Tag = t;
        }
        public string Name { get => name; set => name = value; }
        public string Img { get => img; set => img = value; }
        public string Tag { get => tag; set => tag = value; }
    }
}