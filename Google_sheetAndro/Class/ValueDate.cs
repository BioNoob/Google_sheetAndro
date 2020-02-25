using System;

namespace Google_sheetAndro.Class
{
    public class ValueDate
    {
        private DateTime _dateTime;
        private object _val;
        public object Value { get => _val; set => _val = value; }
        public DateTime date { get => _dateTime; set => _dateTime = value; }
        public string Year { get => date.Year.ToString(); }
        static string[] months = { "январь", "февраль", "март", "апрель", "май", "июнь", "июль", "август", "сентябрь", "октябрь", "ноябрь", "декабрь" };
        public string Mounth
        {
            get
            {
                return months[date.Month - 1];
            }
        }
        public int Day { get => date.Day; }
        public ValueDate(DateTime _date, object _val)
        {
            date = _date;
            Value = _val;
        }
        public static ValueDate Default(int Year, string Mounth)
        {
            int mont = Array.IndexOf(months, Mounth) + 1;
            double val = 0;
            return new ValueDate(new DateTime(Year, mont, 1), val);
        }
        public ValueDate(int Year, int Mounth, int Day, object _val)
        {
            date = new DateTime(Year, Mounth, Day);
            Value = _val;
        }
    }
}
