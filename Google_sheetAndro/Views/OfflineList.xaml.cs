using Android.Widget;
using Google_sheetAndro.Class;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            IsBuser = false;
            Items = new ObservableCollection<TableItem>
            {
            };
            if (is_load)
                StatusLabel.Text = "Для сохранения в базу использовать кнопку сохранить";
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
            if (Is_load)
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
            Navigation.PopModalAsync();
            Saver();
        }
        private void edit()
        {
            int ind = Items.IndexOf(buf);
            Items.Remove(buf);
            Items.Insert(ind, ip.getter());
            Navigation.PopModalAsync();
            Saver();
        }
        private async void Write()
        {
            int ind = Items.IndexOf(buf);
            Items.Remove(buf);
            var item = ip.getter();
            Items.Insert(ind, item);
            Navigation.PopModalAsync();
            if (await SaveToBase(item))
                Items.Remove(item);
            Saver();
        }
        private async void DellAllBtn_Clicked(object sender, EventArgs e)
        {
            if (await DisplayAlert("Подтверждение", "Удалить все имеющиеся элементы в памяти", "Да", "Нет"))
                Items.Clear();
            //await Navigation.PopModalAsync();
            Saver();
        }
        public void Saver()
        {
            string seria = JsonConvert.SerializeObject(Items);
            Preferences.Set("Offline_data", seria);
            Task.Delay(100);
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, "Сохранено в память", ToastLength.Short).Show();
            });
        }
        private async void SaveAllBtn_Clicked(object sender, EventArgs e)
        {
            if (Is_load)
            {
                Dictionary<TableItem, bool> EndOper = new Dictionary<TableItem, bool>();
                //СОХРАНЯЕТ ТОЛЬКО ОДНУ В БАЗУ
                foreach (var item in Items)
                {
                    if (await SaveToBase(item))
                        EndOper.Add(item, true);
                    else
                        EndOper.Add(item, false);
                }
                foreach (var item in EndOper)
                {
                    if (item.Value)
                    {
                        Items.Remove(item.Key);
                    }
                }
                Saver();
            }
            else
                Saver();
        }
        public bool IsBuser { get; set; }
        private async Task<bool> SaveToBase(TableItem ti)
        {
            await Navigation.PushModalAsync(new ScreenSaver("Сохранение записи..."));
            AD.Opacity = 1;
            IsBuser = true;
            try
            {
                ti.author = StaticInfo.AccountEmail;
                ti.route = "";//MapPageAlone.MapObj.SerializableLine;
                ti.points = "";//LoaderFunction.MapPageAlone.MapObj.SerializablePins;
                if (await Googles.ReadEntriesAsync(ti))
                {
                    Toast.MakeText(Android.App.Application.Context, "Запись успешна", ToastLength.Long).Show();
                    await Navigation.PopModalAsync();
                    return true;
                }
                else
                {
                    Toast.MakeText(Android.App.Application.Context, "Запись неудачна", ToastLength.Long).Show();
                    await Navigation.PopModalAsync();
                    return false;
                }
                //LoaderFunction.ExtItNavPage.Navigation.PopModalAsync();
            }
            catch (Exception)
            {
                Toast.MakeText(Android.App.Application.Context, "Обновление неудачно", ToastLength.Long).Show();
                await Navigation.PopModalAsync();
                return false;
            }
            finally
            {
                IsBuser = false;
                AD.Opacity = 0;
            }
        }
    }
}
