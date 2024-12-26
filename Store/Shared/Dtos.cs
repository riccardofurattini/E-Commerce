using System.ComponentModel.DataAnnotations;

namespace Store.Shared
{
    public record ArticoloDto(Guid Id, string Nome, string Descrizione, double Prezzo);
    public record CarrelloDto(Guid IdUtente, Guid IdCarrello, Guid IdArticolo, string Nome, double Prezzo, int Quantita);

}
