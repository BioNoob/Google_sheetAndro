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
using System.Collections.Generic;
using Google_sheetAndro.Class;

namespace Google_sheetAndro.ViewModels
{
    public class Grouping<K, T> : ObservableCollection<T>
    {
        public K Name { get; private set; }
        public Grouping(K name, IEnumerable<T> items)
        {
            Name = name;
            foreach (T item in items)
                Items.Add(item);
        }
    }
    public class ItemsInfoVM : INotifyPropertyChanged
    {
        //public ObservableCollection<string> Items { get; set; }
        public ItemsInfoVM()
        {
            months = new List<string>(){ "Все", "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
            //Items = new ObservableCollection<string>();
        }
        public ObservableCollection<Grouping<string, TableItem>> ItemGroups { get { return _ItemGroups; } set { _ItemGroups = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ItemGroups")); } }
        private ObservableCollection<Grouping<string, TableItem>> _ItemGroups;
        bool isBusy;
        private List<string> _years;
        private List<string> _mounths;
        public List<string> years { get { return _years; } set { _years = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("years")); } }
        public List<string> months { get { return _mounths; } set { _mounths = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("months")); } }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }
}

