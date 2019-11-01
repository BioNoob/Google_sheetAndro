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
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RefreshSample.ViewModels
{
    public class WheatherViewModel : INotifyPropertyChanged
    {
        //public ObservableCollection<string> Items { get; set; }
        public WheatherViewModel()
        {
            Time = DateTime.Now.ToString("HH:mm dd MMMM yyyy");
            LoaderFunction.DoWheatherLoad += LoaderFunction_DoWheatherLoad;
        }

        private void LoaderFunction_DoWheatherLoad()
        {
            gpp = StaticInfo.Wheather;
            Place = StaticInfo.Place;
            Val = gpp.getParams();
            string key = Searcher(StaticInfo.Pos);
            lw = kek(key);
            Airport = key;
            ActualDate = lw.First().Date;
            ActualWind = lw.First().Wind;
        }
        public string Airport { get => airport; set { airport = value; OnPropertyChanged("Airport");} }
        public ResponsedData gpp { get => gpp1; set { gpp1 = value; OnPropertyChanged("gpp");  } }
        public Dictionary<string, string> Val { get => val; set { val = value; OnPropertyChanged("Val");  } }
        public string Place { get => place; set { place = value; OnPropertyChanged("Place"); } }
        public string Time { get => time; set { time = value; OnPropertyChanged("Time"); } }
        public string ActualDate { get => actualDate; set { actualDate = value; OnPropertyChanged("ActualDate");} }
        public List<windout> lw { get => lw1; set { lw1 = value; OnPropertyChanged("lw");  } }
        public List<Winder> ActualWind { get => actualWind; set { actualWind = value; OnPropertyChanged("ActualWind");  } }
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
        private List<windout> lw1;
        private string actualDate;
        private string time;
        private string place;
        private Dictionary<string, string> val;
        private ResponsedData gpp1;
        private string airport;
        private List<Winder> actualWind;

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
                  //caller();
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
        public List<windout> kek(string key)
        {
            var url = $"http://meteocenter.asia/?m=gcc&p={key}";
            var web = new HtmlWeb();
            web.AutoDetectEncoding = false;
            //Encoding.RegisterProvider()
            // var t = Encoding.GetEncodings();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            web.OverrideEncoding = Encoding.GetEncoding("windows-1251");
            var doc = web.Load(url);
            List<windout> lw = ParseAllTables(doc);
            lw = lw.Where(t => t.DateFormat >= DateTime.Now && t.DateFormat <= DateTime.Now.AddDays(1)).ToList();
            return lw;
        }
        public List<windout> ParseAllTables(HtmlDocument doc)
        {
            var rows = doc.DocumentNode.Descendants("tr");
            Regex rgT = new Regex("^[0-9]{2}$");
            Regex rgD = new Regex(@"^([0-9]{2}\.){2}[0-9]+$");
            Regex selc = new Regex(@"(?<Hi>(\([0-9]+.м\))+).(?<asim>[0-9]{3})\/(?<speed>[0-9]{1,3}).(?<temp>(\-)?[0-9]{1,2})");
            windout ww;
            List<windout> outlist = new List<windout>();
            foreach (var row in rows.Skip(1))
            {
                ww = new windout();
                var row_rw = row.Descendants("td");
                string Hour = string.Empty;
                string Data = string.Empty;
                string Wind = string.Empty;
                string temper = string.Empty;
                string asim = string.Empty;
                foreach (var cell in row_rw)
                {
                    var text = cell.InnerText;
                    if (rgT.IsMatch(text) && Hour == string.Empty)
                    {
                        Hour = text;
                        continue;
                    }
                    else if (rgD.IsMatch(text) && Data == string.Empty)
                    {
                        Data = text;
                        continue;
                    }
                    else if (text.Contains("...") && text.Contains("FL"))
                    {
                        Wind = text.Trim().Replace("...", "").Trim();
                        List<Winder> wr = new List<Winder>();
                        int i = 0;
                        foreach (Match item in selc.Matches(Wind))
                        {
                            Winder wind = new Winder();
                            wind.Asim = item.Groups["asim"].Value;
                            wind.Wind = item.Groups["speed"].Value;
                            wind.Temp = item.Groups["temp"].Value;
                            wind.hi = item.Groups["Hi"].Value;
                            wind.chet = i;
                            i++;
                            wr.Add(wind);
                        }
                        ww.Wind = wr;
                        continue;
                    }
                }
                if (Hour != string.Empty && Data != string.Empty && Wind != string.Empty)
                {
                    ww.Date = Hour + ";" + Data;
                    outlist.Add(ww);
                }
            }
            return outlist;
        }
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public string Searcher(Location cur)
        {
            Dictionary<string, Location> data_import = new Dictionary<string, Location>();
            //string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Google_sheetAndro.aopa-points-export.csv");
            var assembly = Assembly.GetExecutingAssembly();
            //string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "aopa-points-export.csv");
            //var assembly = IntrospectionExtensions.GetTypeInfo(typeof(LoadResourceText)).Assembly;
            //Stream stream = assembly.GetManifestResourceStream("WorkingWithFiles.LibTextResource.txt");
            Stream stream = assembly.GetManifestResourceStream("Google_sheetAndro.aopa-points-export.csv");

            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    double lat = 0;
                    double lon = 0;
                    lat = Convert.ToDouble(values[2], CultureInfo.InvariantCulture);
                    lon = Convert.ToDouble(values[1], CultureInfo.InvariantCulture);
                    data_import.Add(values[0], new Location(lat, lon));
                }
            }
            double step = 10;
            double step_step = 1;
            while (data_import.Count > 5)
            {
                //data_import = data_import.Where(t => Math.Abs(t.Value.Lat - cur.Lat) < step && Math.Abs(t.Value.Lon - cur.Lon) < step).ToDictionary(t => t.Key, t => t.Value);
                data_import = data_import.Where(t => Location.CalculateDistance(t.Value, cur, DistanceUnits.Kilometers) < step).ToDictionary(t => t.Key, t => t.Value);
                if (step == 1)
                {
                    step_step = 0.05;
                    step -= step_step;
                }
                else if (step <= 0.05)
                {
                    step_step = 0.005;
                    step -= step_step;
                }
                else
                {
                    step -= step_step;
                }
            }
            double dist = double.MaxValue;
            string minkey = "";
            foreach (var item in data_import)
            {
                double buf = Location.CalculateDistance(item.Value, cur, DistanceUnits.Kilometers);
                if (buf < dist)
                {
                    dist = buf;
                    minkey = item.Key;
                }
            }
            return minkey;
        }
    }
}

