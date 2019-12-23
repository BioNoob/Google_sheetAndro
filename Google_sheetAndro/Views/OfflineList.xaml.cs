using Google_sheetAndro.Class;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OfflineList : ContentPage
    {
        public ObservableCollection<TableItem> Items { get; set; }

        public OfflineList()
        {
            InitializeComponent();

            Items = new ObservableCollection<TableItem>
            {
                //"Item 1",
                //"Item 2",
                //"Item 3",
                //"Item 4",
                //"Item 5"
            };

            MyListView.ItemsSource = Items;
        }

        public void SetTableData(List<TableItem> lti)
        {
            foreach (var item in lti)
            {
                Items.Add(item);
            }
        }
        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        private void DellAllBtn_Clicked(object sender, EventArgs e)
        {

        }

        private void SaveAllBtn_Clicked(object sender, EventArgs e)
        {

        }
    }
}
