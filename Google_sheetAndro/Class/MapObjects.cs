using Newtonsoft.Json;
using System.Collections.Generic;
using Xamarin.Forms.GoogleMaps;

namespace Google_sheetAndro.Class
{
    public class MapObjects
    {
        private List<Polyline> polyline;
        private List<Pin> pins;
        public string SerializablePins { get { return Serialize(Pins); } }
        public string SerializableLine { get { return Serialize(Polylines); } }

        public List<Pin> Pins { get => pins; set => pins = value; }
        public List<Polyline> Polylines { get => polyline; set => polyline = value; }

        public MapObjects() { }
        public MapObjects(List<Pin> pn, List<Polyline> pl)
        {
            Polylines = pl;
            Pins = pn;
        }
        private string Serialize(object ToSerialize)
        {
            return JsonConvert.SerializeObject(ToSerialize);
        }
    }
}
