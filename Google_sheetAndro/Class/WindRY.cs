using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Google_sheetAndro.Class
{
    public class Winder
    {
        public Winder()
        {

        }
        public override string ToString()
        {
            return $"{hi}. Ветер:{Asim}° - {Wind}км/ч ({ConvWindMs()}). Температура {Temp}";
        }
        private double _asim;
        private double _temp;
        private double _wind;
        private string hi1;
        public string Out { get { return this.ToString(); } }
        public int chet { get; set; }
        public string hi { get => hi1.Replace("(", "").Replace(")", ""); set => hi1 = value; }
        public string Wind
        {
            get
            {
                return _wind.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                double t = 0;
                double.TryParse(value, out t);
                _wind = t;
            }
        }
        public string WindForm { get { return $"{Wind} км/ч ({ ConvWindMs()})"; } }
        private string ConvWindMs()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0:#0.#}м/с", _wind * 5 / 18);
        }
        public string Asim
        {
            get
            {
                return _asim.ToString(CultureInfo.InvariantCulture) + "°";
            }
            set
            {
                double t = 0;
                double.TryParse(value, out t);
                _asim = t;
            }
        }
        public string Temp
        {
            get
            {
                if (_temp > 0)
                {
                    return "+" + _temp.ToString(CultureInfo.InvariantCulture);
                }
                else
                    return _temp.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                double t = 0;
                double.TryParse(value, out t);
                _temp = t;
            }
        }
    }
    public class windout
    {
        public windout()
        {
            Wind = new List<Winder>();
        }
        private DateTime _dt;
        private List<Winder> wind;

        public List<Winder> Wind { get => wind.OrderByDescending(wind => wind.chet).ToList(); set => wind = value; }

        public DateTime DateFormat
        {
            get
            {
                return _dt;
            }
        }
        public string time
        {
            get
            {
                return _dt.ToString("HH:mm");
            }
        }
        public string Date
        {
            get
            {
                return _dt.ToString("dd MMMM yyyy");
            }
            set
            {
                //_dt = new DateTime();
                var q = value.Split(';');
                _dt = DateTime.ParseExact(q[1], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                //_dt = Convert.ToDateTime(q[1],CultureInfo.InvariantCulture);
                double x = 0;
                double.TryParse(q[0], out x);
                _dt = _dt.AddHours(x);
            }
        }
    }
}
