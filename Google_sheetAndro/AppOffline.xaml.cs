using Google_sheetAndro.Class;
using Google_sheetAndro.Views;
using Newtonsoft.Json;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppOffline : Application
    {
        NavigationPage np;
        List<TableItem> ti;
        OfflineList ofl;
        public AppOffline()
        {
            InitializeComponent();
            np = new NavigationPage(new OfflineList() { Title = "Сохраненные в оффлайн" });
            string kk = Preferences.Get("Offline_data", "");
            ti = JsonConvert.DeserializeObject<List<TableItem>>(kk);

            ofl = new OfflineList();
            np = new NavigationPage(ofl);
            np.ToolbarItems.Add(new ToolbarItem("Добавить", "", add_item));
            //np.ToolbarItems.Add(new ToolbarItem("Удалить все", "", delete_all));
            if(ti.Count > 0)
            {
                ofl.SetTableData(ti);
            }
            MainPage = np;
        }
        bool fl_wait = false;
        protected override async void OnStart()
        {

            if (!fl_wait)
            {


                //await LoaderFunction.InitialiserPage().ConfigureAwait(true);
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
            //LoaderFunction.ItemsInfoPage.fl_init = false;
            // Handle when your app resumes
        }
        private void add_item()
        {
            ItemsPage ip = new ItemsPage(true);
            ip.setter(new TableItem());
            NavigationPage nnp = new NavigationPage(ip);
            NavigationPage.SetBackButtonTitle(nnp, "Назад");
            NavigationPage.SetHasBackButton(nnp, true);
            ofl.Navigation.PushModalAsync(nnp);
        }
    }
}