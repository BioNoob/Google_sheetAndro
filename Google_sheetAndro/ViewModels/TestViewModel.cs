﻿/*
 * Copyright (C) 2015 Refractored LLC & James Montemagno: 
 * http://github.com/JamesMontemagno
 * http://twitter.com/JamesMontemagno
 * http://refractored.com
 * 
 * The MIT License (MIT) see GitHub For more information
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Google_sheetAndro.Views;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace RefreshSample.ViewModels
{
    public class TestViewModel : INotifyPropertyChanged
    {
        //public ObservableCollection<string> Items { get; set; }
        Page page;
        public TestViewModel(Page page)
        {
            this.page = page;
            //Items = new ObservableCollection<string>();
        }

        bool canRefresh = true;

        public bool CanRefresh
        {
            get { return canRefresh; }
            set
            {
                if (canRefresh == value)
                    return;

                canRefresh = value;
                OnPropertyChanged("CanRefresh");
            }
        }


        bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy == value)
                    return;

                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        ICommand refreshCommand;

        public ICommand RefreshCommand
        {
            get { return refreshCommand ?? (refreshCommand = new Command(async () => await ExecuteRefreshCommand())); }
        }

        async Task ExecuteRefreshCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            //Items.Clear();
            //page.SetDateFields();
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
              {

                 //for (int i = 0; i < 100; i++)
                     //Items.Add(DateTime.Now.AddMinutes(i).ToString("F"));

                  IsBusy = false;
                  if(page is Page_out)
                  ((Page_out)page).SetDateFields();
                  //page.DisplayAlert("Refreshed", "You just refreshed the page! Nice job! Pull to refresh is now disabled", "OK");
                  this.CanRefresh = false;

                          return false;
                  });
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

