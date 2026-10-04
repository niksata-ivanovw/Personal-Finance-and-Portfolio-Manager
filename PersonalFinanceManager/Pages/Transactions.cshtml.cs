using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using test.Models;

namespace test.Pages
{
    public class TransactionsModel : PageModel
    {
        public List<Transaction> TransactionHistory { get; set; } = new List<Transaction>();
        public void OnGet()
        {
            TransactionHistory = PortfolioService.Transactions.OrderByDescending(t => t.Date).ToList();
        }
        public string GetTransactionBadgeClass(string type)
        {
            return type switch
            {
                "Sell (Profit)" => "badge bg-success",
                "Sell (Loss)" => "badge bg-danger",
                "Stock Buy" => "badge bg-primary",
                "Crypto Buy" => "badge bg-info",
                "Deposit" => "badge bg-secondary",
                "Initial Deposit" => "badge bg-dark",
                "Asset Removed" => "badge bg-warning text-dark",
                _ => "badge bg-secondary"
            };
        }
    }
}
