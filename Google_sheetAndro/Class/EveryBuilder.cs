using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Google_sheetAndro.Class
{
    public static class EveryBuilder
    {
        private static Dictionary<string, List<string>> _group = new Dictionary<string, List<string>>();
        private static int _max_val = 0; 
        public static Dictionary<string, List<string>> Getgroup()
        {
            return _group;
        }
        public static void clear()
        {
            _group.Clear();
        }
        public static void SetGroup(string name_group, List<object> values)
        {
            List<string> ls = new List<string>();
            var mylist = values.ConvertAll(x => x.ToString());
            ls.AddRange(mylist);
            if (_group.ContainsKey(name_group))
                _group[name_group].AddRange(ls);
            else
                _group.Add(name_group, ls);
            if (ls.Count > _max_val) _max_val = ls.Count;
        }
        public static string GetString()
        {
            string outs = "";
            List<string> outer = new List<string>();
            foreach (string item in _group.Keys)
            {
                if (_group[item].Count < _max_val)
                {
                    var lol = _max_val - _group[item].Count;
                    _group[item].AddRange(Enumerable.Repeat("_", lol));
                }
            }
            for (int i = 0; i < _max_val; i++)
            {
                foreach (string item in _group.Keys)
                {
                    outer.Add(_group[item][i]);
                }
                outs += string.Join(",", outer) + "|";
                outer.Clear();
            }
            return outs.Remove(outs.Length - 1, 1);
        }
    }
}
