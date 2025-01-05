using Microsoft.AspNetCore.Http.Features;
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
            List<Guid> items = await connection.GetCarrelli();
            if (items == null || !items.Any())
            {
                return NotFound("Nessun carrello trovato.");
            }


            return Ok(items.ToList());
        }

        // Metodo per ottenere un carrello per id
        [HttpGet("{idCarrello}")]
        public async Task<ActionResult<CarrelloDto>> GetById(Guid idCarrello)
        {
            // Recupera gli articoli e le rispettive quantità dal carrello
            var articoliQuantita = await connection.GetArticoliByCarrello(idCarrello);

            if (articoliQuantita == null || articoliQuantita.Count == 0)
            {
                return NotFound("Carrello vuoto o non trovato.");
            }

            // Crea una lista di articoli con la rispettiva quantità
            var articoliDto = articoliQuantita.Select(item => new CarrelloDto(
                idCarrello,
                new ArticoloDto(
                    item.articolo.Id,
                    item.articolo.Nome,
                    item.articolo.Descrizione,
                    item.articolo.Prezzo
                ),
                item.Quantita // Aggiungi la quantità per ogni articolo
            )).ToList();

            // Poiché CarrelloDto è una lista di carrelli, non c'è bisogno di un singolo oggetto
            return Ok(articoliDto); // Restituisce la lista di carrelli con i rispettivi articoli e quantità
        }




        // Metodo per aggiungere un articolo al carrello
        [HttpPost("{idCarrello}/articolo/{idArticolo}/quantita/{Quantita}")]
        public async Task<IActionResult> AddById(Guid idCarrello, Guid idArticolo, int Quantita)
        {
            var articoliDto = await connection.GetArticoloById(idArticolo);
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

        [HttpPut("{idCarrello}/articolo/{idArticolo}/quantita/{quantita}")]
        public async Task<IActionResult> UpdateQuantita(Guid idCarrello, Guid idArticolo, int quantita)
        {
            // Verifica se il carrello esiste
            bool carrelloEsiste = await connection.EsisteCarrello(idCarrello);
            if (!carrelloEsiste)
            {
                return NotFound("Carrello non trovato.");
            }

            // Verifica se l'articolo esiste nel carrello
            var articoloEsiste = await connection.EsisteArticoloNelCarrello(idCarrello, idArticolo);
            if (!articoloEsiste)
            {
                return NotFound("Articolo non trovato nel carrello.");
            }

            // Aggiorna la quantità dell'articolo nel carrello
            bool aggiornato = await connection.EditCarrello(idCarrello, idArticolo, quantita);
            if (aggiornato)
            {
                return Ok("Quantità dell'articolo aggiornata con successo.");
            }

            return BadRequest("Errore nell'aggiornamento della quantità dell'articolo.");
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
