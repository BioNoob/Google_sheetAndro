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

using Android.Widget;
using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace RefreshSample.ViewModels
{
    public class WheatherViewModel : INotifyPropertyChanged
    {
        //public ObservableCollection<string> Items { get; set; }
        public WheatherViewModel()
        {
            //Time = DateTime.Now.ToString("dd MMMM, HH:mm");
            LoaderFunction.DoWheatherLoad += LoaderFunction_DoWheatherLoad;
        }

        private async void LoaderFunction_DoWheatherLoad()
        {
            gpp = StaticInfo.Wheather;
            Place = StaticInfo.Place;
            LastReq = DateTime.Now;
            Val = gpp.getParams();

            Time = gpp.time;
            LoaderFunction.SetterStatus("Ищем ближайшую летную площаку");
            string key = Searcher(StaticInfo.Pos);
            LoaderFunction.SetterStatus("Получаем погодные данные летной площадки");
            lw = await kek(key);
            if (lw != null)
            {
                if (lw.Count != 0)
                {
                    Airport = key;
                    ActualDate = lw.First().DateFormat;
                    ActualWind = lw.First().Wind;
                }
            }
            IsBusy = false;
        }
        public bool ErrorVisual { get; set; }
        public string ErrorStatus
        {
            get => errorstatus;
            set
            {
                errorstatus = value;
                if (String.IsNullOrEmpty(value))
                    ErrorVisual = false;
                else
                    ErrorVisual = true;
                OnPropertyChanged("ErrorStatus");
                OnPropertyChanged("ErrorVisual");
            }
        }
        public string Airport { get => airport; set { airport = value; OnPropertyChanged("Airport"); } }
        public ResponsedData gpp { get => gpp1; set { gpp1 = value; OnPropertyChanged("gpp"); } }
        public Dictionary<string, string> Val { get => val; set { val = value; OnPropertyChanged("Val"); } }
        public string Place { get => place; set { place = value; OnPropertyChanged("Place"); } }
        public string Time { get => time; set { time = value; OnPropertyChanged("Time"); } }
        public DateTime ActualDate { get => actualDate; set { actualDate = value; OnPropertyChanged("ActualDate"); } }
        public List<windout> lw { get => lw1; set { lw1 = value; OnPropertyChanged("lw"); } }
        public List<Winder> ActualWind { get => actualWind; set { actualWind = value; OnPropertyChanged("ActualWind"); } }
        bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                Debug.WriteLine(value);
                OnPropertyChanged("IsBusy");
            }
        }
        private List<windout> lw1;
        private string errorstatus;
        private DateTime actualDate;
        private string time;
        private string place;
        private Dictionary<string, string> val;
        private ResponsedData gpp1;
        private string airport;
        private List<Winder> actualWind;
        public void ExecuteRefreshCommand()
        {
            IsBusy = true;
            try
            {
                caller();
            }
            catch (Exception)
            {
                ErrorStatus = "Сервис погоды недоступен";
                IsBusy = false;
            }
            finally
            {
                //IsBusy = false;
            }

        }
        private async void caller()
        {
            DateTime buf = DateTime.Now;
            if (LastReq != DateTime.MinValue)//TEST ON NULL
            {
                TimeSpan tms = buf - LastReq;

                if (tms.TotalSeconds < 60)
                {
                    Toast.MakeText(Android.App.Application.Context, "Последнее обновление менее минуты назад. Подождите!", ToastLength.Long).Show();
                    IsBusy = false;
                    return;
                }
            }
            else
            {
                LastReq = DateTime.Now;
            }
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(5000);
            try
            {
                await StaticInfo.GetWeatherReqAsync(StaticInfo.Pos, cts.Token);
            }
            catch (OperationCanceledException)
            {
                ErrorStatus = "Время запроса истекло. Сервис погоды недоступен";
                //resultsTextBox.Text += "\r\nDownloads canceled.\r\n";
            }
            catch (Exception)
            {
                ErrorStatus = $"Сервис погоды недоступен";
                //resultsTextBox.Text += "\r\nDownloads failed.\r\n";
            }


            //gpp = StaticInfo.Wheather;
        }
        private DateTime LastReq = DateTime.MinValue;
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        public async Task<List<windout>> kek(string key)
        {
            var url = $"http://meteocenter.asia/?m=gcc&p={key}";
            var web = new HtmlWeb();
            web.AutoDetectEncoding = false;
            CancellationTokenSource cts = new CancellationTokenSource();
            try
            {
                cts.CancelAfter(20000);
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                web.OverrideEncoding = Encoding.GetEncoding("windows-1251");
                var doc = await web.LoadFromWebAsync(url, cts.Token);
                List<windout> lw11 = ParseAllTables(doc);
                lw11 = lw11.Where(t => t.DateFormat >= DateTime.Now && t.DateFormat <= DateTime.Now.AddDays(1)).ToList();
                if (lw11.Count == 0)
                {
                    ErrorStatus = "Данные с Meteocenter.asia по ближайшему объкту на настоящее время отсутствуют";
                    return null;
                }
                //Time = LastReq.ToString("dd MMMM, HH:mm");
                ErrorStatus = null;
                return lw11;
            }
            catch (OperationCanceledException)
            {
                ErrorStatus = "Время запроса истекло. Meteocenter.asia недостпуен";
                LoaderFunction.SetterStatus(ErrorStatus);
                return null;
                //resultsTextBox.Text += "\r\nDownloads canceled.\r\n";
            }
            catch (Exception ex)
            {
                ErrorStatus = $"Ошибка. Неудалось расшифровать данные по аэродрому {key}";
                LoaderFunction.SetterStatus(ErrorStatus);
                Debug.WriteLine(ex.Message);
                return null;
                //resultsTextBox.Text += "\r\nDownloads failed.\r\n";
            }



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
            double step = 100;
            double step_step = 1;
            var data_import = LoaderFunction.GetCSV();
            while (data_import.Count > 5)
            {
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

