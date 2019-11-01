using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Google_sheetAndro.Class
{
    //public class tempers
    //{
    //    private float temper;

    //    public float Temper { get => temper; set => temper = value; }

    //    public override string ToString()
    //    {
    //        if (temper > 0)
    //            return  "+" + temper.ToString();
    //        else
    //            return temper.ToString();
    //    }
    //}
    public class ResponsedData
    {

        public DateTime duration { get; set; }
        public float temperature { get => temperature1; set => temperature1 = value; }
        public float pressure { get; set; }
        private string _source;
        private float temperature1;

        public float windSpeed { get; set; }
        public float windGust { get; set; }
        public float cloudCover { get; set; }
        public float windBearing { get; set; }
        public float humidity { get; set; }
        public string precipType { get; set; }
        public float precipIntensity { get; set; }
        public float precipProbability { get; set; }
        public float visibility { get; set; }

        public Dictionary<string, string> getParams()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string temp_sign = string.Empty;
            if (temperature > 0)
                temp_sign = "+";
            dic.Add("temperature", temp_sign + Math.Round(temperature).ToString() + " C°");
            dic.Add("windSpeed", string.Format("{0:#0.# м/с}", windSpeed));
            dic.Add("pressure", string.Format("{0:#0 hPa}", pressure));
            if (cloudCover <= 0.0)
                dic.Add("cloudCover", "нет");
            else if (cloudCover >= 0.1 && cloudCover <= 0.51)
                dic.Add("cloudCover", "низкая");
            else
                dic.Add("cloudCover", "высокая");
            dic.Add("windGust", string.Format("{0:#0.# м/с}", windGust));
            dic.Add("humidity", string.Format(CultureInfo.InvariantCulture, "{0:P1}", humidity));
            dic.Add("precipIntensity", string.Format("{0:#0.# ?}", precipIntensity));
            dic.Add("visibility", string.Format("{0:#0.# км}", visibility));
            dic.Add("precipProbability", string.Format(CultureInfo.InvariantCulture, "{0:P1}", precipProbability));
            dic.Add("windBearing", string.Format("{0:#0 °}", windBearing));
            return dic;
        }
        public ResponsedData()
        {

        }
        public string icon
        {
            get { return _source; }
            set
            {
                switch (value)
                {
                    case "clear-day":
                    case "clear-night":
                    case "partly-cloudy-day":
                    case "partly-cloudy-night":
                        _source = value.Replace('-', '_') + ".png";
                        break;
                    case "rain":
                    case "snow":
                    case "sleet":
                    case "wind":
                    case "fog":
                    case "cloudy":
                        _source = value + ".png";
                        break;
                    default:
                        _source = "def.png";
                        break;
                }
            }
        }
    }
}
