
using System.ComponentModel.DataAnnotations;

namespace FoodieHub.API.DTOs;

public class CreateOrderDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    // Email is optional because the current checkout form
    // does not collect an email address.
    public string? Email { get; set; }

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required]
    public string PaymentMethod { get; set; } = "Cash on Delivery";

    [Required]
    [MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }
}
