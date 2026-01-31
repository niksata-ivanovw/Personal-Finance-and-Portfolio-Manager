namespace test.Models
{
    public class Transaction
    {
        public string AssetName { get; set; } = default!;
        public decimal Amount { get; set; }
        public decimal PriceAtTime { get; set; }
        public string Type { get; set; } = default!;
        public DateTime Date { get; set; }
    }
}
