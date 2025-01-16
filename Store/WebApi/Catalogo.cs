using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Shared;
using Store.Repository;
using System.Linq;

namespace Store.WebApi
{
    [Route("catalogo")]
    [ApiController]
    public class Catalogo : ControllerBase
    {
        private readonly StoreDbContext _context;

        public Catalogo(StoreDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Recupera tutti gli articoli dal database utilizzando EF Core
            var items = await _context.Articoli.ToListAsync();

            // Mappa gli articoli in DTO
            var articoliDto = items
                .Select(item => new ArticoloDto(item.Id, item.Nome, item.Descrizione, item.Prezzo))
                .ToList();

            // Restituisci la lista di ArticoloDto come una risposta JSON
            return Ok(articoliDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticoloDto>> GetById(Guid id)
        {
            // Recupera l'articolo dal database utilizzando EF Core
            var item = await _context.Articoli.FindAsync(id);

            if (item == null)
            {
                return NotFound(); // Se l'articolo non esiste, restituisci NotFound
            }

            // Mappa l'oggetto Articolo in un ArticoloDto
            var itemDto = new ArticoloDto(item.Id, item.Nome, item.Descrizione, item.Prezzo);

            // Restituisci l'ArticoloDto trovato
            return Ok(itemDto);
        }
    }
}
