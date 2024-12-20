namespace Magazzino.Shared
{
    public class Item
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Descrizione { get; set; }
        public double Prezzo { get; set; }
        public int Quantita { get; set; }

    }
}
