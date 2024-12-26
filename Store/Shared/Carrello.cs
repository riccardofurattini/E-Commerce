namespace Store.Shared
{
    public class Carrello
    {
        public Guid IdUtente { get; set; }
        public Guid IdCarrello { get; set; }
        public Articolo articolo { get; set; }
        public int Quantita { get; set; }
    }
}
