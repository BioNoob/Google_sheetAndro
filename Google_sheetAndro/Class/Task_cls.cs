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
        public Task_cls(string numer, string chto)
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