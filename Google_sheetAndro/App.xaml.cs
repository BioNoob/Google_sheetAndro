using Google_sheetAndro.Models;
using Google_sheetAndro.Views;
using System.Diagnostics;
using Xamarin.Forms;

namespace Google_sheetAndro
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            //LoaderFunction.InitialiserPage();


            //MainPage = new NavigationPage(new MainPage());
            LoaderFunction.WheatherPage = new WheatherView();
            LoaderFunction.InfoPage = new InfoPage();//ok
            LoaderFunction.AbNavPage = new NavigationPage(LoaderFunction.InfoPage);
            LoaderFunction.TaskSelectPage = new TaskSelectPage();//ok
            LoaderFunction.ItemsPage = new ItemsPage();//ok TaskSelectPage
            LoaderFunction.ItemsPageAlone = new ItemsPage(true);//ok TaskSelectPage
            LoaderFunction.OutPage = new Page_out();//ok //переделать чтоб брал инфу с внутреннего хранилища?
            LoaderFunction.OutNavPage = new NavigationPage(LoaderFunction.OutPage);
            LoaderFunction.MapPage = new MapPage(); //ok
                                                    //WheatherPage = new WheatherView();//ok

            LoaderFunction.ItAlNavPage = new NavigationPage(LoaderFunction.ItemsPageAlone) { Title = "Запись", IconImageSource = "new_one.png" };
            LoaderFunction.MapNavPage = new NavigationPage(LoaderFunction.MapPage) { Title = "Навигация", IconImageSource = "info1.png" };
            LoaderFunction.WheNavPage = new NavigationPage(LoaderFunction.WheatherPage) { Title = "Погода", IconImageSource = "info1.png" };
            LoaderFunction.ItNavPage = new NavigationPage(LoaderFunction.ItemsPage);
            LoaderFunction.MainPage = new MainPage();//ok ItemsPageAlone MapPage WheatherView
            LoaderFunction.MainPage.Children.Add(LoaderFunction.ItAlNavPage);
            LoaderFunction.MainPage.Children.Add(LoaderFunction.MapNavPage);
            LoaderFunction.MainPage.Children.Add(LoaderFunction.WheNavPage);
            LoaderFunction.MAINNavPage = new NavigationPage(LoaderFunction.MainPage);
            LoaderFunction.ItemsInfoPage = new ItemsInfo();//not ok need items
            LoaderFunction.ItInfoNavPage = new NavigationPage(LoaderFunction.ItemsInfoPage);
            LoaderFunction.MenuPage = new MenuPage();

            MainPage = LoaderFunction.MenuPage;
            //LoaderFunction.EndLoad();
            //MainPage = new MenuPage();
            //MainPage = new MenuPage();/*MainPage();*/

        }
        bool fl_wait = false;


        protected override async void OnStart()
        {

            if (!fl_wait)
            {
                Debug.WriteLine("RunStart");
                await LoaderFunction.InitialiserPage().ConfigureAwait(true);
                Debug.WriteLine("EndStart");
            }

            // Handle when your app starts

        }
        protected override void OnSleep()
        {
            fl_wait = true;
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            LoaderFunction.ItemsInfoPage.fl_init = false;
            // Handle when your app resumes
        }
    }
}
