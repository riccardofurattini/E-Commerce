using Microsoft.AspNetCore.Mvc;
using Store.ClientHttp;
using Store.Repository;
using Store.Shared;

namespace Store.WebApi
{
    [Route("catalogo")]
    [ApiController]
    public class Catalogo : ControllerBase
    {
        private readonly DbConnection connection =  new DbConnection();
        

        

        [HttpGet]
        public async Task<IActionResult> Get()
        {

            List<Articolo> items = await connection.GetArticoli();
            var articoliDto = items.Select(item => new ArticoloDto(item.Id, item.Nome, item.Descrizione, item.Prezzo)).ToList();


            // Restituisci la lista di ItemDto come una risposta JSON
            return Ok(articoliDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticoloDto>> GetById(Guid id)
        {
            // Recupera l'articolo dal database
            var item = await connection.GetArticoloById(id);

            if (item == null)
            {
                return NotFound(); // Se l'articolo non esiste, restituisci NotFound
            }

            // Mappa l'oggetto Item in un ItemDto
            var itemDto = new ArticoloDto(item.Id, item.Nome, item.Descrizione, item.Prezzo);

            
            // Restituisci l'ItemDto trovato
            return Ok(itemDto);
        }
    }
}
