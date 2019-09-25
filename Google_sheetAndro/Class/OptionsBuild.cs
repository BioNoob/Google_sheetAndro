using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using Google_sheetAndro.Class;

namespace Google_sheetAndro
{

    public struct OptionsBuild
    {

        public enum SortingEnum
        {
            /// <summary>
            /// Каждый год среднее за месяц
            /// </summary>
            AllYearMidEvery = 0,
            /// <summary>
            /// Каждый год сумма за год (сумма) в месяце
            /// </summary>
            AllYearMid = 1,
            /// <summary>
            /// Максимальная за все года в месяце
            /// </summary>
            AllYearMax = 2,
            /// <summary>
            /// Минимальная за все года в месяце
            /// </summary>
            AllYearMin = 3,
            AllYearCount = 4,
            /// <summary>
            /// Среднее за каждый месяц в году
            /// </summary>
            YearMid = 5,
            /// <summary>
            /// Все за месяц в году
            /// </summary>
            YearEvery = 6,
            /// <summary>
            /// максимальное за месяц в году
            /// </summary>
            YearMax = 7,
            /// <summary>
            /// минимальное за месяц в году
            /// </summary>
            YearMin = 8,
            YearCount = 9
        }
        public enum EnumTyp
        {
            Time = 2,
            Vind = 3,
            //Cloud = 4, // NOT
            Temperature = 5,
            Task = 6, //SPECIAL ALARM ЯриК СДЕЛАЙ ЭТО!!!!!Ё!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ПРОверить так ли будет собираться массив для задач
            Height = 7,
            Range = 8,
            Primech = 10
            //Place = 9 // NOT
        }
        private int _activeSort;
        private int _activeType;
        private int _drawingSize_W;
        private int _drawingSize_H;
        //private EnumTyp _activeType_enum;
        public OptionsBuild(EnumTyp typ, SortingEnum sorting, int drawingSize_W, int drawingSize_H, int mounth)
        {
            _activeType = (int)typ;
            _activeSort = (int)sorting;
            _drawingSize_H = drawingSize_H;
            _drawingSize_W = drawingSize_W;
            ActiveMounth = mounth;
        }
        public static OptionsBuild Default
        {
            get
            {
                return new OptionsBuild(EnumTyp.Time, SortingEnum.AllYearMidEvery, 300, 500, 0);
            }
        }
        public int ActiveSort { get => _activeSort; set => _activeSort = value; }
        public SortingEnum SortNum { get => (SortingEnum)_activeSort; set => _activeSort = (int)value; }
        public int Width { get => _drawingSize_W; set => _drawingSize_W = value; }
        public int Height { get => _drawingSize_H; set => _drawingSize_H = value; }
        public int ActiveMounth { get; set; }
        public int ActiveType { get => _activeType; set { _activeType = value; } }//_activeType_enum = (EnumTyp)value;  } }
        //public EnumTyp ActiveType_e { get => _activeType_enum; set { _activeType_enum = value; _activeType = (int)value; } }
    }
}
