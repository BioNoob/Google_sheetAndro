using System;
using System.Collections.Generic;
using System.Text;

namespace Google_sheetAndro.Class
{
    public class ResponsedData
    {
        public DateTime duration { get; set; }
        public float temperature { get; set; }
        public float pressure { get; set; }
        public float windspeed { get; set; }
        public string cloud { get; set; }

        public ResponsedData()
        {

        }
    }
}
