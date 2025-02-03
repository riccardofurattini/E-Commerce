using Magazzino.Repository;
using Microsoft.AspNetCore.Mvc;
using Magazzino.Shared;
using Newtonsoft.Json;

namespace Magazzino.WebApi
{
    [Route("modificaQuantita")]
    [ApiController]
    public class Quantita : ControllerBase
    {
        private readonly ItemsConnection _itemsConnection;

        // Iniettiamo ItemsConnection tramite Dependency Injection
        public Quantita(ItemsConnection itemsConnection)
        {
            _itemsConnection = itemsConnection;
        }

        // PUT api/magazzino/{id}
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(ModificaQuantitaDto modifica)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dati di input non validi.",
                        errors = ModelState
                    });
                }

                Console.WriteLine(JsonConvert.SerializeObject(modifica));

                // Recupera l'articolo dal database
                var item = await _itemsConnection.GetItemByIdAsync(modifica.id);
                if (item == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Articolo non trovato."
                    });
                }

                // Chiamata al servizio esterno per verificare la disponibilità della quantità
                bool suffieciente = await _itemsConnection.ControlloQuantitaAsync(modifica.id, modifica.Quantita);
                if (!suffieciente)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Quantità insufficiente."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Quantità aggiornata con successo."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'aggiornamento della quantità: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Errore interno del server.",
                    details = ex.Message
                });
            }
        }

    }
}
