using Android.Widget;
using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using Google_sheetAndro.ViewModels;
using System;
using TableAndro;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemsInfo : ContentPage
    {
        //NavigationPage main;
        //NavigationPage item;
        public bool fl_init = true;
        ItemsInfoVM vM;
        //public ObservableCollection<ObservableGroupCollection<string, TableItem>> ItemsGroup { get; set; }
        public ItemsInfo()
        {
            InitializeComponent();
            //main = new NavigationPage();
            //item = new NavigationPage(new ItemsPage());

            vM = new ItemsInfoVM();
            vM.DoSetSelect += VM_DoSetSelect;
            LoaderFunction.DoClearMap += LoaderFunction_DoClearMap;
            //Year_pick.SelectedIndex = Year_pick.Items.Count - 1;
            Mounth_pick.SelectedIndex = 0;
            BindingContext = vM;
        }

        private void LoaderFunction_DoClearMap()
        {
            LoaderFunction.ExtItNavPage.Navigation.PopModalAsync();
        }

        private void VM_DoSetSelect()
        {
            Year_pick.SelectedIndex = Year_pick.Items.Count - 1;
        }

        private void Graph_pick_date_SelectedIndexChanged(object sender, EventArgs e)
        {
            //выбор года для показа записей
            Mounth_pick.SelectedIndex = 0;
            vM.ItemGroups = LocalTable.SortItems(Year_pick.SelectedItem.ToString(), 0);
        }

        private void Mounth_pick_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Mounth_pick.SelectedIndex != 0)
                vM.ItemGroups = LocalTable.SortItems(Year_pick.SelectedItem.ToString(), Mounth_pick.SelectedIndex);
            else
                vM.ItemGroups = LocalTable.SortItems(Year_pick.SelectedItem.ToString(), 0);
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            int yy = 0;
            if (int.TryParse(Year_pick.SelectedItem.ToString(), out yy) && Mounth_pick.SelectedIndex != 0)
                Options.opt.dateTime = new DateTime(yy, Mounth_pick.SelectedIndex, 1);
            //activity.IsEnabled = true;
            //activity.IsRunning = true;
            //activity.IsVisible = true;
            //activity.Focus();
            //StaticInfo.AI = activity;
            //main = new NavigationPage(new MainPage());
            NavigationPage.SetHasBackButton(LoaderFunction.MAINNavPage, true);
            await Navigation.PushModalAsync(LoaderFunction.MAINNavPage);//main);
        }

        private void update()
        {
            //ItemsPage tp = (ItemsPage)item.CurrentPage;
            ItemsPage tp = LoaderFunction.ItemsPageAlone;
            int last = Year_pick.SelectedIndex;
            try
            {
                TableItem ti = tp.getter();
                ti.author = StaticInfo.AccountEmail;
                ti.route = LoaderFunction.MapPageAlone.MapObj.SerializableLine;
                ti.points = LoaderFunction.MapPageAlone.MapObj.SerializablePins;
                Googles.UpdateEntry(ti);
                LoaderFunction.ExtItNavPage.Navigation.PopModalAsync();
                Toast.MakeText(Android.App.Application.Context, "Обновление прошло успешно", ToastLength.Long).Show();
            }
            catch (Exception)
            {
                Toast.MakeText(Android.App.Application.Context, "Обновление неудачно", ToastLength.Long).Show();
            }
            finally
            {
                if (Mounth_pick.SelectedIndex != 0)
                    vM.ItemGroups = LocalTable.SortItems(Year_pick.SelectedItem.ToString(), Mounth_pick.SelectedIndex);
                else
                    vM.ItemGroups = LocalTable.SortItems(Year_pick.SelectedItem.ToString(), 0);
                Year_pick.SelectedIndexChanged -= Graph_pick_date_SelectedIndexChanged;
                vM.years.Clear();
                vM.years = LocalTable.GetYearsList();
                Year_pick.SelectedIndexChanged += Graph_pick_date_SelectedIndexChanged;
                Year_pick.SelectedIndex = last;
            }

        }
        private void delete()
        {
            ItemsPage tp = LoaderFunction.ItemsPageAlone;
            //ItemsPage tp = (ItemsPage)item.CurrentPage;
            int last = Year_pick.SelectedIndex;
            try
            {
                TableItem ti = tp.getter();
                ti.author = StaticInfo.AccountEmail;
                ti.route = LoaderFunction.MapPageAlone.MapObj.SerializableLine;
                ti.points = LoaderFunction.MapPageAlone.MapObj.SerializablePins;
                Googles.DeleteEntry(ti);
                LoaderFunction.ExtItNavPage.Navigation.PopModalAsync();
                Toast.MakeText(Android.App.Application.Context, "Удаление прошло успешно", ToastLength.Long).Show();
            }
            catch (Exception)
            {
                Toast.MakeText(Android.App.Application.Context, "Удаление неудачно", ToastLength.Long).Show();
            }
            finally
            {
                if (Mounth_pick.SelectedIndex != 0)
                    vM.ItemGroups = LocalTable.SortItems(Year_pick.SelectedItem.ToString(), Mounth_pick.SelectedIndex);
                else
                    vM.ItemGroups = LocalTable.SortItems(Year_pick.SelectedItem.ToString(), 0);
                Year_pick.SelectedIndexChanged -= Graph_pick_date_SelectedIndexChanged;
                vM.years.Clear();
                vM.years = LocalTable.GetYearsList();
                Year_pick.SelectedIndexChanged += Graph_pick_date_SelectedIndexChanged;
                Year_pick.SelectedIndex = last;
            }

        }
        private async void TableItems_ItemTapped(object sender, ItemTappedEventArgs e)
        {

            //LoaderFunction.ItNavPage.ToolbarItems.Clear();
            //LoaderFunction.ItNavPage.ToolbarItems.Add(new ToolbarItem("Изменить", "", update));
            //LoaderFunction.ItNavPage.ToolbarItems.Add(new ToolbarItem("Удалить", "", delete));
            TableItem Ti = (TableItem)e.Item;

            ItemsPage tp = LoaderFunction.ItemsPageAlone;
            MapPage mp = LoaderFunction.MapPageAlone;
            tp.setter(Ti);
            mp.AbsSetter(Ti.route,Ti.points);
            
            mp.SetDSetH(Ti.range, Ti.height);

            LoaderFunction.ItAlNavPage = new NavigationPage(tp) { Title = "Запись", IconImageSource = "new_one.png" };
            LoaderFunction.MapAlNavPage = new NavigationPage(mp) { Title = "Навигация", IconImageSource = "gogMap.png" };

            LoaderFunction.ExtItemsViewer = new MainPage();
            LoaderFunction.ExtItemsViewer.Children.Clear();
            LoaderFunction.ExtItemsViewer.Children.Add(LoaderFunction.ItAlNavPage);
            LoaderFunction.ExtItemsViewer.Children.Add(LoaderFunction.MapAlNavPage);

            LoaderFunction.ExtItNavPage = new NavigationPage(LoaderFunction.ExtItemsViewer);
            LoaderFunction.ExtItNavPage.ToolbarItems.Clear();
            LoaderFunction.ExtItNavPage.ToolbarItems.Add(new ToolbarItem("Изменить", "", update));
            LoaderFunction.ExtItNavPage.ToolbarItems.Add(new ToolbarItem("Удалить", "", delete));



            NavigationPage.SetHasNavigationBar(LoaderFunction.ExtItNavPage, false);
            await Navigation.PushModalAsync(LoaderFunction.ExtItNavPage);
        }
        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            if (fl_init)
            {
                vM.years = LocalTable.GetYearsList();
                if (Year_pick.Items.Count > 0)
                    Year_pick.SelectedIndex = 0;
                fl_init = !fl_init;
            }
        }

    }
}