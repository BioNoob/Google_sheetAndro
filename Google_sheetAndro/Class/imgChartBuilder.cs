﻿using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Google_sheetAndro.Class
{
    public static class imgChartBuilder
    {
        //строим каждый в месяце
        public static string FormerPost_year(List<ValueDate> vl)
        {
            string label = "";
            EveryBuilder.clear();
            List<string> mounthList = new List<string>();
            List<object> valueList = new List<object>();
            List<object> dop = new List<object>();
            int year = Convert.ToInt32(vl.First().Year);
            ValueDate def;
            string value = "";
            string namer = "";
            HashSet<object> unique_items = new HashSet<object>();
            double min = double.MaxValue; double max = double.MinValue;
            string[] months = { "январь", "февраль", "март", "апрель", "май", "июнь", "июль", "август", "сентябрь", "октябрь", "ноябрь", "декабрь" };
            if (Options.opt.ActiveType != 6 && Options.opt.ActiveType != 10)
            {
                min = (double)vl.Min(t => t.Value);
                if (min > 0) min = 0;
                max = (double)vl.Max(t => t.Value);
            }
            mounthList = new HashSet<string>(vl.Select(v => v.Mounth).ToList()).ToList();
            //mounthList.AddRange(months);
            label = string.Join("|", new HashSet<string>(mounthList).ToList());
            foreach (string item in mounthList)
            {
                def = ValueDate.Default(year, item);
                if (vl.FindAll(t => t.Mounth == item).Count < 1)
                    vl.Add(def);
                switch (Options.opt.SortNum)//Options.opt.SortNum)
                {
                    case OptionsBuild.SortingEnum.YearMid:
                        valueList.Add(vl.Where(t => t.Mounth == item).Average(u => (double)u.Value));
                        //value += valueList.Last() + ",";
                        //valueList.Clear();
                        break;
                    case OptionsBuild.SortingEnum.YearEvery:
                        valueList = (from ValueDate vd in vl
                                     where vd.Mounth == item
                                     select vd.Value).ToList();
                        EveryBuilder.SetGroup(item, valueList);
                        //value += string.Join(",", valueList) + "|";
                        valueList.Clear();
                        //valueList = vl.Where(t => t.Mounth == item).ToList().Select(u => u.Value).ToList();
                        break;
                    //нашли максимумы за год по месяцам
                    case OptionsBuild.SortingEnum.YearMax:
                        valueList.Add(vl.Where(t => t.Mounth == item).Max(u => (double)u.Value));
                        //value += valueList.Last() + ",";
                        //valueList.Clear();
                        break;
                    case OptionsBuild.SortingEnum.YearMin:
                        valueList.Add(vl.Where(t => t.Mounth == item).Min(u => (double)u.Value));
                        //value += valueList.Last() + ",";
                        //valueList.Clear();
                        break;
                    case OptionsBuild.SortingEnum.YearCount:
                        //valueList = vl.Where(t => t.Mounth == item).Select(u => u.Value).ToList();
                        valueList = (from ValueDate vd in vl
                                     where vd.Mounth == item
                                     select vd.Value).ToList();
                        EveryBuilder.SetGroup(item, valueList);
                        valueList.Clear();
                        break;
                }
            }
            if (value.Length < 1)
            {
                value = string.Join(",", valueList) + "|";
            }
            value = value.Remove(value.Length - 1, 1);
            if (Options.opt.SortNum == OptionsBuild.SortingEnum.YearCount)
            {
                var kek = EveryBuilder.Getgroup(); //слоаврь месяц значения
                Dictionary<string, List<Dictionary<string, int>>> Num_Mounth_count = new Dictionary<string, List<Dictionary<string, int>>>();
                foreach (var item_k in kek.Keys)
                {
                    var lol1 = kek[item_k].ConvertAll(s => (object)s);
                    var help = Counter_former(lol1);
                    foreach (var num in help.Keys)
                    {
                        var oo = new Dictionary<string, int>();
                        oo.Add(item_k, help[num]);
                        if (Num_Mounth_count.ContainsKey(num))
                            Num_Mounth_count[num].Add(oo);
                        else
                        {
                            Num_Mounth_count.Add(num, new List<Dictionary<string, int>>());
                            Num_Mounth_count[num].Add(oo);
                        }
                    }
                }
                int ind = 0;
                foreach (var item in kek.Keys)
                {
                    foreach (var ite in Num_Mounth_count.Keys)
                    {
                        if(Num_Mounth_count[ite].Where(t => t.Keys.Contains(item)).ToList().Count < 1)
                        {
                            Dictionary<string, int> kk = new Dictionary<string, int>();
                            kk.Add(item, 0);
                            Num_Mounth_count[ite].Insert(ind,kk);
                        }
                        if(!namer.Contains(ite))
                            namer += ite + "|";
                    }
                    ind++;
                }
                //namer.Remove(namer.Length - 1, 1);
                namer = namer.TrimEnd('|');
                foreach (var item in Num_Mounth_count.Keys)
                {
                    foreach (var ite in Num_Mounth_count[item])
                    {
                        foreach (var it in ite.Values)
                        {
                            if (it > max) max = it;
                            if (it < min) min = it;
                            value += it + ",";
                        }
                    }
                    value = value.TrimEnd(',');
                    value += "|";
                }
                value = value.TrimEnd('|');
            }
            if (value == "")
            {
                value = EveryBuilder.GetString();
            }

            if (max == double.MinValue)
                max = 50;
            if (min == double.MaxValue)
                min = 0;
            //додумать функцию градиента? (посмотреть), параметр для отображения размера? зависит от размера итема отображения (или статик?)
            if (namer.Length > 1)
                return $"https://image-charts.com/chart?cht=bvg&chs={Options.opt.Width}x{Options.opt.Height}&chds=a&chg=1,1,0,0&chd=t:{value}&chxt=x,y&chxr=1,{min.ToString()},{max.ToString()}&chxl=0:|{label}&chdl={namer}&chof=.png";
            else
                return $"https://image-charts.com/chart?cht=bvg&chs={Options.opt.Width}x{Options.opt.Height}&chds=a&chg=1,1,0,0&chd=t:{value}&chxt=x,y&chxr=1,{min.ToString()},{max.ToString()}&chxl=0:|{label}&chof=.png";
        }
        public static string FormerPost(List<ValueDate> vl, List<string> years)
        {
            string label = "";
            List<string> mounthList = new List<string>();
            List<object> valueList = new List<object>();
            List<object> valueListOut = new List<object>();
            Dictionary<string, int> help = new Dictionary<string, int>();
            string value = "";
            string namer = "";
            HashSet<object> unique_items = new HashSet<object>();
            double min = double.MaxValue; double max = double.MinValue;
            string[] months = { "январь", "февраль", "март", "апрель", "май", "июнь", "июль", "август", "сентябрь", "октябрь", "ноябрь", "декабрь" };
            if (Options.opt.ActiveType != 6 && Options.opt.ActiveType != 10)
            {
                min = (double)vl.Min(t => t.Value);
                max = (double)vl.Max(t => t.Value);
            }
            mounthList.AddRange(months);
            label = string.Join("|", new HashSet<string>(mounthList).ToList());
            foreach (string item in mounthList)//foreach (string item_year in years)
            {
                foreach (string item_year in years)//foreach (string item in mounthList)
                {
                    switch (Options.opt.SortNum)//Options.opt.SortNum)
                    {
                        case OptionsBuild.SortingEnum.AllYearMid:
                            valueList.Add(vl.Where(t => t.Mounth == item && t.Year == item_year).Average(u => (double)u.Value));
                            break;
                        case OptionsBuild.SortingEnum.AllYearMidEvery:
                            valueList = vl.Where(t => t.Mounth == item && t.Year == item_year).Select(u => u.Value).ToList();
                            break;
                        //нашли максимумы за год по месяцам
                        case OptionsBuild.SortingEnum.AllYearMax:
                            valueList.Add(vl.Where(t => t.Mounth == item && t.Year == item_year).Max(u => (double)u.Value));
                            break;
                        case OptionsBuild.SortingEnum.AllYearMin:
                            valueList.Add(vl.Where(t => t.Mounth == item && t.Year == item_year).Min(u => (double)u.Value));
                            break;
                        case OptionsBuild.SortingEnum.AllYearCount:
                            valueList = vl.Where(t => t.Mounth == item && t.Year == item_year).Select(u => u.Value).ToList(); //пассажиры (9 раз) обучение (3 раза)
                            var _help_dic = Counter_former(valueList);//значение количество (пассажиры, 9)
                            //это значение для СЕНТЯБРЬ 2016, СЕНТЯБРЬ 2017 следует далее
                            foreach (string val in _help_dic.Keys)
                            {
                                if (help.ContainsKey(val))
                                    help[val] += _help_dic[val];
                                else
                                    help.Add(val, _help_dic[val]);
                            }
                            //словарь хелп аккамулирует в себе количество за месяц всех годов
                            break;
                    }
                }
                switch (Options.opt.SortNum)
                {
                    case OptionsBuild.SortingEnum.AllYearMidEvery:
                        valueListOut.Add(valueList.Sum(u => (double)u));
                        break;
                    case OptionsBuild.SortingEnum.AllYearMid:
                        valueListOut.Add(valueList.Average(u => (double)u));
                        break;
                    case OptionsBuild.SortingEnum.AllYearMax:
                        valueListOut.Add(valueList.Max());
                        break;
                    case OptionsBuild.SortingEnum.AllYearMin:
                        valueListOut.Add(valueList.Min());
                        break;
                    case OptionsBuild.SortingEnum.AllYearCount:
                        //нужно вывести в значение собранное за месяц
                        //точнее добавить в выходы: месяц, занчение, подпись
                        double help_min = help.Min(t => t.Value);
                        double help_max = help.Max(t => t.Value);
                        if (help_min < min)
                            min = help_min;
                        if (help_max > max)
                            max = help_max;
                        namer += string.Join(",", help.Keys);
                        value += string.Join(",", help) + "|";
                        help.Clear();
                        break;
                }
                if (Options.opt.SortNum != OptionsBuild.SortingEnum.AllYearCount)
                {
                    double mx = valueListOut.Max(u => (double)u);
                    double mn = valueListOut.Min(u => (double)u);
                    if (mx > max)
                        max = mx;
                    if (mn < min)
                        min = mn;
                    value += string.Join(",", valueListOut) + "|";
                    namer += string.Join(",", years);
                    valueListOut.Clear();
                }
            }
            value = value.Remove(value.Length - 1, 1);
            if (max == double.MinValue)
                max = 50;
            if (min == double.MaxValue)
                min = 0;
            //додумать функцию градиента? (посмотреть), параметр для отображения размера? зависит от размера итема отображения (или статик?)
            if (namer.Length > 1)
                return $"https://image-charts.com/chart?cht=bvg&chs=300x500&chd=t:{value}&chg=1,1,0,0&chan&chf=b0,lg,0,FFE7C6,0,76A4FB,1&chxt=x,y&chxr=1,{min.ToString()},{max.ToString()}&chxl=0:|{label}&chl={namer}&chof=.png";
            else
                return $"https://image-charts.com/chart?cht=bvg&chs=300x500&chd=t:{value}&chg=1,1,0,0&chan&chf=b0,lg,0,FFE7C6,0,76A4FB,1&chxt=x,y&chxr=1,{min.ToString()},{max.ToString()}&chxl=0:|{label}&chof=.png";
        }
        private static Dictionary<string, int> Counter_former(List<object> list)
        {
            Dictionary<string, int> valuePairs = new Dictionary<string, int>();
            int chet = 0;
            HashSet<object> unique_items = new HashSet<object>();
            if (Options.opt.ActiveType == 6)
            {
                foreach (string item in list)
                {
                    var vs = item.Split(',').ToList();
                    foreach (string val in vs)
                    {
                        if (valuePairs.ContainsKey(val))
                            valuePairs[val]++;
                        else
                            valuePairs.Add(val, 1);
                    }
                }
            }
            else
            {
                unique_items = new HashSet<object>(list);
                foreach (var un in unique_items)
                {
                    bool ll = un == list[0];
                    var loll = list.Where(t => t.ToString() == un.ToString()).ToList();
                    chet = loll.Count;
                    if (un.ToString() == "")
                        valuePairs.Add("Другие", chet);
                    else
                        valuePairs.Add(un.ToString(), chet);
                }
            }
            return valuePairs;
        }
    }
}
