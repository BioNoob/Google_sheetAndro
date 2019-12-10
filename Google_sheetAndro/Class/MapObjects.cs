using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.GoogleMaps;

namespace Google_sheetAndro.Class
{
    public class MapObjects
    {
        private Polyline polyline;
        private List<Pin> pins;
        public string SerializablePins { get { return Serialize(Pins); } }
        public string SerializableLine { get { return Serialize(Polyline); } }

        public List<Pin> Pins { get => pins; set => pins = value; }
        public Polyline Polyline { get => polyline; set => polyline = value; }

        public MapObjects(){ }
        public MapObjects(List<Pin> pn,Polyline pl)
        {
            Polyline = pl;
            Pins = pn;
        }
        private string Serialize(object ToSerialize)
        {
            return JsonConvert.SerializeObject(ToSerialize);
        }
    }
}
