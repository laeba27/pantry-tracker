using Microsoft.EntityFrameworkCore;
using PantryTracker.Models;

namespace PantryTracker.Data;

public class PantryDbContext : DbContext
{
    public PantryDbContext(DbContextOptions<PantryDbContext> options) : base(options) { }

    public DbSet<PantryItem> PantryItems => Set<PantryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PantryItem>(entity =>
        {
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.BestBefore).IsRequired();
            entity.Property(e => e.IsOpened).IsRequired();
            entity.Property(e => e.Notes).IsRequired(false);
        });
    }

    public static void Seed(PantryDbContext context)
    {
        if (context.PantryItems.Any()) return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var seedItems = new List<PantryItem>
        {
            new() { Name = "Organic Milk", Quantity = 1, BestBefore = today.AddDays(5), IsOpened = true, Notes = "Open - use within 5 days" },
            new() { Name = "Cheddar Cheese", Quantity = 2, BestBefore = today.AddDays(45), IsOpened = false, Notes = "Sealed block" },
            new() { Name = "Greek Yogurt", Quantity = 3, BestBefore = today.AddDays(10), IsOpened = false },
            new() { Name = "Whole Eggs", Quantity = 12, BestBefore = today.AddDays(-2), IsOpened = false, Notes = "Expired - discard" },
            new() { Name = "Fresh Spinach", Quantity = 1, BestBefore = today.AddDays(3), IsOpened = false },
            new() { Name = "Chicken Breast", Quantity = 2, BestBefore = today.AddDays(1), IsOpened = false, Notes = "Freeze if not using" },
            new() { Name = "Canned Tomatoes", Quantity = 5, BestBefore = today.AddDays(365), IsOpened = false, Notes = "Shelf stable" },
            new() { Name = "Olive Oil", Quantity = 1, BestBefore = today.AddDays(180), IsOpened = true, Notes = "Store in cool place" },
            new() { Name = "Maple Syrup", Quantity = 1, BestBefore = today.AddDays(120), IsOpened = false },
            new() { Name = "Fresh Strawberries", Quantity = 2, BestBefore = today.AddDays(-1), IsOpened = false, Notes = "Past prime - compost" }
        };

        context.PantryItems.AddRange(seedItems);
        context.SaveChanges();
    }
}
