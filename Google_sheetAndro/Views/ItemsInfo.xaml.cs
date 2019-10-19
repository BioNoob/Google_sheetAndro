using Android.Widget;
using Google_sheetAndro.Class;
using Google_sheetAndro.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableAndro;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemsInfo : ContentPage
    {
        NavigationPage main;
        NavigationPage item;
        bool fl_init = true;
        ItemsInfoVM vM;
        //public ObservableCollection<ObservableGroupCollection<string, TableItem>> ItemsGroup { get; set; }
        public ItemsInfo()
        {
            InitializeComponent();
            //main = new NavigationPage();
            item = new NavigationPage(new ItemsPage());

            vM = new ItemsInfoVM();
            Year_pick.SelectedIndex = Year_pick.Items.Count - 1;
            Mounth_pick.SelectedIndex = 0;
            BindingContext = vM;
        }

        private void Graph_pick_date_SelectedIndexChanged(object sender, EventArgs e)
        {
            //выбор года для показа записей
            Mounth_pick.SelectedIndex = 0;
            vM.ItemGroups = LocalTable.SortItems(Year_pick.SelectedItem.ToString(), 0);
        }

        private void Mounth_pick_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(Mounth_pick.SelectedIndex != 0)
                vM.ItemGroups = LocalTable.SortItems(Year_pick.SelectedItem.ToString(), Mounth_pick.SelectedIndex);
            else
                vM.ItemGroups = LocalTable.SortItems(Year_pick.SelectedItem.ToString(), 0);
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            int yy = 0;
            if (int.TryParse(Year_pick.SelectedItem.ToString(), out yy) && Mounth_pick.SelectedIndex != 0)
                Options.opt.dateTime = new DateTime(yy, Mounth_pick.SelectedIndex, 1);
                main = new NavigationPage(new MainPage());
                await Navigation.PushModalAsync(main);
        }

        private void update()
        {
            ItemsPage tp = (ItemsPage)item.CurrentPage;
            int last = Year_pick.SelectedIndex;
            try
            {
                Googles.UpdateEntry(tp.getter());
                tp.Navigation.PopModalAsync();
                Toast.MakeText(Android.App.Application.Context, "Обновление прошло успешно", ToastLength.Long).Show();
            }
            catch(Exception)
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
            ItemsPage tp = (ItemsPage)item.CurrentPage;
            int last = Year_pick.SelectedIndex;
            try
            {
                Googles.DeleteEntry(tp.getter());
                tp.Navigation.PopModalAsync();
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
            item.ToolbarItems.Clear();
            item.ToolbarItems.Add(new ToolbarItem("Изменить", "", update));
            item.ToolbarItems.Add(new ToolbarItem("Удалить", "", delete));
            ItemsPage tp = (ItemsPage)item.RootPage;
            NavigationPage.SetHasNavigationBar(tp, true);
            NavigationPage.SetBackButtonTitle(tp, "Назад");
            NavigationPage.SetHasBackButton(tp, true);
            tp.setter((TableItem)e.Item);
            await Navigation.PushModalAsync(item);
        }



        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            if(fl_init)
            {
                vM.years = LocalTable.GetYearsList();
                if (Year_pick.Items.Count > 0)
                    Year_pick.SelectedIndex = 0;   
                fl_init = !fl_init;
            }
        }
        
    }
}