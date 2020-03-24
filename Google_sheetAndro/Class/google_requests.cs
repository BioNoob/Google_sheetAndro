using Google.Apis.Sheets.v4.Data;
using Google_sheetAndro.Class;
using System.Collections.Generic;

namespace TableAndro
{
    public class Range_border
    {
        public int fst_rw { get; set; }
        public int sec_rw { get; set; }
        public int fst_clm { get; set; }
        public int sec_clm { get; set; }
        public Range_border(int sri, int eri, int sci, int eci)
        {
            fst_rw = sri;
            sec_rw = eri;
            sec_clm = eci;
            fst_clm = sci;
        }
    }
    public static class google_requests
    {
        public static Request MergeRq(int shid, Range_border RB)
        {
            Request RQ = new Request();
            RQ.MergeCells = new MergeCellsRequest();
            GridRange gr = new GridRange();
            gr.SheetId = shid;
            gr.StartRowIndex = RB.fst_rw;
            gr.EndRowIndex = RB.sec_rw;
            gr.StartColumnIndex = RB.fst_clm;
            gr.EndColumnIndex = RB.sec_clm;
            RQ.MergeCells.MergeType = "MERGE_ALL";
            RQ.MergeCells.Range = gr;
            return RQ;
        }
        /// <summary>
        /// Вставить пустую строку
        /// </summary>
        /// <param name="shid"></param>
        /// <param name="row_after">после строки (натуральная цифра, не индекс)</param>
        /// <returns></returns>
        public static Request InsRow(int shid, int row_after)
        {
            Request RQ = new Request();
            RQ.InsertDimension = new InsertDimensionRequest();
            RQ.InsertDimension.Range = new DimensionRange();
            RQ.InsertDimension.Range.SheetId = shid;
            RQ.InsertDimension.Range.StartIndex = row_after;
            RQ.InsertDimension.Range.EndIndex = row_after + 1;
            RQ.InsertDimension.Range.Dimension = "ROWS";
            RQ.InsertDimension.InheritFromBefore = true;
            return RQ;
        }
        public static Request DeleteRow(TableItem ti)
        {
            Request RQ = new Request();
            RQ.DeleteDimension = new DeleteDimensionRequest();
            RQ.DeleteDimension.Range = new DimensionRange();
            RQ.DeleteDimension.Range.SheetId = ti.sh_id;
            RQ.DeleteDimension.Range.StartIndex = ti.row_nb - 1;
            RQ.DeleteDimension.Range.EndIndex = ti.row_nb;
            RQ.DeleteDimension.Range.Dimension = "ROWS";
            return RQ;
        }
        public static Request RenamerSh(int? shid, string new_name)
        {
            Request RQ = new Request();
            RQ.UpdateSheetProperties = new UpdateSheetPropertiesRequest();
            RQ.UpdateSheetProperties.Properties = new SheetProperties
            {
                Title = new_name,
                SheetId = shid
            };
            RQ.UpdateSheetProperties.Fields = "title";
            return RQ;
        }
        public static Request DuplicateSh(int? shid, string new_name, int last_indx)
        {
            Request RQ = new Request();
            RQ.DuplicateSheet = new DuplicateSheetRequest
            {
                SourceSheetId = shid,
                NewSheetName = new_name,
                InsertSheetIndex = last_indx
            };
            var t = RQ.DuplicateSheet.NewSheetId;
            return RQ;
        }
        public static List<Request> FormateRq(int shid, int row)
        {
            List<Request> BUF = new List<Request>();
            //for (int i = 1; i < 11; i++)
            int i = 1;
            {
                Request RQ = new Request();
                RQ.RepeatCell = new RepeatCellRequest();
                RQ.RepeatCell.Cell = new CellData
                {
                    UserEnteredFormat = new CellFormat
                    {
                        NumberFormat = new NumberFormat()
                    }
                };
                switch (i)
                {
                    case 1:
                        RQ.RepeatCell.Cell.UserEnteredFormat.NumberFormat.Type = "DATE_TIME";
                        RQ.RepeatCell.Cell.UserEnteredFormat.NumberFormat.Pattern = "dd mmmm yyyy г.";
                        break;
                    case 2:
                        RQ.RepeatCell.Cell.UserEnteredFormat.NumberFormat.Type = "TIME";
                        RQ.RepeatCell.Cell.UserEnteredFormat.NumberFormat.Pattern = "[h]:mm:ss";
                        break;
                    case 3:
                    case 7:
                    case 8:
                        RQ.RepeatCell.Cell.UserEnteredFormat.NumberFormat.Type = "NUMBER";
                        RQ.RepeatCell.Cell.UserEnteredFormat.NumberFormat.Pattern = "###0";
                        break;
                    default:
                        RQ.RepeatCell.Cell.UserEnteredFormat.NumberFormat.Type = "TEXT";
                        break;
                }
                //RQ.RepeatCell.Cell.UserEnteredFormat.HorizontalAlignment = "CENTER";
                //RQ.RepeatCell.Cell.UserEnteredFormat.VerticalAlignment = "CENTER";
                // ДАТА,ДЮРЕЙШЕН,ЧИСЛО,текст,текст,текст,число,число,текст,текст
                //RQ.RepeatCell.Fields = RQ.RepeatCell.Cell.UserEnteredFormat.NumberFormat; //"numberFormat"; "userEnteredFormat.numberFormat"
                GridRange gr = new GridRange();
                gr.SheetId = shid;
                gr.StartRowIndex = 1;//row - 1;
                gr.EndRowIndex = 999;//row;
                gr.StartColumnIndex = i;
                gr.EndColumnIndex = i + 1;
                RQ.RepeatCell.Range = gr;
                BUF.Add(RQ);
            }
            return BUF;
        }

    }
}
