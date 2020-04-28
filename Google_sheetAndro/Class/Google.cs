﻿using Android.Widget;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google_sheetAndro;
using Google_sheetAndro.Class;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
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
        public static Range_border RB = new Range_border(0, 0, 0, 0);
        public static void ShablonDuplicater(int shID, string title, int last_index_pg)
        {
            //CopySheetToAnotherSpreadsheetRequest requestBody = new CopySheetToAnotherSpreadsheetRequest
            //{
            //    DestinationSpreadsheetId = SpreadsheetId
            //};
            //SpreadsheetsResource.SheetsResource.CopyToRequest request = service.Spreadsheets.Sheets.CopyTo(requestBody, SpreadsheetId, shID);
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(15000);
            //var response = request.ExecuteAsync(cts.Token);

            BatchUpdateSpreadsheetRequest requestBodyBU = new BatchUpdateSpreadsheetRequest();
            IList<Request> LQReq = new List<Request>();
            LQReq.Add(google_requests.DuplicateSh(shID, title, last_index_pg + 1));
            //LQReq.Add(google_requests.RenamerSh(response.Result.SheetId, title));
            requestBodyBU.Requests = LQReq;
            BatchUpdateRequest BUrequest = service.Spreadsheets.BatchUpdate(requestBodyBU, SpreadsheetId);
            var resp2 = BUrequest.ExecuteAsync(cts.Token);
            var t = resp2.Result;
            newSheetId = t.Replies[0].DuplicateSheet.Properties.SheetId.Value;
        }
        public async static Task<bool> InitService(string year = "")
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(15000);
            var assembly = Assembly.GetExecutingAssembly();
            GoogleCredential credential;
            using (var stream = assembly.GetManifestResourceStream("Google_sheetAndro.sicret_new.json"))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(Googles.Scopes);
            }

            // Create Google Sheets API service.
            Googles.service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Googles.ApplicationName,
            });
            var qq = Googles.service.Spreadsheets.Get(Googles.SpreadsheetId).ExecuteAsync(cts.Token);
            Googles.sheetInfo = qq.Result;//await qq;//.Result;
            ShReader(year);
            return true;
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
        public static ValueRange GetBaseData()
        {
            var range = $"AirportBase!A:C";
            SpreadsheetsResource.ValuesResource.GetRequest request =
        service.Spreadsheets.Values.Get(SpreadsheetId, range);
            request.ValueRenderOption = ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(15000);
            return request.ExecuteAsync(cts.Token).Result;
        }
        public static void ShReader(string year_to = "")
        {
            var sheets = sheetInfo.Sheets;
            int shid = 0;
            string sh_name;
            int year = 0;
            int row_indx = -1;
            if (year_to != "")
            {
                sheets = sheets.Where(t => t.Properties.Title == year_to).ToList();
                LocalTable.ListItems.RemoveAll(t => t.year.ToString() == year_to);
                LocalTable.SheetsVal.Remove(year_to.ToString());
            }
            foreach (var item in sheets)
            {
                row_indx = -1;
                if (int.TryParse(item.Properties.Title, out year))
                {
                    sh_name = item.Properties.Title;
                    shid = (int)item.Properties.SheetId;
                    //var range = $"{year}!A:K";
                    var range = $"{year}!A:N";
                    SpreadsheetsResource.ValuesResource.GetRequest request =
        service.Spreadsheets.Values.Get(SpreadsheetId, range);
                    request.ValueRenderOption = ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.CancelAfter(15000);
                    var response = request.ExecuteAsync(cts.Token);
                    IList<IList<object>> values = response.Result.Values;
                    if (values != null && values.Count > 0)
                    {
                        LocalTable.SheetsVal.Add(year.ToString(), values);
                        foreach (var row in values)
                        {
                            row_indx++;
                            if (row[0].ToString() != "Мес" && row.Count > 1)
                            {
                                string r = "";
                                TableItem ti = new TableItem();
                                DateTime dtt = new DateTime(1899, 12, 30, new GregorianCalendar());
                                int daybuf = Convert.ToInt32(row[1].ToString());
                                dtt = dtt.AddDays(daybuf);
                                //string dt = dtt.ToString("D", CultureInfo.GetCultureInfo("ru-RU"));
                                ti.date = dtt;
                                double val = Convert.ToDouble(row[2], CultureInfo.InvariantCulture);
                                ti.time = new Time_r(val * 24 * 60 * 60).ToString();
                                if (row[3].ToString() == "")
                                {
                                    r = "0";
                                    ti.wind = Convert.ToDouble(r, CultureInfo.InvariantCulture);
                                }
                                else
                                    ti.wind = Convert.ToDouble(row[3], CultureInfo.InvariantCulture);
                                if (row[4].ToString() == "")
                                {
                                    r = "нет";
                                    ti.cloud = r;
                                }
                                else
                                    ti.cloud = row[4].ToString();

                                if (row[5].ToString() == "")
                                {
                                    r = "0";
                                    ti.temp = Convert.ToDouble(r, CultureInfo.InvariantCulture);
                                }
                                else
                                    ti.temp = Convert.ToDouble(row[5], CultureInfo.InvariantCulture);
                                ti.task = row[6].ToString();
                                if (row[7].ToString() == "")
                                {
                                    r = "0";
                                    ti.height = Convert.ToDouble(r, CultureInfo.InvariantCulture);
                                }
                                else
                                    ti.height = Convert.ToDouble(row[7], CultureInfo.InvariantCulture);

                                if (row[8].ToString() == "")
                                {
                                    r = "0";
                                    ti.range = Convert.ToDouble(r, CultureInfo.InvariantCulture);
                                }
                                else
                                    ti.range = Convert.ToDouble(row[8], CultureInfo.InvariantCulture);

                                ti.plase = row[9].ToString();
                                if (row.Count > 10)
                                    ti.comment = row[10].ToString();
                                else
                                    ti.comment = "";

                                if (row.Count > 11)
                                {
                                    if (row[11].ToString() == "")
                                        ti.author = "Oparakhin@gmail.com";
                                    else
                                        ti.author = row[11].ToString();
                                }
                                else
                                    ti.author = "Oparakhin@gmail.com";

                                if (row.Count > 12)
                                    ti.points = row[12].ToString();
                                else
                                    ti.points = "";

                                if (row.Count > 13)
                                    ti.route = row[13].ToString();
                                else
                                    ti.route = "";

                                ti.row_nb = row_indx + 1;
                                //ti.tabelplase = $"{year}!A{row_indx + 1}:K{row_indx + 1}";
                                ti.tabelplase = $"{year}!A{row_indx + 1}:N{row_indx + 1}";
                                if (row[0].ToString() != "")
                                    ti.exect_mounth = row[0].ToString();
                                else
                                    ti.exect_mounth = "";
                                ti.sh_id = shid;
                                LocalTable.ListItems.Add(ti);
                            }
                        }

                    }
                }
            }
            StaticInfo.EndLoadForListItems();
        }
        private static bool fl_yearadd;
        public static void SheetExist(int year)
        {
            //узнаем есть ли интересующий шит
            //sheetInfo = service.Spreadsheets.Get(SpreadsheetId).Execute();
            var sheets = sheetInfo.Sheets; // массив листов. тут можно проверить.
            string sh_name = "";
            int shID = 0;
            int indx_last = int.MinValue;
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
                if (item.Properties.Index > indx_last) indx_last = item.Properties.Index.Value;
            }
            sheet_id = shID;
            if (sh_name != "")
            {
                SheetName = sh_name;
            }
            else
            {
                ShablonDuplicater(Sh_shbalon, year.ToString(), indx_last);//createnewsheets и вернем его имя
                sheet_id = Googles.newSheetId;
                SheetName = year.ToString();
                InitService(SheetName);
                fl_yearadd = true;

            }
        }
        public static async Task<bool> ReadEntriesAsync(TableItem ti/*Dictionary<string, object> dic*/)
        {
            var network = Connectivity.NetworkAccess;
            try
            {
                if (network == NetworkAccess.None)
                {
                    throw new Exception("No connect");
                }
                int month = ti.date.Month;
                string[] months = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
                string mont_nm = months[month - 1].ToLower();
                int year = ti.year;
                int sh_ID = 0;
                SheetExist(year);
                sh_ID = sheet_id;//ti.sh_id;//Googles.sheet_id;
                sheet = SheetName;
                //var range = $"{sheet}!A:K";
                var range = $"{sheet}!A:N";
                int inp_row = 0;
                int inp_row_mount = 0;
                var values = LocalTable.SheetsVal[year.ToString()];
                foreach (var row in values)
                {
                    if (row[0].ToString() == mont_nm.ToLower())
                    {

                        inp_row = inp_row_mount = values.IndexOf(row) + 1;
                        ti.row_nb = inp_row;
                        break;
                    }
                }
                //нашли строку, смотрим пустая она
                //если да пишем в нее

                if (ti.exect_mounth != "")//!string.IsNullOrEmpty(ti.exect_mounth))//ti.exect_mounth != "")
                {
                    IList<Request> LQReq = new List<Request>();
                    if (values[inp_row - 1].Count > 1)
                    {
                        if (values.Count < inp_row + 1)//проверка на последнюю строку. Если после нее ничего нет то... (декабрь ласт)
                        {
                            InsertRowAsync(ti, inp_row + 1);
                            RB.fst_clm = 0;
                            RB.sec_clm = 1;
                            RB.fst_rw = inp_row - 1;
                            RB.sec_rw = inp_row + 1;
                            LQReq.Add(Merger(sh_ID));
                        }
                        else if (values[inp_row][0].ToString().Length > 1) //если в следующей строке в месяце что-то написано
                        {
                            InsertRowAsync(ti, inp_row + 1);
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
                                if (values.Count < inp_row + 1)
                                {
                                    break;
                                }
                            }
                            InsertRowAsync(ti, inp_row + 1);
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
                        CancellationTokenSource cts = new CancellationTokenSource();
                        cts.CancelAfter(15000);
                        var t = BUrequest.ExecuteAsync(cts.Token).Result;
                    }
                    else
                    {
                        //пустая, использовать существующий номер
                        UpdateEntry(ti);
                    }
                }
                else
                {
                    Debug.WriteLine("No data found.");
                    return false;
                }
                //sheetInfo = service.Spreadsheets.Get(SpreadsheetId).Execute();
                InitService(year.ToString());
                if (fl_yearadd)
                {
                    StaticInfo.EndLoadForListItemsYear();
                    fl_yearadd = false;
                }
                StaticInfo.EndSuccSend();
                Toast.MakeText(Android.App.Application.Context, "Запись прошла успешно", ToastLength.Long).Show();
                return true;
            }
            catch (Exception ex)
            {
                string buff = ex.Message;
                string outas = "Запись неудачна";
                if (network == NetworkAccess.None)
                {
                    string kk = Preferences.Get("Offline_data", "");
                    var ti_list = JsonConvert.DeserializeObject<List<TableItem>>(kk);
                    if (ti_list.Count > 0)
                    {
                        if (!ti_list.Contains(ti))
                        {
                            ti_list.Add(ti);
                        }
                    }
                    else
                        ti_list.Add(ti);
                    string seria = JsonConvert.SerializeObject(ti_list);
                    Preferences.Set("Offline_data", seria);
                    outas += "\nЗапись сохранена для офлайн";
                }
                Toast.MakeText(Android.App.Application.Context, outas, ToastLength.Long).Show();
                return false;
            }
        }

        /// <summary>
        /// Вставка строки со значениями
        /// </summary>
        /// <param name="sheet_ID"></param>
        /// <param name="Row_after"></param>
        static bool InsertRowAsync(TableItem ti, int Row_after/*Dictionary<string, object> dic, int Row_after*/)
        {
            BatchUpdateSpreadsheetRequest requestBody = new BatchUpdateSpreadsheetRequest();
            requestBody.Requests = new List<Request>() { google_requests.InsRow(Googles.sheet_id, Row_after - 1) };
            BatchUpdateRequest BUrequest = service.Spreadsheets.BatchUpdate(requestBody, SpreadsheetId);
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(15000);
            var resp = BUrequest.ExecuteAsync(cts.Token).Result;
            ti.row_nb = Row_after;
            UpdateEntry(ti);
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
        public static void UpdateEntry(TableItem tbi/*int num_inp, Dictionary<string, object> dic*/)
        {
            var sheet = tbi.year.ToString();
            //var range = $"{sheet}!B{tbi.row_nb}:K{tbi.row_nb}";
            var range = $"{sheet}!B{tbi.row_nb}:N{tbi.row_nb}";
            var valueRange = new ValueRange();
            //var oblist = new List<object>();
            //foreach (string item in dic.Keys)
            //{
            //    oblist.Add(dic[item]);
            //}
            var oblist = tbi.GetListForEntry();
            valueRange.Values = new List<IList<object>> { oblist };

            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(15000);
            var appendReponse = updateRequest.ExecuteAsync(cts.Token).Result;
        }
        public static void DeleteEntry(TableItem tbl)
        {
            var range = tbl.tabelplase;
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(15000);
            var requestBody = new ClearValuesRequest();
            range = range.Replace("A", "B");
            if (tbl.exect_mounth == "")//string.IsNullOrEmpty(tbl.exect_mounth))//)
            {
                BatchUpdateSpreadsheetRequest rqBody = new BatchUpdateSpreadsheetRequest();
                rqBody.Requests = new List<Request>() { google_requests.DeleteRow(tbl) };
                BatchUpdateRequest BUrequest = service.Spreadsheets.BatchUpdate(rqBody, SpreadsheetId);
                var t = BUrequest.ExecuteAsync(cts.Token).Result;
            }
            else
            {
                var deleteRequest = service.Spreadsheets.Values.Clear(requestBody, SpreadsheetId, range);
                var deleteReponse = deleteRequest.ExecuteAsync(cts.Token).Result;
            }
            InitService(tbl.year.ToString());
            //LocalTable.ListItems.Remove(tbl);
        }
    }
}
