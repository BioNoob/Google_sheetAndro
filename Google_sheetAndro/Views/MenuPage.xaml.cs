using System;
using System.Collections.Generic;
using System.Threading;
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
            Thread t2 = new Thread(new ThreadStart(Creator));
            t2.IsBackground = true;
            t2.Start();
            GG();
            t2.Join();
        }
        private void Creator()
        {
            Task[] tasks = new Task[] { new Task(() => cre(new InfoPage())), new Task(() => cre(new Page_out())), new Task(() => cre(new ItemsInfo())) };
            //aboutpg = new NavigationPage(new InfoPage());
            //outpg = new NavigationPage(new Page_out());
            //itemspg = new NavigationPage(new ItemsInfo());
            foreach (Task item in tasks)
            {
                item.Start();
            }
            Task.WaitAll(tasks);
            this.Detail = itemspg;
        }
        private void cre(Page pg)
        {
            if(pg is InfoPage)
            {
                aboutpg = new NavigationPage(pg);
            }
            else if(pg is Page_out)
            {
                outpg = new NavigationPage(pg);
            }
            else if(pg is ItemsInfo)
            {
                itemspg = new NavigationPage(pg);
            }
        }
        private void GG()
        {
            Googles.InitService();
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