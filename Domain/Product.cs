
namespace ProductManager.Domain;

public class Product
{
    public int Id { get; set; }
    // auto-implemented properties som har get och set metoder

    public required string Name { get; set; }

    public required string Sku { get; set; }

    public required string Description { get; set; }

    public required string Image { get; set; }

    public required decimal Price { get; set; }

}
