using Android.Widget;
using Google_sheetAndro.Class;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TableAndro;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OfflineList : ContentPage
    {
        public ObservableCollection<TableItem> Items { get; set; }
        ItemsPage ip;
        bool Is_load;
        public OfflineList(bool is_load = false)
        {
            InitializeComponent();
            Is_load = is_load;
            Items = new ObservableCollection<TableItem>
            {
            };

            TableItems.ItemsSource = Items;
            TableItems.ItemTapped += Handle_ItemTapped;
        }

        public void SetTableData(List<TableItem> lti)
        {
            foreach (var item in lti)
            {
                Items.Add(item);
            }
        }
        TableItem buf;
        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;
            ip = new ItemsPage(true);
            buf = (TableItem)e.Item;
            ip.setter((TableItem)e.Item);
            NavigationPage np = new NavigationPage(ip);
            np.ToolbarItems.Clear();
            if(Is_load)
                np.ToolbarItems.Add(new ToolbarItem("Записать", "", Write));
            else
            np.ToolbarItems.Add(new ToolbarItem("Изменить", "", edit));
            np.ToolbarItems.Add(new ToolbarItem("Удалить", "", deletit));
            NavigationPage.SetHasNavigationBar(ip, true);
            //NavigationPage.SetHasNavigationBar(ip, true);
            //ip.ToolbarItems.Add(new ToolbarItem("Изменить it", "", edit));
            await Navigation.PushModalAsync(np);
            //await Navigation.PushModalAsync(ip, true);
            //await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

            //Deselect Item
            ((Xamarin.Forms.ListView)sender).SelectedItem = null;
        }
        private void deletit()
        {
            Items.Remove(buf);
        }
        private void edit()
        {
            int ind = Items.IndexOf(buf);
            Items.Remove(buf);
            Items.Insert(ind, ip.getter());
            Navigation.PopModalAsync();
            Saver();
        }
        private void Write()
        {
            int ind = Items.IndexOf(buf);
            Items.Remove(buf);
            var item = ip.getter();
            Items.Insert(ind, item);
            Navigation.PopModalAsync();
            if (SaveToBase(item))
                Items.Remove(item);
            Saver();
        }
        private void DellAllBtn_Clicked(object sender, EventArgs e)
        {
            Items.Clear();
            Navigation.PopModalAsync();
            Saver();
        }
        private void Saver()
        {
            string seria = JsonConvert.SerializeObject(Items);
            Preferences.Set("Offline_data", seria);
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, "Сохранено в память", ToastLength.Short).Show();
            });
        }
        private void SaveAllBtn_Clicked(object sender, EventArgs e)
        {
            if(Is_load)
            {
                foreach (var item in Items)
                {
                    if (SaveToBase(item))
                        Items.Remove(item);
                }
                Saver();
            }
            else
                Saver();
        }
        private bool SaveToBase(TableItem ti)
        {
            try
            {
                ti.author = StaticInfo.AccountEmail;
                ti.route = "";//MapPageAlone.MapObj.SerializableLine;
                ti.points = "";//LoaderFunction.MapPageAlone.MapObj.SerializablePins;
                Googles.ReadEntriesAsync(ti);
                //LoaderFunction.ExtItNavPage.Navigation.PopModalAsync();
                Toast.MakeText(Android.App.Application.Context, "Запись успешна", ToastLength.Long).Show();
                return true;
            }
            catch (Exception)
            {
                Toast.MakeText(Android.App.Application.Context, "Обновление неудачно", ToastLength.Long).Show();
                return false;
            }
        }
    }
}
