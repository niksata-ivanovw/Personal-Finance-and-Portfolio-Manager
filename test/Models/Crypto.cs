namespace test.Models;

public class Crypto : Asset
{
    private decimal quantity;
    private decimal price;
    public decimal Price
    {
        get { return this.price; }
        set
        {
            if (value < 0)
                throw new ArgumentException("Amount cannot be negative");
            this.price = value;
        }
    }
    public decimal Quantity
    {
        get { return this.quantity; }
        set
        {
            if (value < 0)
                throw new ArgumentException("Amount cannot be negative");
            this.quantity = value;
        }
    }

    public Crypto(string name, decimal quantity, decimal price) : base(name)
    {
        Quantity = quantity;
        Price = price;
    }

    public override decimal CalculateValue() => Quantity * Price;

    public override string GetTypeDescription() => "Crypto Purchase";
}