namespace test.Models;

public class Cash : Asset
{
    private decimal amount;
    public decimal Amount
    {
        get { return this.amount; }
        set
        {
            if (value < 0)
                throw new ArgumentException("Amount cannot be negative");
            this.amount = value;
        }
    }

    public Cash(string name, decimal amount) : base(name)
    {
        Amount = amount;
    }

    public override decimal CalculateValue() => Amount;

    public override string GetTypeDescription() => "Cash Deposit";

}