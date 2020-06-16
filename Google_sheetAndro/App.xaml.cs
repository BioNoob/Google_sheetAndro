using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using Google_sheetAndro.Services;
using Google_sheetAndro.Views;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace Google_sheetAndro
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            if (!fl_wait)
            {
                LoaderFunction.MenuPage = new MenuPage();
                MainPage = LoaderFunction.MenuPage;
                //LoaderFunction.asd.Finish();
                //MainPage = new NavigationPage(new MainPage());
                LoaderFunction.WheatherPage = new WheatherView();
                LoaderFunction.InfoPage = new InfoPage();//ok
                LoaderFunction.AbNavPage = new NavigationPage(LoaderFunction.InfoPage);
                LoaderFunction.TaskSelectPage = new TaskSelectPage();//ok
                LoaderFunction.ItemsPage = new ItemsPage();//ok TaskSelectPage

                LoaderFunction.ItemsPageAlone = new ItemsPage(true);//ok TaskSelectPage
                LoaderFunction.MapPageAlone = new MapPage(true);

                LoaderFunction.OutPage = new Page_out();//ok //переделать чтоб брал инфу с внутреннего хранилища?
                LoaderFunction.OutNavPage = new NavigationPage(LoaderFunction.OutPage);
                LoaderFunction.MapPage = new MapPage(); //ok
                LoaderFunction.SimpPage = new SimpleListView();



                //LoaderFunction.ItAlNavPage = new NavigationPage(LoaderFunction.ItemsPageAlone) { Title = "Запись", IconImageSource = "new_one.png" };
                //LoaderFunction.MapAlNavPage = new NavigationPage(LoaderFunction.MapPageAlone) { Title = "Навигация", IconImageSource = "gogMap.png" };

                //ЭТО ЕСТЬ НОВЫЙ ИТЕМ

                LoaderFunction.ItNavPage = new NavigationPage(LoaderFunction.ItemsPage) { Title = "Данные", IconImageSource = "EditTable.png" };
                LoaderFunction.MapNavPage = new NavigationPage(LoaderFunction.MapPage) { Title = "Навигация", IconImageSource = "gogMap.png" };
                LoaderFunction.WheNavPage = new NavigationPage(LoaderFunction.WheatherPage) { Title = "Погода", IconImageSource = "partly_cloudy_day.png" };
                LoaderFunction.MainPage = new MainPage();
                LoaderFunction.MainPage.Children.Add(LoaderFunction.ItNavPage);
                LoaderFunction.MainPage.Children.Add(LoaderFunction.MapNavPage);
                LoaderFunction.MainPage.Children.Add(LoaderFunction.WheNavPage);
                LoaderFunction.MAINNavPage = new NavigationPage(LoaderFunction.MainPage);

                //Это есть итем для отображения

                //LoaderFunction.ExtItemsViewer.Children.Add(LoaderFunction.ItNavPage);
                //LoaderFunction.ExtItemsViewer.Children.Add(LoaderFunction.MapNavPage);
                //LoaderFunction.ExtItNavPage = new NavigationPage(LoaderFunction.ExtItemsViewer);

                //вывод бд
                LoaderFunction.ItemsInfoPage = new ItemsInfo();//not ok need items
                LoaderFunction.ItInfoNavPage = new NavigationPage(LoaderFunction.ItemsInfoPage);

            }
            if (MainPage != LoaderFunction.MenuPage)
                MainPage = LoaderFunction.MenuPage;
            //LoaderFunction.EndLoad();
            //MainPage = new MenuPage();
            //MainPage = new MenuPage();/*MainPage();*/

        }
        static bool fl_wait = false;

        protected override async void OnStart()
        {

            if (!fl_wait)
            {
                Debug.WriteLine("RunStart");
                bool asd = await LoaderFunction.InitialiserPage();//.ConfigureAwait(true);
                Debug.WriteLine("EndStart");
            }
        }

        protected override async void OnSleep()
        {
            fl_wait = true;
            LoaderFunction.is_sleep = true;
            //if (!string.IsNullOrEmpty(Application.Current.MainPage.Title) & Application.Current.MainPage.Title != "Detail")
            {
                LoaderFunction.SaveLastState();
                Debug.WriteLine("Saved");
            }

            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            LoaderFunction.is_sleep = false;
            LoaderFunction.ItemsInfoPage.fl_init = false;
            // Handle when your app resumes
        }
    }
}
