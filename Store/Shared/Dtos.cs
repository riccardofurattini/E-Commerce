using System.ComponentModel.DataAnnotations;

namespace Store.Shared
{
    public record ArticoloDto(Guid Id, string Nome, string Descrizione, double Prezzo);
    public record CarrelloDto(Guid IdCarrello, ArticoloDto IdArticolo, int Quantita);

}
