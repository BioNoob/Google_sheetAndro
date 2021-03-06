﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.MultiSelectListView;
using Xamarin.Forms.Xaml;

namespace Google_sheetAndro.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TaskSelectPage : ContentPage
    {
        public MultiSelectObservableCollection<Task_cls> Taskk = new MultiSelectObservableCollection<Task_cls>();
        List<string> Tasked = new List<string>
            {
                "1а\tОтработка руления",
                "01\tОзнакомительный полет",
                "02\tВывозной полет в зону(по большому кругу) для обучения элементам техники пилотирования со второго сидения",
                "03\tВывозной полет по кругу для обучения взлету, построению маршрута, заходу, расчету и посадке со второго сиденья",
                "04\tКонтрольный полет для проверки освоения элементов техники пилотирования, взлета и посадки со второго сиденья",
                "05\tВывозной полет в зону для обучения элементам техники пилотирования с первого сидения",
                "06\tВывозной полет по кругу для обучения взлету, построению маршрута, заходу, расчету и посадке с первого сиденья.",
                "07\tВывозной полет по кругу для обучения исправлению отклонений в расчете и на посадке, уходу на второй круг.",
                "08\tВывозной полет по кругу для обучения взлету, построению маршрута и посадке с боковым ветром.",
                "09\tКонтрольно - вывозной полет для обучения действиям в особых случаях: при отказе двигателя, оборудования, управления, закрытии и смене старта.",
                "10\tКонтрольно - вывозной полет в зону для ознакомления с пилотированием на минимально и максимально допустимых скоростях\n" +
                ", кренах, углах набора и снижения, и обучения действиям по выводу СЛА в запланированный режим полета.",
                "11\tШлифовочные, контрольные и зачетные полеты по кругу.",
                "12\tПервый самостоятельный вылет",
                "13\tТренировочный (самостоятельный).полет по кругу",
                "14\tЗачетные и соревновательные полеты по кругу на точность приземления с работающим двигателем.(Для спортсменов - на размеченной стандартной \"палубе\").",


//2 ступень ОСВОЕНИЕ ПОЛНЫХ ЭКСПЛУАТАЦИОННЫХ ВОЗМОЖНОСТЕЙ


"15\tКонтрольно-вывозной полет по схеме для отработки расчета и посадки на точность приземления с отключенным двигателем.",
"16\tТренировочный (зачетный, соревнова- тельный ) полет по схеме для отработки расчета и посадки с выключенным двигате­лем. (Для спортсменов - на размеченной стандартной \"палубе\").",
"17\tКонтрольно-вывозной (контрольный, зачетный ) полет для отработки маневри- рования в зоне и действий при отказе двигателя.",
"18\tТренировочный (зачетный) полет в зону для отработки маневрирования с кренами 15, 30, углами набора и снижения 10,15",
"19\tКонтрольно-вывозной (зачетный) полет в зону для отработки маневрирования с кренами до 45 , углами набора и снижения до 20 , фигур простого пилотажа раздельно и в комплексе.",
"20\tТренировочный полет в зону для отработки фигур простого пилотажа.",
"21\tТренировочный (зачетный) полет в зону для отработки фигур простого пилотажа в комплексе.",
"20\tКонтрольно-показной полет в зону для отработки техники пилотирования на предельно-малой высоте.",
"23\tТренировочный полет в зону для отработки техники пилотирования на предельно-малой высоте.",
"24\tТренировочный полет в зону (по схеме) на большую высоту (на потолок)",
"25\tКонтрольно-вывозной полет в зону для отработки маневрирования на максимально-допустимых углах крена, тангажа, минимально- и максимально-допустимых скоростях",
"26\tТренировочный полет в зону для отработки маневрирования на максимально допустимых углах крена, тангажа, минимально- и максимально-дпустимых скоростях",
"27\tКонтрольный, тренировочный полет для от работки взлета и посадки с площадок вне аэродрома.",


//2 ступень ОСВОЕНИЕ ПОЛНЫХ ЭКСПЛУАТАЦИОННЫХ ВОЗМОЖНОСТЕЙ


"28\tКонтрольный, тренировочный полет для отработки взлета и посадки на площадке ограниченных размеров.",
"29\tКонтрольно-вывозной полет для отработки захода, расчета, посадки и взлета на площадке подобранной с воздуха.",
"30\tТренировочный полет для отработки захода, расчета, посадки и взлета на площадке подобранной с воздуха.",

//ЗАДАЧА 5 ОБУЧЕНИЕ ПРАКТИЧЕСКОЙ ВОЗДУШНОЙ НАВИГАЦИИ И ОТРАБОТКА ПОЛЕТОВ ПО МАРШРУТУ
//1 ступень ОСНОВНОЙ КУРС
"31\tТренировочный полет в районе аэродрома для отработки элементов навигации.",
"32\tОблет района полетов.",
"33\tКонтрольно-вывозной полет по маршруту для отработки техники навигации и способов восстановления ориентировки.",
"34\tТренировочный, контрольный, зачетный полет по маршруту для отработки визуальной ориентировки, контроля пути по дальности и направлению.",
"35\tТренировочный, контрольный полет по маршруту для отработки выдерживания и исправления линии пути, техники прохода ИПМ, ППМ.",
"36\tТренировочный, зачетный, соревновательный полет по маршруту, рассчитанному по известному ветру. Для спортсменов - по нормам ЕВСК ( D= 50 км)",
"37\tКонтрольно-вывозной (контрольный, зачетный) полет по маршруту для отработки выхода на ППМ (КПМ)\n в заданное время и промежуточных посадок на подготовленных, а также подобранных с воздуха площадках.",
"38\tТренировочный(контрольный, зачетный) полет по маршруту в выходом на ППМ (КПМ) в заданное время.",
"39\tТренировочный(контрольный, зачетный) полет по маршруту с промежуточными посадками на подготовленных площадках (аэродромах).",


//2 ступень ОСВОЕНИЕ ПОЛНЫХ ЭКСПЛУАТАЦИОННЫХ ВОЗМОЖНОСТЕЙ


"40\tТренировочный, контрольный, зачетный полет по маршрут с промежуточными посадками на площадках, подобранных с воздуха.",
"41\tТренировочный полет по маршруту на набольшую (максимальную) дальность. Для спортсменов- согласно нормам ЕВСК : 100 км; 150 км; 200 км.",
"42\tКонтрольный, тренировочный полет по маршруту на предельно малой высоте",
//ДЛЯ СПОРТСМЕНОВ
"43\tТренировочный полет по маршруту сложной конфигурации и переменного профиля",
"44\tТренировочный, зачетный, соревновательный полет по маршруту для отработки спортивных заданий на скорость, экономичность и поиск целей",
"45\tКонтрольно - вывозной полет на отработку элементов каталога спортивных заданий 1 - й категории ФАИ.",
"46\tТренировочных полет на отработку элементов каталога заданий соревнований национальной и 1 - й ФАЙ категории.",
"47\tТренировочное, зачетное, соревновательное комплексное упражнение на точность планирования и навигации\n с оценкой времени и(или ) скорости, а также приземления с отключенным двигателем.",
"48\tТренировочное, зачетное, соревновательноекомплексное упражнение на дальность, скорость.",
"49\tТренировочное, зачетное, соревновательное комплексное упражнение по программе многодневного перелета согласно квалификационным нормам ФАИ.",
"50\tТренировочное, зачетное упражнение по программе установления рекорда"
            };

        public TaskSelectPage()
        {
            InitializeComponent();
            foreach (string item in Tasked)
            {
                Taskk.Add(new Task_cls(item.Substring(0, 2), item.Substring(2).Replace("\t", "")));
            }
            lvTask.ItemsSource = Taskk;
        }
        private void clearer()
        {
            OutTask = string.Empty;
            foreach (var it in Taskk)
            {
                it.IsSelected = false;
            }
        }
        public void SetSelected(string inner)
        {
            clearer();
            Task_txt.TextChanged -= Task_txt_TextChanged;
            string buf = string.Empty;
            foreach (var item in inner.Split(','))
            {
                if (item.Length < 2)
                    buf = $"0{item}";
                else
                    buf = item;
                foreach (var it in Taskk)
                {
                    if (it.Data.Num == buf)
                    {
                        it.IsSelected = true;
                        LvTask_ItemSelected(this, new SelectedItemChangedEventArgs(it));
                    }
                }
            }
            Task_txt.TextChanged += Task_txt_TextChanged;
        }
        public string OutTask = string.Empty;
        public event EventHandler tasksetsucs;
        private async void Confirm_btn_Clicked(object sender, EventArgs e)
        {
            if (OutTask.Length > 0)
            {
                if (OutTask[OutTask.Length - 1] == ',')
                    OutTask = OutTask.Remove(OutTask.Length, 1);
            }
            tasksetsucs(this, EventArgs.Empty);
            await Navigation.PopModalAsync();
        }
        public ICommand DisplayNameCommand => new Command<Task_cls>(Taskk =>
       {
           //await 
           // if(!fl_zamena)
           // {
           if (OutTask != string.Empty)
           {
               OutTask += ",";
           }
           OutTask += Taskk.Num;
           OutNum.Text = OutTask;
           // }

           //Application.Current.MainPage.DisplayAlert("Selected Name", Task_cls.Name, "OK");
       });
        //bool fl_zamena = false;
        private void Task_txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            MultiSelectObservableCollection<Task_cls> Task_buf = new MultiSelectObservableCollection<Task_cls>();
            if (Task_txt.Text.Length > 0)
            {
                var ss = Taskk.Where(t => t.Data.Num.Contains(e.NewTextValue)).ToList();
                foreach (var item in ss)
                {
                    Task_buf.Add(new Task_cls(item.Data.Num, item.Data.Opicanie));
                }
                if (Task_buf.Count > 0)
                {
                    lvTask.ItemsSource = Task_buf;//"{Binding Task_buf}";
                };
            }
            else
            {
                foreach (var item in (MultiSelectObservableCollection<Task_cls>)lvTask.ItemsSource)
                {
                    if (item.IsSelected)
                        Taskk.IsSelected(item.Data);
                }
                lvTask.ItemsSource = Taskk;//"{Binding Taskk}";
            }
        }
        private void LvTask_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                if (((SelectableItem<Task_cls>)e.SelectedItem).IsSelected)
                {
                    if (OutTask != string.Empty)
                        OutTask += ",";
                    OutTask += ((SelectableItem<Task_cls>)e.SelectedItem).Data.Num;
                }
                else
                {
                    var lol = OutTask.Split(',');
                    lol = lol.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    lol = lol.Where(x => x != ((SelectableItem<Task_cls>)e.SelectedItem).Data.Num).ToArray();
                    lol = lol.Where(x => x != ",").ToArray();
                    OutTask = string.Join(",", lol);// OutTask.Replace("," + ((SelectableItem<Task_cls>)e.SelectedItem).Data.Num, "");
                }
                OutNum.Text = OutTask;
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Label s = (Label)sender;
            foreach (var item in Taskk)
            {
                if (item.Data.Outter == s.Text)
                {
                    if (item.IsSelected)
                        item.IsSelected = false;
                    else
                        item.IsSelected = true;
                    LvTask_ItemSelected(this, new SelectedItemChangedEventArgs(item));
                    return;
                }
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            clearer();
            Task_txt.Text = "";
            OutNum.Text = string.Empty;
            OutTask = string.Empty;
        }
    }
}