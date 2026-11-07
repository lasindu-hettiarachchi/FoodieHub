
using FoodieHub.API.Data;
using FoodieHub.API.DTOs;
using FoodieHub.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodieHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly FoodieHubDbContext _context;

    public OrdersController(FoodieHubDbContext context)
    {
        _context = context;
    }

    // CREATE A NEW ORDER
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
    {
        // 1. Get product IDs
        var productIds = dto.Items
            .Select(item => item.ProductId)
            .Distinct()
            .ToList();

        // 2. Find products in database
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        // 3. Check whether all products exist
        if (products.Count != productIds.Count)
        {
            return BadRequest("One or more products do not exist.");
        }

        // 4. Check product availability
        if (products.Any(p => !p.IsAvailable))
        {
            return BadRequest("One or more products are unavailable.");
        }

        // 5. Calculate total amount
        decimal totalAmount = 0;

        foreach (var item in dto.Items)
        {
            var product = products.First(
                p => p.Id == item.ProductId);

            totalAmount += product.Price * item.Quantity;
        }

        // 6. Create customer
        var customer = new Customer
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email?.Trim() ?? string.Empty,
            PhoneNumber = dto.PhoneNumber.Trim(),
            Address = dto.DeliveryAddress.Trim()
        };

        // 7. Create order
        var order = new Order
        {
            Customer = customer,
            OrderDate = DateTime.UtcNow,
            TotalAmount = totalAmount,
            Status = "Pending",
            PaymentMethod = dto.PaymentMethod.Trim(),
            DeliveryAddress = dto.DeliveryAddress.Trim()
        };

        // 8. Add order items
        foreach (var item in dto.Items)
        {
            var product = products.First(
                p => p.Id == item.ProductId);

            order.OrderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        // 9. Save order to database
        _context.Orders.Add(order);

        await _context.SaveChangesAsync();

        // 10. Return success response
        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = order.Id },
            new
            {
                message = "Order created successfully!",
                orderId = order.Id,
                customerName = customer.FullName,
                totalAmount = order.TotalAmount,
                status = order.Status
            });
    }


    // GET ALL ORDERS
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Select(order => new
            {
                order.Id,
                order.OrderDate,
                order.TotalAmount,
                order.Status,
                order.PaymentMethod,
                order.DeliveryAddress,

                Customer = order.Customer == null ? null : new
                {
                    order.Customer.Id,
                    order.Customer.FullName,
                    order.Customer.PhoneNumber,
                    order.Customer.Email
                },

                OrderItems = order.OrderItems.Select(item => new
                {
                    item.Id,
                    item.ProductId,
                    ProductName = item.Product != null
                        ? item.Product.Name
                        : "Unknown Product",
                    item.Quantity,
                    item.UnitPrice,
                    LineTotal = item.Quantity * item.UnitPrice
                })
            })
            .ToListAsync();

        return Ok(orders);
    }


    // GET ORDER BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound("Order not found.");
        }

        // Return selected data only
        // This prevents JSON circular reference errors
        var result = new
        {
            order.Id,
            order.OrderDate,
            order.TotalAmount,
            order.Status,
            order.PaymentMethod,
            order.DeliveryAddress,

            Customer = order.Customer == null ? null : new
            {
                order.Customer.Id,
                order.Customer.FullName,
                order.Customer.Email,
                order.Customer.PhoneNumber
            },

            OrderItems = order.OrderItems.Select(item => new
            {
                item.Id,
                item.ProductId,
                ProductName = item.Product?.Name,
                item.Quantity,
                item.UnitPrice,
                LineTotal = item.Quantity * item.UnitPrice
            })
        };

        return Ok(result);
    }
}
