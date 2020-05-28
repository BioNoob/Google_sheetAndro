using Google_sheetAndro.Class;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Google_sheetAndro.Services
{
    public class SaveService
    {
        public SaveService()
        {
            ti = new TableItem();
            CurrentMode = ActiveMode.newpage;
            CurrentPage = ActivePage.items;
        }
        public TableItem ti { get; set; }

        public enum ActivePage
        {
            map,
            item,
            wheather,
            items
        }
        public enum ActiveMode
        {
            newpage,
            watchpage
        }
        public ActivePage CurrentPage { get; set; }
        public ActiveMode CurrentMode { get; set; }
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static SaveService Deserialize(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                var t = JsonConvert.DeserializeObject<SaveService>(json);
                return t;
            }
            return null;
        }
    }
}
