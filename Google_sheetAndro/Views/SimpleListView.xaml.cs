﻿using Google_sheetAndro.Class;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SimpleListView : ContentPage
    {
        private List<Winder> actualWind;
        public ObservableCollection<string> Items { get; set; }
        public List<Winder> ActualWind { get => actualWind; set { actualWind = value; OnPropertyChanged("ActualWind"); } }
        public SimpleListView()
        {
            InitializeComponent();
            this.BindingContext = this;
        }
    }
}
