using Google_sheetAndro.Models;
using Google_sheetAndro.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            tt.Sort();
            return tt;
        }
        public static ObservableCollection<Grouping<string, TableItem>> SortItems(string Year, int mouth)
        {
            //ObservableCollection<Grouping<string, TableItem>> groupedData = new ObservableCollection<Grouping<string, TableItem>>();
            List<Grouping<string, TableItem>> kk;
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
        public string tabelplase { get; set; }
        public string exect_mounth { get; set; }
        public int sh_id { get; set; }
        public int row_nb { get; set; }


        public string author { get; set; }
        public string points { get; set; }
        public string route { get; set; }
        public List<object> GetListForEntry()
        {
            List<object> lst = new List<object>();
            lst.AddRange(new List<object>() { this.date.ToString("dd/MM/yyyy"), this.time, this.wind, this.cloud, this.temp, this.task, this.height, this.range, this.plase, this.comment,this.author,this.points,this.route });
            return lst;
        }
    }

}
