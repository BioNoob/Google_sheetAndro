using System;
using System.Threading.Tasks;
using TableAndro;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : MasterDetailPage
    {
        NavigationPage aboutpg;
        NavigationPage itemspg;
        NavigationPage outpg;
        public MenuPage()
        {
            InitializeComponent();
            //IsPresented = true;
            //возможно нужно прокрутить сразу все чтоб не лагало
            Googles.InitService();
            aboutpg = new NavigationPage(new InfoPage());
            outpg = new NavigationPage(new Page_out());
            itemspg = new NavigationPage(new ItemsInfo());
            this.Detail = itemspg;

        }

        private void sett(Page pg)
        {
            Detail = pg;
        }
        private async void ItemPageView_Clicked(object sender, EventArgs e)
        {
            await Task.Run(() => sett(itemspg));
            IsPresented = false;
        }

        private async void GetItems_Clicked(object sender, EventArgs e)
        {
            await Task.Run(() => sett(aboutpg));
            IsPresented = false;
        }

        private async void GetItems_Clicked_1(object sender, EventArgs e)
        {
            await Task.Run(() => sett(outpg));
            IsPresented = false;
        }
    }
}