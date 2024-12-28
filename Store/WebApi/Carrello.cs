using Microsoft.AspNetCore.Mvc;
using Store.Repository;
using Store.Shared;

namespace Store.WebApi
{
    [Route("carrello")]
    [ApiController]
    public class Carrello : ControllerBase
    {
        private readonly DbConnection connection = new DbConnection();

        // Metodo per ottenere tutti i carrelli
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<Articolo> items = await connection.GetCarrelli();
            if (items == null || !items.Any())
            {
                return NotFound("Nessun carrello trovato.");
            }
            var articoliDto = items.Select(item => new ArticoloDto(item.Id, item.Nome, item.Descrizione, item.Prezzo)).ToList();

            return Ok(articoliDto);
        }

        // Metodo per ottenere un carrello per id
        [HttpGet("{idCarrello}")]
        public async Task<ActionResult<CarrelloDto>> GetById(Guid idCarrello)
        {
            var articoli = await connection.GetArticoliByCarrello(idCarrello);
            if (articoli == null || articoli.Count == 0)
            {
                return NotFound("Carrello vuoto o non trovato.");
            }

            var articoliDto = articoli.Select(item => new ArticoloDto(item.Id, item.Nome, item.Descrizione, item.Prezzo)).ToList();
            return Ok(articoliDto);
        }

        // Metodo per aggiungere un articolo al carrello
        [HttpPost("{idCarrello}/articolo/{idArticolo}/quantita/{Quantita}")]
        public async Task<IActionResult> AddById(Guid idCarrello, Guid idarticolo,int Quantita)
        {
            var articoliDto = await connection.GetArticoloById(idarticolo);
            // Verifica se il carrello esiste già, altrimenti crealo
            bool carrelloEsiste = await connection.EsisteCarrello(idCarrello);
            if (!carrelloEsiste)
            {
                idCarrello = Guid.NewGuid();
                if (idCarrello == Guid.Empty)
                {
                    return BadRequest("Errore nella creazione del carrello.");
                }
            }

            // Aggiungi articolo al carrello
            var result = await connection.AddArticoloById(idCarrello, articoliDto.Id, Quantita);
            if (result)
            {
                return Ok("Articolo aggiunto al carrello.");
            }

            return BadRequest("Errore nell'aggiungere l'articolo al carrello.");
        }

        // Metodo per eliminare un carrello
        [HttpDelete("{idCarrello}")]
        public async Task<IActionResult> DeleteCarrello(Guid idCarrello)
        {
            try
            {
                var result = await connection.DeleteCarrello(idCarrello);
                if (result)
                {
                    return Ok("Carrello eliminato con successo.");
                }
                else
                {
                    return NotFound("Carrello non trovato.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore del server: {ex.Message}");
            }
        }

        // Metodo per eliminare un articolo dal carrello
        [HttpDelete("{idCarrello}/articolo/{idArticolo}")]
        public async Task<IActionResult> DeleteArticolo(Guid idCarrello, Guid idArticolo)
        {
            try
            {
                var result = await connection.DeleteArticolo(idCarrello, idArticolo);
                if (result)
                {
                    return Ok("Articolo rimosso dal carrello.");
                }
                else
                {
                    return NotFound("Articolo non trovato nel carrello.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore del server: {ex.Message}");
            }
        }
    }
}
