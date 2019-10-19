using System;
using System.Collections.Generic;
using System.Text;

namespace Google_sheetAndro.Class
{
    public class ResponsedData
    {

        public DateTime duration { get; set; }
        public float temperature { get; set; }
        public float pressure { get; set; }
        private string _source;

        public float windSpeed { get; set; }
        public float windGust { get; set; }
        public float cloudCover { get; set; }
        public float windBearing { get; set; }
        public float humidity { get; set; }
        public string precipType { get; set; }
        public float precipIntensity { get; set; }
        public float precipProbability { get; set; }
        public float visibility { get; set; }

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
                        _source = value.Replace('-','_') + ".png";
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
