using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoController(TodoContext context)
        {
            _context = context;

            if (_context.TodoItems.Count() == 0)
            {
                _context.TodoItems.Add(new TodoItem { Name = "Items1" });
                _context.SaveChanges();
            }
        }

        [HttpGet]
        public ActionResult<List<TodoItem>> GetAll() //  /api/todo
        {
            return _context.TodoItems.ToList();
        }

        //Name = "GetTodo" 创建具名路由。 具名路由：
        //使应用程序使用路由名称创建 HTTP 链接。
        [HttpGet("{id}", Name = "GetTodo")]
        public ActionResult<TodoItem> GetById(long id) ///api/todo/{id}
        {
            TodoItem item = _context.TodoItems.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            return item;
        }



        [HttpPost]
        public IActionResult Create(TodoItem item)
        {
            _context.TodoItems.Add(item);
            _context.SaveChanges();

            //CreatedAtRoute 方法：
            //返回 201 响应。 HTTP 201 是在服务器上创建新资源的 HTTP POST 方法的标准响应。
            //向响应添加位置标头。 位置标头指定新建的待办事项的 URI。
            return CreatedAtRoute("GetTodo", new { id = item.Id }, item);


        }


        //根据 HTTP 规范，PUT 请求需要客户端发送整个更新的实体，而不仅仅是增量。 若要支持部分更新，请使用 HTTP PATCH。
        [HttpPut("{id}")]
        public IActionResult Update(long id , TodoItem item)
        {
            var todo = _context.TodoItems.Find(id);
            if(todo==null)
            {
                return NotFound();
            }

            todo.IsComplete = item.IsComplete;
            todo.Name = item.Name;

            _context.TodoItems.Update(todo);
            _context.SaveChanges();
            // 响应是 204（无内容）
            return NoContent();
        }


        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            var todo = _context.TodoItems.Find(id);
            if(todo==null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todo);
            _context.SaveChanges();

            return NoContent();
        }
    }



}