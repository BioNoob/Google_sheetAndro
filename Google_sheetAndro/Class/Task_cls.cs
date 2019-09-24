using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.MultiSelectListView;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    public class Task_cls
    {
        public string Num { get; set; }
        public string Opicanie { get; set; }
        public string Outter
        {
            get
            {
                return Num + "\t" + Opicanie;
            }
        }
        public Task_cls(string numer,string chto)
        {
            Num = numer;
            Opicanie = chto;
        }
        public override string ToString()
        {
            return Outter;
        }
    }
}