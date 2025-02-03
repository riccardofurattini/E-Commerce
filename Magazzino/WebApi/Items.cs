using Magazzino.Repository;
using Magazzino.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Magazzino.WebApi
{
    [Route("items")]
    [ApiController]
    public class Items : ControllerBase
    {
        private readonly ItemsConnection _itemsConnection;

        // Iniettiamo ItemsConnection tramite Dependency Injection
        public Items(ItemsConnection itemsConnection)
        {
            _itemsConnection = itemsConnection;
        }

        // GET api/items
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Usa ItemsConnection per ottenere gli articoli dal database
                var items = await _itemsConnection.GetItemsAsync();

                if (items == null || !items.Any())
                {
                    return NotFound(); // Se non ci sono articoli, restituisce NotFound
                }

                // Trasformazione della lista di articoli in ItemDto
                var itemsDto = items.Select(item => new ItemDto(item.Id, item.Nome, item.Descrizione, item.Prezzo, item.Quantita)).ToList();

                // Restituisci gli articoli come JSON
                return Ok(itemsDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il recupero degli articoli: {ex.Message}");
                return StatusCode(500, "Errore interno del server");
            }
        }

        // GET api/items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetById(Guid id)
        {
            try
            {
                // Usa ItemsConnection per ottenere l'articolo per ID
                var item = await _itemsConnection.GetItemByIdAsync(id);

                if (item == null)
                {
                    return NotFound(); // Se l'articolo non esiste, restituisce NotFound
                }

                // Mappa l'oggetto Item in un ItemDto
                var itemDto = new ItemDto(item.Id, item.Nome, item.Descrizione, item.Prezzo, item.Quantita);

                // Restituisci l'ItemDto trovato
                return Ok(itemDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il recupero dell'articolo: {ex.Message}");
                return StatusCode(500, "Errore interno del server");
            }
        }

        // POST api/items
        [HttpPost]
        public async Task<ActionResult<ItemDto>> Post(CreateItemDto createItemDto)
        {
            try
            {
                // Validazione del modello
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Mappatura da CreateItemDto a Item
                var item = new Item
                {
                    Id = Guid.NewGuid(),
                    Nome = createItemDto.Nome,
                    Descrizione = createItemDto.Descrizione,
                    Prezzo = createItemDto.Prezzo,
                    Quantita = createItemDto.Quantita
                };

                // Usa ItemsConnection per aggiungere l'articolo
                await _itemsConnection.AddItemAsync(item);

                // Mappa l'oggetto Item in ItemDto per la risposta
                var itemDto = new ItemDto(item.Id, item.Nome, item.Descrizione, item.Prezzo, item.Quantita);

                // Restituisci un "Created" con la locazione dell'oggetto appena creato
                return CreatedAtAction(nameof(GetById), new { id = item.Id }, itemDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il salvataggio dell'articolo: {ex.Message}");
                return StatusCode(500, "Errore interno del server"); // Risposta 500 in caso di errore
            }
        }

        // PUT api/items/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UpdateItemDto updateItemDto)
        {
            try
            {
                // Verifica che il modello sia valido
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Mappatura da UpdateItemDto a Item
                var item = new Item
                {
                    Id = id,
                    Nome = updateItemDto.Nome,
                    Descrizione = updateItemDto.Descrizione,
                    Prezzo = updateItemDto.Prezzo,
                    Quantita = updateItemDto.Quantita
                };

                // Usa ItemsConnection per aggiornare l'articolo
                await _itemsConnection.UpdateItemAsync(item);

                // Restituisce NoContent se l'operazione è andata a buon fine
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'aggiornamento dell'articolo: {ex.Message}");
                return StatusCode(500, "Errore interno del server");
            }
        }

        // DELETE api/items/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Usa ItemsConnection per eliminare l'articolo
                await _itemsConnection.DeleteItemAsync(id);

                // Restituisci NoContent se l'operazione è andata a buon fine
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la cancellazione dell'articolo: {ex.Message}");
                return StatusCode(500, "Errore interno del server");
            }
        }


    }
}
