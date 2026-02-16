using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PantryTracker.Models;

public class PantryItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Best before date is required")]
    public DateOnly BestBefore { get; set; }

    [Required(ErrorMessage = "IsOpened is required")]
    public bool IsOpened { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    public int DaysUntilExpiry => BestBefore.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;

    public bool IsExpired => DaysUntilExpiry < 0;
}
