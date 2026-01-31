namespace test.Models;
public abstract class Asset
{
    public string Name { get; set; }
    public DateTime PurchaseDate { get; set; }

    public Asset(string name)
    {
        Name = name;
        PurchaseDate = DateTime.Now;
    }

    public abstract decimal CalculateValue();

    public virtual string GetTypeDescription() => "Generic Asset";
}