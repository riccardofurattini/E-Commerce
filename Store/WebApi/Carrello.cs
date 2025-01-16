using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Shared;
using Store.Repository;

namespace Store.WebApi
{
    [Route("carrello")]
    [ApiController]
    public class CarrelloController : ControllerBase
    {
        private readonly DbConnection _dbConnection;

        public CarrelloController(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Metodo per ottenere tutti i carrelli
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var carrelli = await _dbConnection.GetCarrelli();

            if (carrelli == null || !carrelli.Any())
            {
                return NotFound("Nessun carrello trovato.");
            }

            return Ok(carrelli);
        }

        // Metodo per ottenere un carrello per id
        [HttpGet("{idCarrello}")]
        public async Task<ActionResult<IEnumerable<CarrelloDto>>> GetById(Guid idCarrello)
        {
            var articoliQuantita = await _dbConnection.GetArticoliByCarrello(idCarrello);

            if (articoliQuantita == null || !articoliQuantita.Any())
            {
                return NotFound("Carrello vuoto o non trovato.");
            }

            var articoliDto = articoliQuantita.Select(item => new CarrelloDto(
                idCarrello,
                new ArticoloDto(item.articolo.Id, item.articolo.Nome, item.articolo.Descrizione, item.articolo.Prezzo),
                item.Quantita
            )).ToList();

            return Ok(articoliDto);
        }

        // Metodo per aggiungere un articolo al carrello
        [HttpPost("{idCarrello}/articolo/{idArticolo}/quantita/{quantita}")]
        public async Task<IActionResult> AddById(Guid idCarrello, Guid idArticolo, int quantita)
        {
            bool result = await _dbConnection.AddArticoloById(idCarrello, idArticolo, quantita);

            if (!result)
            {
                return NotFound("Articolo non trovato o errore durante l'aggiunta.");
            }

            return Ok("Articolo aggiunto o aggiornato nel carrello.");
        }

        // Metodo per aggiornare la quantità di un articolo nel carrello
        [HttpPut("{idCarrello}/articolo/{idArticolo}/quantita/{quantita}")]
        public async Task<IActionResult> UpdateQuantita(Guid idCarrello, Guid idArticolo, int quantita)
        {
            bool result = await _dbConnection.EditCarrello(idCarrello, idArticolo, quantita);

            if (!result)
            {
                return NotFound("Articolo non trovato nel carrello.");
            }

            return Ok("Quantità dell'articolo aggiornata con successo.");
        }

        // Metodo per eliminare un carrello
        [HttpDelete("{idCarrello}")]
        public async Task<IActionResult> DeleteCarrello(Guid idCarrello)
        {
            bool result = await _dbConnection.DeleteCarrello(idCarrello);

            if (!result)
            {
                return NotFound("Carrello non trovato.");
            }

            return Ok("Carrello eliminato con successo.");
        }

        // Metodo per eliminare un articolo dal carrello
        [HttpDelete("{idCarrello}/articolo/{idArticolo}")]
        public async Task<IActionResult> DeleteArticolo(Guid idCarrello, Guid idArticolo)
        {
            bool result = await _dbConnection.DeleteArticolo(idCarrello, idArticolo);

            if (!result)
            {
                return NotFound("Articolo non trovato nel carrello.");
            }

            return Ok("Articolo rimosso dal carrello.");
        }
    }
}
