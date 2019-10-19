/*
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

using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Windows.Input;
using Google_sheetAndro.Views;
using Google_sheetAndro.Class;

namespace RefreshSample.ViewModels
{
    public class WheatherViewModel : INotifyPropertyChanged
    {
        //public ObservableCollection<string> Items { get; set; }
        public WheatherViewModel()
        {
            Time = DateTime.Now.ToString("dd MMMM yyyy HH:mm");
            gpp = StaticInfo.Wheather;
            Place = StaticInfo.Place;
        }

        public ResponsedData gpp { get; set; }
        public string Place { get; set; }
        public string Time { get; set; }
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
                  caller();
                  this.CanRefresh = false;

                  return false;
              });
        }
        private async void caller()
        {
            await StaticInfo.GetWeatherReqAsync(StaticInfo.Pos);
            gpp = StaticInfo.Wheather;
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

