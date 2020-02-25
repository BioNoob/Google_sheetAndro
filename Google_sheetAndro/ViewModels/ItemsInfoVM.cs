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

using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Google_sheetAndro.ViewModels
{
    public class ItemsInfoVM : INotifyPropertyChanged
    {
        //public ObservableCollection<string> Items { get; set; }
        public ItemsInfoVM()
        {
            months = new List<string>() { "Все", "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
            //Items = new ObservableCollection<string>();
        }
        public ObservableCollection<Grouping<string, TableItem>> ItemGroups { get { return _ItemGroups; } set { _ItemGroups = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ItemGroups")); } }
        private ObservableCollection<Grouping<string, TableItem>> _ItemGroups;
        //bool isBusy;
        private List<string> _years;
        private List<string> _mounths;
        public List<string> years
        {
            get
            {
                return _years;
            }
            set
            {
                _years = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("years"));
            }
        }
        public List<string> months { get { return _mounths; } set { _mounths = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("months")); } }
        string _selectedyear;
        public string selectedyear
        {
            get => _selectedyear; set
            {
                if (value == null)
                {
                    _selectedyear = years[years.Count - 1];
                }
                else
                {
                    _selectedyear = value;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("selectedyear"));
            }
        }
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }
}

