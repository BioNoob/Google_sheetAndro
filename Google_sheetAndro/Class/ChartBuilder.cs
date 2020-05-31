using Google_sheetAndro.Class;
using Google_sheetAndro.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Google_sheetAndro
{
    public static class Options
    {
        public static OptionsBuild opt = OptionsBuild.Default;
    }
    public static class ChartBuilder
    {

        //private static List<Microcharts.Entry> PlotBar(Dictionary<string, Dictionary<string, double>> entryvalue, bool ONEyear = false)
        //{
        //    List<Microcharts.Entry> CHARTlist = new List<Microcharts.Entry>();
        //    //имеем месяц, дата значение. Допустим работаем с температурой

        //    double summ = 0;
        //    int chetchik = 0;
        //    if (ONEyear)
        //    {
        //        foreach (string key in entryvalue.Keys)
        //        {
        //            List<string> list = entryvalue[key].Keys.ToList();
        //            list.Sort();
        //            switch (Options.opt.SortNum)
        //            {
        //                case OptionsBuild.SortingEnum.YearMid:
        //                    foreach (string item in list)
        //                    {
        //                        chetchik++;
        //                        summ += entryvalue[key][item];
        //                    }
        //                    if (chetchik != 0)
        //                        summ /= chetchik;
        //                    CHARTlist.Add(new Microcharts.Entry((float)summ) { Color = dicColor[key], ValueLabel = Math.Round(summ).ToString() });
        //                    //CHARTlist.Add(new Microcharts.Entry(0));
        //                    //schet += Convert.ToInt32(Math.Floor((double)(CHARTlist.Count / 2)));
        //                    //CHARTlist[schet - 1].Label = key;
        //                    break;
        //                case OptionsBuild.SortingEnum.YearEvery:
        //                    foreach (string item in list)
        //                    {
        //                        CHARTlist.Add(new Microcharts.Entry((float)entryvalue[key][item]) { Color = dicColor[key], ValueLabel = Math.Round(entryvalue[key][item]).ToString() });
        //                    }
        //                    //CHARTlist.Add(new Microcharts.Entry(0));
        //                    //schet += Convert.ToInt32(Math.Floor((double)(CHARTlist.Count / 2)));
        //                    //CHARTlist[schet - 1].Label = key;
        //                    break;
        //                case OptionsBuild.SortingEnum.YearMax:
        //                    double max = double.MinValue;
        //                    foreach (string item in list)
        //                    {
        //                        if (entryvalue[key][item] > max)
        //                            max = entryvalue[key][item];
        //                    }
        //                    CHARTlist.Add(new Microcharts.Entry((float)max) { Color = dicColor[key], ValueLabel = Math.Round(max).ToString() });
        //                    //CHARTlist.Add(new Microcharts.Entry(0));
        //                    //schet += Convert.ToInt32(Math.Floor((double)(CHARTlist.Count / 2)));
        //                    //CHARTlist[schet - 1].Label = key;
        //                    break;
        //                case OptionsBuild.SortingEnum.YearMin:
        //                    double min = double.MaxValue;
        //                    foreach (string item in list)
        //                    {
        //                        if (entryvalue[key][item] < min)
        //                            min = entryvalue[key][item];
        //                    }
        //                    CHARTlist.Add(new Microcharts.Entry((float)min) { Color = dicColor[key], ValueLabel = Math.Round(min).ToString() });
        //                    //CHARTlist.Add(new Microcharts.Entry(0));
        //                    break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        /*
        //        switch (Options.opt.SortNum)
        //        {
        //            case OptionsBuild.SortingEnum.AllYearMidEvery:
        //                break;
        //            case OptionsBuild.SortingEnum.AllYearMid:
        //                break;
        //            case OptionsBuild.SortingEnum.AllYearMax:
        //                break;
        //            case OptionsBuild.SortingEnum.AllYearMin:
        //                break;
        //        }
        //        */
        //    }
        //    return CHARTlist;
        //}
        private static double GetIntFromVal(string ss)
        {
            Regex mask = new Regex(@"^(?<Num>[+-])?(?<Value>\d*.\d+)?$");
            Match matchres = mask.Match(ss);
            double val = 0;
            if (matchres.Success)
            {
                if (!double.TryParse(matchres.Groups["Value"].Value, out val))
                    return 0;
                if (matchres.Groups["Num"].Success)
                    if (matchres.Groups["Num"].Value == "-")
                        val *= -1;
                return val;
            }
            else
                return 0;
        }
        //private static Dictionary<string, int> counter = new Dictionary<string, int>();
        //private static void GetTaskList(string value)
        //{
        //    List<string> vs = new List<string>();
        //    if (value == "")
        //    {
        //        value = "Другие";
        //    }
        //    if (Options.opt.ActiveType == 6)
        //    {
        //        vs = value.Split(',').ToList();
        //        foreach (string item in vs)
        //        {
        //            if (counter.ContainsKey(item))
        //                counter[item]++;
        //            else
        //                counter.Add(item, 1);
        //        }
        //    }
        //    else
        //    {
        //        if (counter.ContainsKey(value))
        //            counter[value]++;
        //        else
        //            counter.Add(value, 1);
        //    }
        //}
        /// <summary>
        /// Для одного года
        /// </summary>
        /// <param name="vlr"></param>
        public static string GetBar(Dictionary<string, IList<IList<object>>> vlr, string year = null)
        {
            //counter.Clear();
            //Microcharts.Chart chtr = new Microcharts.BarChart();
            //месяц лист(число знаечние) 
            string name_bk = "";
            //Dictionary<string, Dictionary<string, double>> approx = new Dictionary<string, Dictionary<string, double>>();
            List<ValueDate> valueDates = new List<ValueDate>();
            List<string> YL = new List<string>();
            YL = vlr.Keys.ToList();
            if (year != null)
                YL = YL.Where(t => t == year).ToList();

            foreach (string item in YL)
            {
                foreach (var cell in vlr[item])
                {
                    string mount_nm = cell[0].ToString();
                    if (mount_nm == "")
                        mount_nm = name_bk;
                    else
                        name_bk = mount_nm;
                    if (cell[0].ToString() != "Мес")
                    {
                        if (cell.Count > 1)
                        {
                            if (cell[1].ToString() != "")
                            {
                                if(LoaderFunction.is_only_user_shown)
                                {
                                    if(cell[11].ToString()!=StaticInfo.AccountEmail)
                                    {
                                        continue;
                                    }
                                }
                                DateTime dtt = new DateTime(1899, 12, 30, new GregorianCalendar());
                                int daybuf = 0;
                                int.TryParse(cell[1].ToString(), out daybuf);
                                //int daybuf = Convert.ToInt32(cell[1].ToString());
                                dtt = dtt.AddDays(daybuf);
                                string dt = dtt.ToString("D", CultureInfo.GetCultureInfo("ru-RU"));
                                //DateTime dt = Convert.ToDateTime(cell[1].ToString());
                                object db = new object();
                                double dbd = 0;
                                if (Options.opt.ActiveType == 2)
                                {
                                    double val = Convert.ToDouble(cell[Options.opt.ActiveType]);
                                    // *24*60
                                    //Time_r tm = new Time_r(form_val[0], form_val[1], form_val[2]);
                                    db = new Time_r(val * 24 * 60 * 60).Min;
                                }
                                else if (Options.opt.ActiveType == 5)
                                {
                                    db = GetIntFromVal(cell[Options.opt.ActiveType].ToString());
                                }
                                else if (Options.opt.ActiveType != 6 && Options.opt.ActiveType != 10)
                                {
                                    if (!double.TryParse(cell[Options.opt.ActiveType].ToString(), out dbd))
                                        db = 0;
                                    else
                                        db = dbd;
                                }
                                else if (Options.opt.ActiveType == 10)
                                {
                                    if (cell.Count < 11)
                                        db = "";
                                    else
                                        db = cell[Options.opt.ActiveType].ToString();
                                }
                                else if (Options.opt.ActiveType == 6)
                                {
                                    db = cell[Options.opt.ActiveType].ToString().Replace('.', ',');
                                }
                                valueDates.Add(new ValueDate(dtt, db));
                            }
                        }
                    }
                }
            }
            if (YL.Count > 1)
                return imgChartBuilder.FormerPost(valueDates, vlr.Keys.ToList());
            else
                return imgChartBuilder.FormerPost_year(valueDates);
            /*
            if (approx.ContainsKey(mount_nm))
            {
                if (approx[mount_nm].ContainsKey(dt))
                    approx[mount_nm][dt] += db;
                else
                    approx[mount_nm].Add(dt, db);
            }
            else
            {
                approx.Add(mount_nm, new Dictionary<string, double>());
                approx[mount_nm].Add(dt, db);
            }
            //chtr.Entries = PlotBar(approx, true);
        }
        else
        {
            approx.Add(mount_nm, new Dictionary<string, double>());
            approx[mount_nm].Add(mount_nm, 0);
        }

    }

}
if (Options.opt.ActiveType != 6 && Options.opt.ActiveType != 10)
    Debug.Write("Место для формирования графика");
//chtr.Entries = PlotBar(approx, true);
else
{
    List<Microcharts.Entry> CHARTlist = new List<Microcharts.Entry>();
    Random rnd = new Random();
    foreach (string item in counter.Keys)
    {
        //Debug.WriteLine(item + "  " + counter[item]);
        //string color = String.Format("#{0:X6}", rnd.Next(0x1000000));
        //CHARTlist.Add(new Microcharts.Entry((float)counter[item]) { Color = SkiaSharp.SKColor.Parse(color), ValueLabel = item });
    }
    chtr.Entries = CHARTlist;
}
return chtr;
//approx.OrderBy(key => key.Value);
*/
        }

        /// <summary>
        /// Для всех годов
        /// </summary>
        /// <param name="vlr"></param>
        //    public static Microcharts.Chart GetBar(Dictionary<string, ValueRange> vlr)
        //    {
        //        counter.Clear();
        //        Microcharts.Chart chtr = new Microcharts.BarChart();
        //        //месяц лист(число знаечние) 
        //        string name_bk = "";
        //        Dictionary<string,Dictionary<string, Dictionary<string, double>>> approx = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
        //        foreach (string item_vlr in vlr.Keys)
        //        {
        //            approx.Add(item_vlr, new Dictionary<string, Dictionary<string, double>>());
        //            ValueRange vlr_i = vlr[item_vlr];
        //            foreach (var cell in vlr_i.Values)//массив строк 1 строка к многим ячейкам)
        //            {

        //                string mount_nm = cell[0].ToString();
        //                if (mount_nm == "")
        //                    mount_nm = name_bk;
        //                else
        //                    name_bk = mount_nm;
        //                if (cell[0].ToString() != "Мес")
        //                {
        //                    if (cell.Count > 1)
        //                    {
        //                        DateTime dtt = new DateTime(1899, 12, 30, new GregorianCalendar());
        //                        int daybuf = Convert.ToInt32(cell[1].ToString());
        //                        dtt = dtt.AddDays(daybuf);
        //                        string dt = dtt.ToString("D", CultureInfo.GetCultureInfo("ru-RU"));
        //                        double db = 0;

        //                        if (Options.opt.ActiveType == 5)
        //                        {
        //                            db = GetIntFromVal(cell[Options.opt.ActiveType].ToString());
        //                        }
        //                        else if (Options.opt.ActiveType != 6 && Options.opt.ActiveType != 10)
        //                        {
        //                            if (!double.TryParse(cell[Options.opt.ActiveType].ToString(), out db))
        //                                db = 0;
        //                        }
        //                        else
        //                        {
        //                            if(cell.Count < 11)
        //                            {
        //                                GetTaskList("");
        //                            }
        //                            else
        //                            {
        //                                GetTaskList(cell[Options.opt.ActiveType].ToString());
        //                                continue;
        //                            }
        //                        }
        //                        Dictionary<string, double> dic = new Dictionary<string, double> { { dt, db } };

        //                        if (approx[item_vlr].ContainsKey(mount_nm))
        //                        {
        //                            if (approx[item_vlr][mount_nm].ContainsKey(dt))
        //                                approx[item_vlr][mount_nm][dt] += db;
        //                            else
        //                                approx[item_vlr][mount_nm].Add(dt, db);
        //                        }
        //                        else
        //                        {
        //                            approx[item_vlr].Add(mount_nm, new Dictionary<string, double>());
        //                            approx[item_vlr][mount_nm].Add(dt, db);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        approx[item_vlr].Add(mount_nm, new Dictionary<string, double>());
        //                        approx[item_vlr][mount_nm].Add(mount_nm, 0);
        //                    }
        //                }
        //            }
        //        }
        //        Dictionary<string, double> outter = new Dictionary<string, double>();
        //        List<Microcharts.Entry> CHARTlist = new List<Microcharts.Entry>();
        //        switch (Options.opt.SortNum)
        //        {
        //            case OptionsBuild.SortingEnum.AllYearMidEvery:
        //                foreach (string year in approx.Keys)//перебор года
        //                {
        //                    foreach (string mounth in approx[year].Keys)//перебор месяца
        //                    {
        //                        foreach (string date in approx[year][mounth].Keys)
        //                        {
        //                            if (outter.ContainsKey(mounth))
        //                                outter[mounth] += approx[year][mounth][date];
        //                            else
        //                                outter.Add(mounth, approx[year][mounth][date]);
        //                        }
        //                    }
        //                }
        //                break;
        //            case OptionsBuild.SortingEnum.AllYearMid:
        //                int chet = 0;
        //                foreach (string year in approx.Keys)//перебор года
        //                {
        //                    foreach (string mounth in approx[year].Keys)//перебор месяца
        //                    {
        //                        foreach (string date in approx[year][mounth].Keys)//перебор месяца
        //                        {
        //                            chet++;
        //                            if (outter.ContainsKey(mounth))
        //                                outter[mounth] += approx[year][mounth][date];
        //                            else
        //                                outter.Add(mounth, approx[year][mounth][date]);
        //                        }
        //                        if (chet == 0)
        //                            outter[mounth] = 0;
        //                        else
        //                            outter[mounth] = outter[mounth] / chet;
        //                        chet = 0;
        //                    }
        //                }
        //                break;
        //            case OptionsBuild.SortingEnum.AllYearMax:
        //                double max = double.MinValue;
        //                foreach (string year in approx.Keys)//перебор года
        //                {
        //                    foreach (string mounth in approx[year].Keys)//перебор месяца
        //                    {
        //                        foreach (string date in approx[year][mounth].Keys)//перебор месяца
        //                        {
        //                            double val = approx[year][mounth][date];
        //                            if(val > max)
        //                            {
        //                                max = val;
        //                                if (outter.ContainsKey(mounth))
        //                                    outter[mounth] = val;
        //                                else
        //                                    outter.Add(mounth, val);
        //                            }
        //                        }
        //                        max = double.MinValue;
        //                    }
        //                }
        //                break;
        //            case OptionsBuild.SortingEnum.AllYearMin:
        //                double min = double.MaxValue;
        //                foreach (string year in approx.Keys)//перебор года
        //                {
        //                    foreach (string mounth in approx[year].Keys)//перебор месяца
        //                    {
        //                        foreach (string date in approx[year][mounth].Keys)//перебор месяца
        //                        {
        //                            double val = approx[year][mounth][date];
        //                            if (val < min)
        //                            {
        //                                min = val;
        //                                if (outter.ContainsKey(mounth))
        //                                    outter[mounth] = val;
        //                                else
        //                                    outter.Add(mounth, val);
        //                            }
        //                        }
        //                        min = double.MaxValue;
        //                    }
        //                }
        //                break;
        //            case OptionsBuild.SortingEnum.AllYearCount:
        //                break;
        //        }
        //        foreach (string item in outter.Keys)
        //        {
        //            CHARTlist.Add(new Microcharts.Entry((float)outter[item]) { Color = dicColor[item], ValueLabel = item });
        //        }
        //        chtr.Entries = CHARTlist;
        //        return chtr;
        //    }


    }
}
