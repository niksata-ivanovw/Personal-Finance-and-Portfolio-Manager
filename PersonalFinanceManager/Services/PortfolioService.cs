using test.Models;

public static class PortfolioService
{
    public static List<Asset> Assets { get; } = new List<Asset>();
    public static List<Transaction> Transactions { get; } = new List<Transaction>();

    static PortfolioService()
    {
        Assets.Add(new Cash("Trading Cash", 100_000));

        Transactions.Add(new Transaction
        {
            AssetName = "Trading Cash",
            Amount = 100_000,
            Type = "Initial Deposit",
            Date = DateTime.Now
        });
    }
}