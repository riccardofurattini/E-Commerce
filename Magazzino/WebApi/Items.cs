using Magazzino.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Magazzino.WebApi
{
    [Route("items")]
    [ApiController]
    public class Items : ControllerBase
    {
        private static readonly List<ItemDto> items = new()
        {
            new ItemDto(Guid.NewGuid(),"Mele","Frutta",1.99,250),
            new ItemDto(Guid.NewGuid(),"Pere","Frutta",1.50,400),
            new ItemDto(Guid.NewGuid(),"Fragole","Frutta",2.99,100)
        };

        [HttpGet]
        public IEnumerable<ItemDto> Get()
        {
            return items;
        }

        [HttpGet("{id}")]
        public ActionResult<ItemDto> GetById(Guid id)
        {
            var item = items.Where(item=> item.Id == id).FirstOrDefault();

            if (item == null)
            {
                return NotFound();
            }

            return item;
            
        }

        [HttpPost]
        public ActionResult<ItemDto> Post(CreateItemDto createItemDto)
        {
            var item = new ItemDto(Guid.NewGuid(), createItemDto.Nome, createItemDto.Descrizione, createItemDto.Prezzo, createItemDto.Quantita);
            items.Add(item);

            return CreatedAtAction(nameof(GetById), new {id = item.Id}, item);
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem =  items.Where(item => item.Id == id).FirstOrDefault();

            if (existingItem == null)
            {
                return NotFound();
            }

            var updateItem = existingItem with
            {
                Nome = updateItemDto.Nome,
                Descrizione = updateItemDto.Descrizione,
                Prezzo = updateItemDto.Prezzo,
                Quantita = updateItemDto.Quantita
            };
            var index = items.FindIndex(existingItem =>  existingItem.Id == id);
            items[index] = updateItem;

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var index = items.FindIndex(existingItem => existingItem.Id == id);

            if (index < 0)
            {
                return NotFound();
            }

            items.RemoveAt(index);

            return NoContent();
        }
    }

    
}
