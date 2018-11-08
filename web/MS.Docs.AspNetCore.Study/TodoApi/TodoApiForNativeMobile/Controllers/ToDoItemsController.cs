using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Pages;
using Microsoft.AspNetCore.Mvc;
using TodoApiForNativeMobile.Models;

namespace TodoApiForNativeMobile.Controllers
{
    [Route("api/[controller]")]
    public class ToDoItemsController : Controller
    {
        private readonly IToDoRepository _toDoRepository;


        public ToDoItemsController(IToDoRepository toDoRepository)
        {
            this._toDoRepository = toDoRepository;
        }


        [HttpGet]
        public IActionResult List()
        {
            return Ok(_toDoRepository.All);
        }

        //由于 item 参数将在 POST 的正文中传递，因此该参数用 [FromBody] 属性修饰
        [HttpPost]
        public IActionResult Create([FromBody] ToDoItem item)
        {
            try
            {
                if (item == null || !ModelState.IsValid)
                {
                    return BadRequest(ErrorCode.TodoItemNameAndNotesRequired.ToString());
                }

                bool itemExists = _toDoRepository.DoesItemExist(item.ID);
                if (itemExists)
                {
                    return StatusCode(StatusCodes.Status409Conflict, ErrorCode.TodoItemIDInUse.ToString());
                }
                _toDoRepository.Insert(item);
            }
            catch (Exception)
            {
                return BadRequest(ErrorCode.CouldNotCreateItem.ToString());
            }
            return Ok(item);
        }
         
        
        [HttpPut]
        public IActionResult Edit([FromBody] ToDoItem item)
        {
            try
            {
                if(item==null||!ModelState.IsValid)
                {
                    return BadRequest(ErrorCode.TodoItemNameAndNotesRequired.ToString());
                }
                var existingItem = _toDoRepository.Find(item.ID);
                if(existingItem==null)
                {
                    return NotFound(ErrorCode.RecordNotFound.ToString());
                }
                _toDoRepository.Update(item);
            }
            catch (Exception)
            {
                return BadRequest(ErrorCode.CouldNotUpdateItem.ToString());
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                var item = _toDoRepository.Find(id);
                if(item==null)
                {
                    return NotFound(ErrorCode.RecordNotFound.ToString());
                }
                _toDoRepository.Delete(id);
            }
            catch (Exception)
            {
                return BadRequest(ErrorCode.CouldNotDeleteItem.ToString());
            }
            return NoContent();
        }
    }
}