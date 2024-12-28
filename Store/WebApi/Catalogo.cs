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
    }
}
