using Google_sheetAndro.Class;
using Google_sheetAndro.Views;
using System;
using System.Threading.Tasks;
using TableAndro;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Google_sheetAndro.Models
{
    public static class LoaderFunction
    {
        public delegate void SetView();
        public static event SetView DoSetView;
        public delegate void CreateRow();
        public static event CreateRow DoCreateRow;
        public delegate void WheatherLoad();
        public static event WheatherLoad DoWheatherLoad;
        public static async Task<bool> GetGEOAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                var s = await Geolocation.GetLocationAsync(request);
                StaticInfo.Pos = s;
            }
            #region catch
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception

                //return fnsEx.Message;
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception

                //return fneEx.Message;
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception

                //return pEx.Message;
            }
            catch (Exception ex)
            {

                // Unable to get location
                //return ex.Message;
            }
            #endregion
            return true;
        }
        public static async Task<bool> init()
        {
            try
            {
                if (string.IsNullOrEmpty(StaticInfo.Place))
                    await StaticInfo.GetPlace(StaticInfo.Pos.Latitude, StaticInfo.Pos.Longitude);
                if (StaticInfo.Wheather == null)
                    await StaticInfo.GetWeatherReqAsync(StaticInfo.Pos);
                return true;
            }
            catch (Exception)
            {
                return false;
            }


            //Place = StaticInfo.Place;
            //gpp = StaticInfo.Wheather;
        }
        public static void RunSetter()
        {
            DoSetView?.Invoke();
        }
        public static void CreRow()
        {
            DoCreateRow?.Invoke();
        }
        public static void EndLoad()
        {
            ItemsInfoPage.Title = "Записи";
            LoaderFunction.MenuPage.sett(LoaderFunction.ItemsInfoPage);
            DoWheatherLoad?.Invoke();
            if(MenuPage != null && StaticInfo.AccountEmail != null)
            {
                MenuPage.setImg(StaticInfo.AccountPicture,StaticInfo.AccountEmail);
            }
        }
        public static InfoPage InfoPage;
        public static ItemsInfo ItemsInfoPage;
        public static ItemsPage ItemsPage;
        public static ItemsPage ItemsPageAlone;
        public static MainPage MainPage;
        public static MapPage MapPage;
        public static MenuPage MenuPage;
        public static Page_out OutPage;
        public static TaskSelectPage TaskSelectPage;
        public static WheatherView WheatherPage;
        public static NavigationPage ItAlNavPage;
        public static NavigationPage MapNavPage;
        public static NavigationPage WheNavPage;
        public static NavigationPage ItNavPage;
        public static NavigationPage AbNavPage;
        public static NavigationPage OutNavPage;
        public static NavigationPage MAINNavPage;
        public static NavigationPage ItInfoNavPage;
        //NavigationPage itemspg;
        //NavigationPage outpg;
        private static async Task Loader()
        {
        }
        private static Task IniPage()
        {
            return new Task(() =>
            {
                InfoPage = new InfoPage();//ok
                AbNavPage = new NavigationPage(InfoPage);
                TaskSelectPage = new TaskSelectPage();//ok
                ItemsPage = new ItemsPage();//ok TaskSelectPage
                ItemsPageAlone = new ItemsPage(true);//ok TaskSelectPage
                OutPage = new Page_out();//ok //переделать чтоб брал инфу с внутреннего хранилища?
                OutNavPage = new NavigationPage(OutPage);
                MapPage = new MapPage(); //ok
                                         //WheatherPage = new WheatherView();//ok

                ItAlNavPage = new NavigationPage(ItemsPageAlone) { Title = "Запись", IconImageSource = "new_one.png" };
                MapNavPage = new NavigationPage(MapPage) { Title = "Навигация", IconImageSource = "info1.png" };
                WheNavPage = new NavigationPage(WheatherPage) { Title = "Погода", IconImageSource = "info1.png" };
                ItNavPage = new NavigationPage(ItemsPage);
                MainPage = new MainPage();//ok ItemsPageAlone MapPage WheatherView
                MainPage.Children.Add(ItAlNavPage);
                MainPage.Children.Add(MapNavPage);
                MainPage.Children.Add(WheNavPage);
                MAINNavPage = new NavigationPage(MainPage);
                ItemsInfoPage = new ItemsInfo();//not ok need items
                ItInfoNavPage = new NavigationPage(ItemsInfoPage);
                MenuPage = new MenuPage();
            });
        }
        public static async Task InitialiserPage()
        {
            Googles.InitService();
            var q = await GetGEOAsync();
            var qq = await init();
            while (true)
            {
                //Меню имеет в  себе активити индикатор по дефолту на пустой странице
                //по окончанию загрузки ставит итемсинфо
                //запустить его в начале этого метода
                //раскрасить его и страницу градиентом
                //после выполнения endлоад выключать индикатор и ставить страницу
                //баг карты которая вылезает за пейдж на меинпейдже с табами *не нашел. Кнопка обнуления карты, сброс текущих значений. перемещение к позиции запомненной? почему она другая
                //добавить при выборе элемента кнопку назад!
                //Аутентификация - метода - логин с гугл, запомнили почту, имя + фотка в меню пейдж + почта, при записи данных добавляем почту добавившего
                //возможен функционал сейва маршрутов. (гугл таблица + джсон) в настройки = строка = разделенные геоточки.
                //вместо тригера на ручное переключение настройки карты

                if(q == true && qq == true)
                {
                    EndLoad();
                    break;
                }


            }

            //Task.Run(async () => await Loader()).Wait();

            //Task t1 = GetGEOAsync();

            //Task t2 = t1.ContinueWith(ct => init());
            //Task tWload = new Task(() => { WheatherPage = new WheatherView(); });
            //Task t = Task.WhenAll(new List<Task>() { t2, tWload }).ContinueWith(ct => EndLoad());
            //Task BackGround = tWload.ContinueWith(ct =>
            //{
            //    //not ok out items info
            //});
            //await Task.WhenAll(new[] { t, BackGround });

            //return true;
            //then load all info wheather and map and etc/
        }
    }
}
