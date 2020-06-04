using Google_sheetAndro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Google_sheetAndro.Class
{
    //list.Add("date", Date_pick.Date.ToUniversalTime() /*+ TimeZoneInfo.GetUtcOffset(Date_pick.Date.ToUniversalTime())*/); //фикс для времени по локальному
    //list.Add("time", Time_pick.Text);
    //list.Add("wind", Convert.ToDouble(Wind_Num.Text));
    //list.Add("cloud", CloudPicker.SelectedItem.ToString());
    //list.Add("temp", Convert.ToDouble(Temp_Num.Text));
    //list.Add("task", Task_txt.Text);
    //list.Add("height", Convert.ToDouble(Hight_txt_num.Text));
    //list.Add("range", Convert.ToDouble(Range_txt.Text));
    //list.Add("plase", Place_txt.SelectedItem.ToString());
    //list.Add("comment", Comment_txt.Text);
    public class ObservableGroupCollection<S, T> : ObservableCollection<T>
    {
        private readonly S _key;

        public ObservableGroupCollection(IGrouping<S, T> group)
            : base(group)
        {
            _key = group.Key;
        }

        public S Key
        {
            get { return _key; }
        }
    }
    public static class LocalTable
    {
        public static List<TableItem> ListItems = new List<TableItem>();
        public static Dictionary<string, IList<IList<object>>> SheetsVal = new Dictionary<string, IList<IList<object>>>();
        public static List<string> GetYearsList()
        {
            var tt = new HashSet<string>(ListItems.Select(t => t.year.ToString()).ToList()).ToList();
            //tt.OrderByDescending(t => t);
            tt.Sort();
            return tt;
        }
        public static ObservableCollection<Grouping<string, TableItem>> SortItems(string Year, int mouth, bool owner_use)
        {
            //ObservableCollection<Grouping<string, TableItem>> groupedData = new ObservableCollection<Grouping<string, TableItem>>();
            List<Grouping<string, TableItem>> kk;
            if (owner_use)
            {
                switch (mouth)
                {
                    case 0:
                        kk = ListItems.Where(p => p.year.ToString() == Year && p.author == StaticInfo.AccountEmail)
                        .GroupBy(p => p.mounth)
                        .Select(g => new Grouping<string, TableItem>(g.Key, g))
                        .ToList();
                        break;
                    default:
                        kk = ListItems.Where(p => p.year.ToString() == Year && p.date.Month == mouth && p.author == StaticInfo.AccountEmail)
                        .GroupBy(p => p.mounth)
                        .Select(g => new Grouping<string, TableItem>(g.Key, g))
                        .ToList();
                        break;
                }
            }
            else
            {
                switch (mouth)
                {
                    case 0:
                        kk = ListItems.Where(p => p.year.ToString() == Year)
                        .GroupBy(p => p.mounth)
                        .Select(g => new Grouping<string, TableItem>(g.Key, g))
                        .ToList();
                        break;
                    default:
                        kk = ListItems.Where(p => p.year.ToString() == Year && p.date.Month == mouth)
                        .GroupBy(p => p.mounth)
                        .Select(g => new Grouping<string, TableItem>(g.Key, g))
                        .ToList();
                        break;
                }
            }
            return new ObservableCollection<Grouping<string, TableItem>>(kk);
        }
        public static List<TableItem> getitemsbyyear(int year)
        {
            var kk = ListItems.Where(y => y.year == year).ToList();
            return kk;
        }
    }
    public class TableItem
    {
        public TableItem()
        {
            //date = DateTime.Now;
            //time = "00:00:00";
            //wind = 0;
            //cloud = "";
            //temp = 0;
            //task = "";
            //height = 0;
            //plase = "";
            //comment = "";
            //tabelplase = "";
            //exect_mounth = "";
            //sh_id = 0;
            //row_nb = 0;
        }
        public int year { get { return date.Year; } }
        public string mounth { get { return date.ToString("MMMM"); } }
        public DateTime date { get; set; }
        public string time { get; set; }
        public double wind { get; set; }
        public string cloud { get; set; }
        public double temp { get; set; }
        public string task { get; set; }
        public double height { get; set; }
        public double range { get; set; }
        public string plase { get; set; }
        public string comment { get; set; }
        public string exect_mounth { get; set; }
        public int sh_id { get; set; }
        public string points { get; set; }
        public string route { get; set; }

        public string tabelplase { get; set; }
        public int row_nb { get; set; }
        public int row_nb_end { get; set; }
        public int row_mounth_firs { get; set; }


        public string author { get; set; }

        //public int Col_vo_zapr()
        //{
        //    double answ = this.lenght_point_row() / 45000;
        //    if (answ > 1)
        //    {
        //        return (int)Math.Ceiling(answ);
        //    }
        //    else
        //        return 1;
        //}
        public enum CompareStatus
        {
            equal,
            position,
            more
        }
        public static bool operator ==(TableItem x, TableItem y)
        {
            foreach (PropertyInfo propertyInfo in x.GetType().GetProperties())
            {
                // do stuff here
                //if (propertyInfo.GetValue(x) != propertyInfo.GetValue(y))
                //{
                var t = propertyInfo.GetValue(x);
                var q = propertyInfo.GetValue(y);
                if (!t.Equals(q))
                    return false;
                //}

            }
            return true;
        }
        public static bool operator !=(TableItem x, TableItem y)
        {
            foreach (PropertyInfo propertyInfo in x.GetType().GetProperties())
            {
                // do stuff here
                var t = propertyInfo.GetValue(x);
                var q = propertyInfo.GetValue(y);
                if (t.Equals(q))
                    return false;
            }
            return true;
        }
        public CompareStatus Comparer(TableItem ti)
        {
            if (ti == this)//не факт
            {
                return CompareStatus.equal;
            }
            var q = (ti.row_nb != row_nb | ti.row_nb_end != row_nb_end | ti.row_mounth_firs != row_mounth_firs |
                ti.tabelplase != tabelplase);
            var bq = (ti.year == year & ti.mounth == mounth & ti.date == date & ti.time == time &
                ti.wind == wind & ti.cloud == cloud & ti.temp == temp & ti.task == task & ti.height == height & ti.range == range &
                ti.plase == plase & ti.comment == comment & ti.exect_mounth == exect_mounth & ti.sh_id == sh_id & ti.points == points & ti.route == route);
            if (q & bq)
            {
                return CompareStatus.position;
            }
            else
            {
                return CompareStatus.more;
            }

        }
        public List<object> GetListForEntry()
        {
            List<object> lst = new List<object>();
            lst.AddRange(new List<object>() { this.date.ToString("dd/MM/yyyy"), this.time, this.wind, this.cloud, this.temp, this.task, this.height, this.range, this.plase, this.comment, this.author, this.points, this.route });
            return lst;
        }
        public List<object> GetListForEntry_ex_points_route()
        {
            List<object> lst = new List<object>();
            lst.AddRange(new List<object>() { this.date.ToString("dd/MM/yyyy"), this.time, this.wind, this.cloud, this.temp, this.task, this.height, this.range, this.plase, this.comment, this.author });
            return lst;
        }
        public int GetMaxRowFor_ro_po()
        {
            var tt = this.points.Length;
            int q1 = (int)Math.Ceiling(tt / 49000.0);
            tt = this.route.Length;
            int q2 = (int)Math.Ceiling(tt / 49000.0);
            if (q1 > q2) return q1;
            else return q2;

        }
        public List<IList<object>> GetVal_points_route()
        {

            int chet = 0;
            int iter = 49000;
            var tt_pt = this.points.Length;
            int count_pt = (int)Math.Ceiling(tt_pt / 49000.0);
            var tt_rt = this.route.Length;
            int count_rt = (int)Math.Ceiling(tt_rt / 49000.0);
            List<IList<object>> val;
            var ct = 0;
            if (count_pt > count_rt)
            {
                val = new List<IList<object>>(count_pt);
                ct = count_pt;
            }
            else
            {
                val = new List<IList<object>>(count_rt);
                ct = count_rt;
            }
            for (int i = 0; i < ct; i++)
            {
                val.Add(new List<object>());
                val[i].Add(new List<string>());
                val[i].Add(new List<string>());
                val[i][0] = "";
                val[i][1] = "";
            }

            for (int i = 0; i < count_pt; i++)
            {
                List<object> lst = new List<object>();
                if (i + 1 == count_pt)
                {
                    iter = tt_pt - chet;
                }
                var tm1 = this.points.Substring(chet, iter);
                //lst.Add(tm1);
                chet += iter;
                val[i][0] = tm1;
                //val.Add(lst);
            }
            chet = 0;
            iter = 49000;
            for (int i = 0; i < count_rt; i++)
            {
                List<object> lst = new List<object>();
                if (i + 1 == count_rt)
                {
                    iter = tt_rt - chet;
                }
                var tm1 = this.route.Substring(chet, iter);
                chet += iter;
                val[i][1] = tm1;
            }
            //for (int i = 0; i < ct; i++)
            //{
            //    var q = val[i][0].ToString();
            //    var qq = val[i][1].ToString();
            //    val[i][1].
            //    if (string.IsNullOrEmpty(val[i][0].ToString()))
            //    {
            //        val[i][0] = "";

            //    }
            //    if (string.IsNullOrEmpty(val[i][1].ToString()))
            //    {
            //        val[i][1] = "";
            //    }
            //}
            return val;
        }
    }

}
