using Magazzino.Repository;
using Magazzino.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data.Common;

namespace Magazzino.WebApi
{
    [Route("items")]
    [ApiController]
    public class Items : ControllerBase
    {
        // Crea l'istanza della connessione
        ItemsConnection connection = new ItemsConnection();


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<Item> items = await connection.GetItemsAsync();

            // Trasforma la lista di Item in ItemDto
            var itemsDto = items.Select(item => new ItemDto(item.Id, item.Nome, item.Descrizione, item.Prezzo, item.Quantita)).ToList();

            // Restituisci la lista di ItemDto come una risposta JSON
            return Ok(itemsDto);
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetById(Guid id)
        {
            // Recupera l'articolo dal database
            var item = await connection.GetItemById(id); 

            if (item == null)
            {
                return NotFound(); // Se l'articolo non esiste, restituisci NotFound
            }

            // Mappa l'oggetto Item in un ItemDto
            var itemDto = new ItemDto(item.Id, item.Nome, item.Descrizione, item.Prezzo, item.Quantita);

            // Restituisci l'ItemDto trovato
            return Ok(itemDto);
        }


        [HttpPost]
        public async Task<ActionResult<ItemDto>> Post(CreateItemDto createItemDto)
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

            try
            {
                // Salva l'item nel database in modo asincrono
                await connection.AddItemAsync(item);

                // Mappa l'oggetto Item in ItemDto per la risposta
                var itemDto = new ItemDto(item.Id, item.Nome, item.Descrizione, item.Prezzo, item.Quantita);

                // Restituisci un "Created" con la locazione dell'oggetto appena creato
                return CreatedAtAction(nameof(GetById), new { id = item.Id }, itemDto);
            }
            catch (Exception ex)
            {
                // Gestione degli errori (nel caso di errore durante il salvataggio)
                Console.WriteLine($"Errore durante il salvataggio dell'articolo: {ex.Message}");
                return StatusCode(500, "Errore interno del server"); // Risposta 500 in caso di errore
            }
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UpdateItemDto updateItemDto)
        {
            // Verifica che il modello sia valido
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Recupera l'item esistente dal database
                var existingItem = await connection.GetItemById(id);

                // Se l'item non esiste, restituisce NotFound
                if (existingItem == null)
                {
                    return NotFound();
                }

                // Applica le modifiche
                existingItem.Nome = updateItemDto.Nome;
                existingItem.Descrizione = updateItemDto.Descrizione;
                existingItem.Prezzo = updateItemDto.Prezzo;
                existingItem.Quantita = updateItemDto.Quantita;

                // Salva le modifiche nel database in modo asincrono
                await connection.UpdateItemAsync(existingItem);

                // Restituisce NoContent se l'operazione è andata a buon fine
                return Ok();
            }
            catch (Exception ex)
            {
                // Gestione degli errori in caso di problemi nel processo
                Console.WriteLine($"Errore durante l'aggiornamento dell'articolo: {ex.Message}");
                return StatusCode(500, "Errore interno del server");
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Recupera l'item esistente dal database
                var existingItem = await connection.GetItemById(id);

                // Se l'item non esiste, restituisce NotFound
                if (existingItem == null)
                {
                    return NotFound();
                }

                // Rimuovi l'item dal database
                await connection.DeleteItemAsync(id);

                // Restituisci NoContent se l'operazione è andata a buon fine
                return Ok();
            }
            catch (Exception ex)
            {
                // Gestione degli errori in caso di problemi nel processo
                Console.WriteLine($"Errore durante la cancellazione dell'articolo: {ex.Message}");
                return StatusCode(500, "Errore interno del server");
            }
        }


    }


}
