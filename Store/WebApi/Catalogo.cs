using Store.Repository;
using Store.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Store.WebApi
{
    public class Catalogo
    {
        [Route("catalogo")]
        [ApiController]
        public class Items : ControllerBase
        {
            // Crea l'istanza della connessione
            DbConnection connection = new DbConnection();


            [HttpGet]
            public async Task<IActionResult> Get()
            {
                List<Articolo> articoli = await connection.GetArticoli();

                // Trasforma la lista di Item in ItemDto
                var itemsDto = articoli.Select(item => new ArticoloDto(item.Id, item.Nome, item.Descrizione, item.Prezzo)).ToList();

                // Restituisci la lista di ItemDto come una risposta JSON
                return Ok(itemsDto);
            }
        }
}
