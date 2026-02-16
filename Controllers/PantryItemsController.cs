using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PantryTracker.Data;
using PantryTracker.Models;
using System.ComponentModel.DataAnnotations;

namespace PantryTracker.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PantryItemsController : ControllerBase
{
    private readonly PantryDbContext _context;
    private readonly ILogger<PantryItemsController> _logger;

    public PantryItemsController(PantryDbContext context, ILogger<PantryItemsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/pantryitems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PantryItem>>> GetPantryItems(
        [FromQuery] string? q = null,
        [FromQuery] bool? opened = null,
        [FromQuery] DateOnly? expiresBefore = null)
    {
        _logger.LogInformation("Fetching pantry items with filters: q={Q}, opened={Opened}, expiresBefore={ExpiresBefore}",
            q, opened, expiresBefore);

        var query = _context.PantryItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(item => item.Name.Contains(q));
        }

        if (opened.HasValue)
        {
            query = query.Where(item => item.IsOpened == opened.Value);
        }

        if (expiresBefore.HasValue)
        {
            query = query.Where(item => item.BestBefore <= expiresBefore.Value);
        }

        var items = await query.OrderBy(item => item.BestBefore).ToListAsync();
        return Ok(items);
    }

    // GET: api/pantryitems/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PantryItem>> GetPantryItem(int id)
    {
        _logger.LogInformation("Fetching pantry item with id: {Id}", id);

        var item = await _context.PantryItems.FindAsync(id);

        if (item == null)
        {
            _logger.LogWarning("Pantry item with id {Id} not found", id);
            return NotFound(new { error = $"Pantry item with id {id} not found" });
        }

        return Ok(item);
    }

    // POST: api/pantryitems
    [HttpPost]
    public async Task<ActionResult<PantryItem>> CreatePantryItem([FromBody] CreatePantryItemRequest request)
    {
        _logger.LogInformation("Creating new pantry item: {Name}", request.Name);

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);

        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            return BadRequest(new
            {
                error = "Validation failed",
                details = validationResults.Select(vr => vr.ErrorMessage)
            });
        }

        var item = new PantryItem
        {
            Name = request.Name,
            Quantity = request.Quantity,
            BestBefore = request.BestBefore,
            IsOpened = request.IsOpened,
            Notes = request.Notes
        };

        _context.PantryItems.Add(item);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created pantry item with id: {Id}", item.Id);
        return CreatedAtAction(nameof(GetPantryItem), new { id = item.Id }, item);
    }

    // GET: api/pantryitems/expiring?days=7
    [HttpGet("expiring")]
    public async Task<ActionResult<IEnumerable<PantryItem>>> GetExpiringItems([FromQuery] int days = 7)
    {
        _logger.LogInformation("Fetching items expiring within {Days} days", days);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thresholdDate = today.AddDays(days);

        var items = await _context.PantryItems
            .Where(item => item.BestBefore <= thresholdDate)
            .OrderBy(item => item.BestBefore)
            .ToListAsync();

        _logger.LogInformation("Found {Count} items expiring within {Days} days", items.Count, days);
        return Ok(items);
    }

    // PATCH: api/pantryitems/5/toggle-opened
    [HttpPatch("{id}/toggle-opened")]
    public async Task<ActionResult<PantryItem>> ToggleOpened(int id)
    {
        _logger.LogInformation("Toggling opened status for pantry item with id: {Id}", id);

        var item = await _context.PantryItems.FindAsync(id);

        if (item == null)
        {
            _logger.LogWarning("Pantry item with id {Id} not found", id);
            return NotFound(new { error = $"Pantry item with id {id} not found" });
        }

        item.IsOpened = !item.IsOpened;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Toggled opened status for pantry item {Id} to {IsOpened}", id, item.IsOpened);
        return Ok(item);
    }

    // DELETE: api/pantryitems/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePantryItem(int id)
    {
        _logger.LogInformation("Deleting pantry item with id: {Id}", id);

        var item = await _context.PantryItems.FindAsync(id);

        if (item == null)
        {
            _logger.LogWarning("Pantry item with id {Id} not found", id);
            return NotFound(new { error = $"Pantry item with id {id} not found" });
        }

        _context.PantryItems.Remove(item);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted pantry item with id: {Id}", id);
        return NoContent();
    }
}

public record CreatePantryItemRequest(
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    string Name,

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be >= 0")]
    int Quantity,

    [Required(ErrorMessage = "Best before date is required")]
    DateOnly BestBefore,

    [Required(ErrorMessage = "IsOpened is required")]
    bool IsOpened,

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    string? Notes
);
