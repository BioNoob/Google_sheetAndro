using Android.App;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;
using static Google.Apis.Sheets.v4.SpreadsheetsResource;

namespace TableAndro
{
    /*
     * 1.заполняем таблицу +
     * 2.читаем таблицу гугл +
     * 3.получаем год из даты +
     * 4.чекаем есть ли такой лист +
     * 5.если нет создаем (шаблон с одной строкой на месяц) (может быть берем просто шаблон копируем, ибо форматы данных) + 
     * 6.по месяцу из даты (мб =MONTH()) идем в нужную соседнюю справа ячейку
     * (или следующий, потому что первая строка) и на одну вверх
     * 7.вставляем строку с нашими данными
     * 
     * Можем работать как с десириалезатором ответа, так и просто по приколу
     */
    public static class Googles
    {


        static string sheet { get; set; }
        public static Spreadsheet sheetInfo;
        public static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        public static readonly string ApplicationName = "Flight Base";
        //public static readonly string SpreadsheetId = "1Hak8SE7-5SXClivHBB1dgEd3secFCS40n0JBwT34QOU";
        public static readonly string SpreadsheetId = "1O8AeG_B45kAyG3cuClJigZhfKUtjV-0eQRZq8XzUutk";
        public static SheetsService service;
        public static int newSheetId = 0;
        public static Range_border RB = new Range_border(0,0,0,0);
        public static void ShablonDuplicater(int shID,string title)
        {
            CopySheetToAnotherSpreadsheetRequest requestBody = new CopySheetToAnotherSpreadsheetRequest
            {
                DestinationSpreadsheetId = SpreadsheetId
            };
            SpreadsheetsResource.SheetsResource.CopyToRequest request = service.Spreadsheets.Sheets.CopyTo(requestBody, SpreadsheetId, shID);
            SheetProperties response = request.Execute();
            
            BatchUpdateSpreadsheetRequest requestBodyBU = new BatchUpdateSpreadsheetRequest();
            IList<Request> LQReq = new List<Request>();
            LQReq.Add(google_requests.RenamerSh(response.SheetId, title));
            requestBodyBU.Requests = LQReq;
            BatchUpdateRequest BUrequest = service.Spreadsheets.BatchUpdate(requestBodyBU, SpreadsheetId);
            var resp2 = BUrequest.Execute();
            newSheetId = response.SheetId ?? 0;
        }
        public static Dictionary<string, ValueRange> GetValueTabel()
        {
            Dictionary<string, ValueRange> dic = new Dictionary<string, ValueRange>();
            SpreadsheetsResource.ValuesResource.GetRequest request;
            string range = string.Empty;
            var sheets = sheetInfo.Sheets;
            foreach (Sheet item in sheets)
            {
                range = string.Empty;
                string sh_name = item.Properties.Title;
                //if (sh_name == "Общий налет")
                //    range = $"{sh_name}!A:G";
                if (IsDigitsOnly(item.Properties.Title))
                    range = $"{sh_name}!A:K";
                if (range != string.Empty)
                {
                    request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
                    request.ValueRenderOption = ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                    //request.DateTimeRenderOption = ValuesResource.GetRequest.DateTimeRenderOptionEnum.FORMATTEDSTRING;
                    var response = request.Execute();
                    dic.Add(sh_name, response);
                }
            }
            return dic;
        }
        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
        public static string SheetName = string.Empty;
        public static int sheet_id = 0;
        public static void SheetExist(int year)
        {
            //узнаем есть ли интересующий шит
            //sheetInfo = service.Spreadsheets.Get(SpreadsheetId).Execute();
            var sheets = sheetInfo.Sheets; // массив листов. тут можно проверить.
            string sh_name = "";
            int shID = 0;
            int Sh_shbalon = 0;
            foreach (var item in sheets)
            {
                if (item.Properties.Title == year.ToString())
                {
                    sh_name = item.Properties.Title;
                    shID = (int)item.Properties.SheetId;
                }
                if (item.Properties.Title == "Шаблон")
                {
                    Sh_shbalon = (int)item.Properties.SheetId;
                }
            }
            sheet_id = shID;
            if (sh_name != "")
            {
                SheetName = sh_name;
            }
            else
            {
                ShablonDuplicater(Sh_shbalon, year.ToString());//createnewsheets и вернем его имя
                sheet_id = Googles.newSheetId;
                SheetName =  year.ToString();
            }
        }
        public static bool ReadEntriesAsync(Dictionary<string, object> dic)
        {
            try
            {
                int year = ((DateTime)dic["date"]).Year;
                int month = ((DateTime)dic["date"]).Month;
                dic["date"] = ((DateTime)dic["date"]).ToString("dd/MM/yyyy");
                //string mont_nm = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(((DateTime)dic["date"]).Month);
                //string mont_nm = ((DateTime)dic["date"]).Month.ToString("MMMM", new CultureInfo("ru-RU"));

                string[] months = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
                string mont_nm = months[month - 1].ToLower();

                int sh_ID = 0;
                SheetExist(year);
                sh_ID = Googles.sheet_id;
                sheet = SheetName;
                var range = $"{sheet}!A:K";
                //находить пустые строки при месяце
                //range A:A 
                //IF row[0] == mont_nm то вставить строчку выше(если строчка выше пустая if предыдущий row[1] <> "", то получить ее номер, если нет новую)
                //заность данные туда. Получить координаты этой строчки (A + row.number, K + row.number)
                //ну и собстно поехали         
                int inp_row = 0;
                int inp_row_mount = 0;
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(SpreadsheetId, range);
                var lol = service.Spreadsheets.Get(SpreadsheetId).Execute();
                var response = request.Execute();
                IList<IList<object>> values = response.Values;
                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        if (row[0].ToString() == mont_nm.ToLower())
                        {

                            inp_row = inp_row_mount = values.IndexOf(row) + 1;
                            //Получаем индекс предыдущей строки (или нужен настоящей?)
                            //проверка на пустую строку должна быть, если строка не пустая вставить строку а не апдейтить
                            //тобишь если строка row.count > 1
                            break;
                        }
                    }
                    //нашли строку, смотрим пустая она
                    //если да пишем в нее


                    IList<Request> LQReq = new List<Request>();
                    if (values[inp_row - 1].Count > 1)
                    {
                        if (values[inp_row][0].ToString().Length > 1) //если в следующей строке в месяце что-то написано
                        {
                            InsertRowAsync(dic, inp_row + 1);
                            Debug.WriteLine("RETURN HOME");
                            RB.fst_clm = 0;
                            RB.sec_clm = 1;
                            RB.fst_rw = inp_row - 1;
                            RB.sec_rw = inp_row + 1;
                            LQReq.Add(Merger(sh_ID));
                            Debug.WriteLine("MERGER");
                        }
                        else
                        {
                            while (values[inp_row][0].ToString().Length == 0)
                            {
                                inp_row++;
                            }
                            InsertRowAsync(dic, inp_row + 1);
                            Debug.WriteLine("RETURN HOME");
                            RB.fst_clm = 0;
                            RB.sec_clm = 1;
                            RB.fst_rw = inp_row_mount - 1;
                            RB.sec_rw = inp_row + 1;
                            LQReq.Add(Merger(sh_ID));
                            Debug.WriteLine("MERGER");
                        }
                        //foreach (Request item in google_requests.FormateRq(sh_ID, inp_row))
                        // {
                        //LQReq.Add(item);
                        //}
                        BatchUpdateSpreadsheetRequest requestBody = new BatchUpdateSpreadsheetRequest();
                        requestBody.Requests = LQReq;
                        BatchUpdateRequest BUrequest = service.Spreadsheets.BatchUpdate(requestBody, SpreadsheetId);
                        BUrequest.Execute();
                    }
                    else
                    {
                        //пустая, использовать существующий номер
                        UpdateEntry(inp_row, dic);
                    }
                }
                else
                {
                    Debug.WriteLine("No data found.");
                    return false;
                }
                //sheetInfo = service.Spreadsheets.Get(SpreadsheetId).Execute();
                sheetInfo = Googles.service.Spreadsheets.Get(Googles.SpreadsheetId).Execute();
                Toast.MakeText(Android.App.Application.Context, "Запись прошла успешна", ToastLength.Long).Show();
                return true;
            }
            catch (Exception)
            {
                Toast.MakeText(Android.App.Application.Context, "Запись неудачна", ToastLength.Long).Show();
                return false;
            }
        }

        /// <summary>
        /// Вставка строки со значениями
        /// </summary>
        /// <param name="sheet_ID"></param>
        /// <param name="Row_after"></param>
        static bool InsertRowAsync(Dictionary<string, object> dic, int Row_after)
        {
            var range = $"{sheet}!B{Row_after}:K{Row_after}";
            var valueRange = new ValueRange();
            IList<Object> obj = new List<Object>();

            foreach (string item in dic.Keys)
            {
                obj.Add(dic[item]);
            }

            IList<IList<Object>> values = new List<IList<Object>>();
            values.Add(obj);
            SpreadsheetsResource.ValuesResource.AppendRequest request =
                    service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, SpreadsheetId, range);
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            //var response = 
            Debug.WriteLine("Entry RowIns");
            var resp = request.Execute();
            Debug.WriteLine("EXIT RowIns");
            return true;
        }
        static Request Merger(int sh_id)
        {
            return google_requests.MergeRq(sh_id, RB);
        }
        /// <summary>
        /// обновление строки со значениями
        /// </summary>
        /// <param name="num_inp"></param>
        /// <param name="dic"></param>
        static void UpdateEntry(int num_inp, Dictionary<string, object> dic)
        {
            var range = $"{sheet}!B{num_inp}:K{num_inp}";
            var valueRange = new ValueRange();
            var oblist = new List<object>();
            foreach (string item in dic.Keys)
            {
                oblist.Add(dic[item]);
            }

            valueRange.Values = new List<IList<object>> { oblist };

            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = updateRequest.Execute();
        }
        static void DeleteEntry()
        {
            var range = $"{sheet}!A543:F";
            var requestBody = new ClearValuesRequest();

            var deleteRequest = service.Spreadsheets.Values.Clear(requestBody, SpreadsheetId, range);
            var deleteReponse = deleteRequest.Execute();
        }
    }
}
