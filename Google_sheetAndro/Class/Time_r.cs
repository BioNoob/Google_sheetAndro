using System;

namespace Google_sheetAndro
{
    public class Selectore
    {
        private string name;
        private OptionsBuild.SortingEnum enumer;
        public string Name { get => name; set => name = value; }
        public OptionsBuild.SortingEnum Enumer { get => enumer; set => enumer = value; }
        public Selectore(string _Name, OptionsBuild.SortingEnum ENumSort)
        {
            Name = _Name;
            Enumer = ENumSort;
        }
        public override string ToString()
        {
            return Name;
        }
    }
    public class Selectore_sorting
    {
        private string name;
        private OptionsBuild.EnumTyp enumer;
        public string Name { get => name; set => name = value; }
        public OptionsBuild.EnumTyp Enumer { get => enumer; set => enumer = value; }
        public Selectore_sorting(string _Name, OptionsBuild.EnumTyp ENumSort)
        {
            Name = _Name;
            Enumer = ENumSort;
        }
        public override string ToString()
        {
            return Name;
        }
    }
    public class Time_r
    {
        private double _sec = 0;
        public Time_r()
        {
            _sec = 0;
        }
        public Time_r(double h, double m, double sec)
        {
            _sec = h * 3600 + m * 60 + sec;
        }
        public Time_r(double sec)
        {
            _sec = sec;
        }
        public double Sec
        {
            get
            {
                return _sec;
            }
            set
            {
                _sec = Math.Abs(value);
            }
        }
        public double Min
        {
            get
            {
                return Sec / 60;
            }
            set
            {
                Sec = value * 60;
            }
        }
        public double Hour
        {
            get
            {
                return Sec / 3600;
            }
            set
            {
                Sec = value * 3600;
            }
        }
        public override string ToString()
        {
            double sec = _sec;
            int hour = Convert.ToInt32(Math.Floor(sec / 3600));
            sec -= hour * 3600;
            int min = Convert.ToInt32(Math.Floor(sec / 60));
            sec -= min * 60;
            return string.Format("{0:00}:{1:00}:{2:00}", hour, min, sec);
        }
    }
}
