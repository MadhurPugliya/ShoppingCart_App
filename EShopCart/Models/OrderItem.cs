using System.Text.Json.Serialization;
using EShopCart.Models;
public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }    
    public decimal TotalPrice { get; set; }
    [JsonIgnore]
    public Product Product { get; set; }
}

