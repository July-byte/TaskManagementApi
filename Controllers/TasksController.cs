using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagementApi.Data;
using TaskManagementApi.Models;

namespace TaskManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // Получить все задачи текущего пользователя
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            int userId = GetUserId();

            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();

            return Ok(tasks);
        }

        // Создать задачу
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskItem task)
        {
            task.UserId = GetUserId();

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok(task);
        }

        // Завершить задачу
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteTask(int id)
        {
            int userId = GetUserId();

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                return NotFound();

            task.IsCompleted = true;
            await _context.SaveChangesAsync();

            return Ok(task);
        }

        // Удалить задачу
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            int userId = GetUserId();

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok("Задача удалена");
        }
    }
} 
