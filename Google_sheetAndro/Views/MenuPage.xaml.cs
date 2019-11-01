using Google_sheetAndro.Models;
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
        //NavigationPage aboutpg;
        //NavigationPage itemspg;
        //NavigationPage outpg;
        public MenuPage()
        {
            InitializeComponent();
            //this.Detail = itemspg;
            //возможно нужно прокрутить сразу все чтоб не лагало
            //Creator();
            //GG();
            //Thread t2 = new Thread(new ThreadStart(Creator));
            //t2.IsBackground = true;
            //t2.Start();
            //GG();
            //t2.Join();
        }
        //private void Creator()
        //{
        //Task[] tasks = new Task[] { new Task(() => cre(new InfoPage())), new Task(() => cre(new Page_out())), new Task(() => cre(new ItemsInfo())) };
        //aboutpg = new NavigationPage(new InfoPage());
        //outpg = new NavigationPage(new Page_out());
        //itemspg = new NavigationPage(new ItemsInfo());
        //foreach (Task item in tasks)
        //{
        //    item.Start();
        //}
        //Task.WaitAll(tasks);
        //this.Detail = itemspg;
        //}
        //private void cre(Page pg)
        //{
        //    if(pg is InfoPage)
        //    {
        //        aboutpg = new NavigationPage(pg);
        //    }
        //    else if(pg is Page_out)
        //    {
        //        outpg = new NavigationPage(pg);
        //    }
        //    else if(pg is ItemsInfo)
        //    {
        //        itemspg = new NavigationPage(pg);
        //    }
        //}
        //private void GG()
        //{
        //    Googles.InitService();
        //}
        public void sett(Page pg)
        {
            NavigationPage.SetHasNavigationBar(pg, true);
            if (Detail != null)
            {
                if (((NavigationPage)Detail).RootPage != pg)
                    Detail = new NavigationPage(pg);
            }
            else
                Detail = new NavigationPage(pg);
            //NavigationPage.SetHasNavigationBar(pg, false);
        }
        private async void ItemPageView_Clicked(object sender, EventArgs e)
        {
            await Task.Run(() => sett(LoaderFunction.ItemsInfoPage));
            IsPresented = false;
        }

        private async void GetItems_Clicked(object sender, EventArgs e)
        {
            await Task.Run(() => sett(LoaderFunction.OutPage));
            IsPresented = false;
        }

        private async void GetItems_Clicked_1(object sender, EventArgs e)
        {
            await Task.Run(() => sett(LoaderFunction.InfoPage));
            IsPresented = false;
        }
        private async void GetItems_Clicked_2(object sender, EventArgs e)
        {
            await Task.Run(() => sett(LoaderFunction.MapPage));
            //LoaderFunction.RunSetter();
            //map.SetInitVew();
            IsPresented = false;
        }
        private async void GetItems_Clicked_3(object sender, EventArgs e)
        {
            await Task.Run(() => sett(LoaderFunction.WheatherPage));
            IsPresented = false;
        }
    }
}