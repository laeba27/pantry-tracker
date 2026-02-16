using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PantryTracker.Controllers;
using PantryTracker.Data;
using PantryTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace PantryTracker.Tests;

public class PantryItemTests
{
    private PantryDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<PantryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PantryDbContext(options);
    }

    private PantryItemsController GetController(PantryDbContext context)
    {
        var logger = new Mock<ILogger<PantryItemsController>>();
        return new PantryItemsController(context, logger.Object);
    }

    #region Model Tests

    [Fact]
    public void PantryItem_DaysUntilExpiry_CalculatesCorrectly()
    {
        // Arrange
        var item = new PantryItem
        {
            Name = "Test Item",
            Quantity = 1,
            BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            IsOpened = false
        };

        // Act
        var daysUntilExpiry = item.DaysUntilExpiry;

        // Assert
        Assert.Equal(5, daysUntilExpiry);
    }

    [Fact]
    public void PantryItem_IsExpired_ReturnsTrueForPastDate()
    {
        // Arrange
        var item = new PantryItem
        {
            Name = "Expired Item",
            Quantity = 1,
            BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
            IsOpened = false
        };

        // Assert
        Assert.True(item.IsExpired);
    }

    [Fact]
    public void PantryItem_IsExpired_ReturnsFalseForFutureDate()
    {
        // Arrange
        var item = new PantryItem
        {
            Name = "Fresh Item",
            Quantity = 1,
            BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            IsOpened = false
        };

        // Assert
        Assert.False(item.IsExpired);
    }

    #endregion

    #region Controller Tests

    [Fact]
    public async Task GetPantryItems_ReturnsAllItems()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        context.PantryItems.AddRange(
            new PantryItem { Name = "Milk", Quantity = 1, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), IsOpened = false },
            new PantryItem { Name = "Bread", Quantity = 2, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)), IsOpened = true }
        );
        await context.SaveChangesAsync();

        var controller = GetController(context);

        // Act
        var result = await controller.GetPantryItems();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<PantryItem>>(okResult.Value);
        Assert.Equal(2, items.Count());
    }

    [Fact]
    public async Task GetPantryItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var controller = GetController(context);

        // Act
        var result = await controller.GetPantryItem(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreatePantryItem_AddsItemToDatabase()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var controller = GetController(context);

        var request = new CreatePantryItemRequest(
            Name: "New Item",
            Quantity: 3,
            BestBefore: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            IsOpened: false,
            Notes: "Test notes"
        );

        // Act
        var result = await controller.CreatePantryItem(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var item = Assert.IsType<PantryItem>(createdResult.Value);
        Assert.Equal("New Item", item.Name);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(1, await context.PantryItems.CountAsync());
    }

    [Fact]
    public async Task ToggleOpened_TogglesItemStatus()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var item = new PantryItem
        {
            Name = "Toggle Test",
            Quantity = 1,
            BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            IsOpened = false
        };
        context.PantryItems.Add(item);
        await context.SaveChangesAsync();

        var controller = GetController(context);

        // Act
        var result = await controller.ToggleOpened(item.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var toggledItem = Assert.IsType<PantryItem>(okResult.Value);
        Assert.True(toggledItem.IsOpened);
    }

    [Fact]
    public async Task DeletePantryItem_RemovesItemFromDatabase()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var item = new PantryItem
        {
            Name = "To Delete",
            Quantity = 1,
            BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            IsOpened = false
        };
        context.PantryItems.Add(item);
        await context.SaveChangesAsync();

        var controller = GetController(context);

        // Act
        var result = await controller.DeletePantryItem(item.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await context.PantryItems.CountAsync());
    }

    [Fact]
    public async Task GetExpiringItems_ReturnsOnlyExpiringItems()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        context.PantryItems.AddRange(
            new PantryItem { Name = "Expiring Soon", Quantity = 1, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)), IsOpened = false },
            new PantryItem { Name = "Not Expiring", Quantity = 1, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), IsOpened = false }
        );
        await context.SaveChangesAsync();

        var controller = GetController(context);

        // Act
        var result = await controller.GetExpiringItems(7);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<PantryItem>>(okResult.Value);
        Assert.Single(items);
        Assert.Equal("Expiring Soon", items.First().Name);
    }

    #endregion
}
