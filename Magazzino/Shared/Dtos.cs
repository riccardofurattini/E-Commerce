using System.ComponentModel.DataAnnotations;

namespace Magazzino.Shared
{
    public record ItemDto(Guid Id, string Nome, string Descrizione, double Prezzo, int Quantita);

    public record CreateItemDto([Required] string Nome, string Descrizione, [Range(0.01, 999999)] double Prezzo, int Quantita);

    public record UpdateItemDto([Required] string Nome, string Descrizione, [Range(0.01, 999999)] double Prezzo, int Quantita);
}
