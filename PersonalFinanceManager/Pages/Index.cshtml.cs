using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using test.Models;

namespace test.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        public List<Asset> MyAssets { get; set; } = new List<Asset>();
        public decimal TotalNetWorth { get; set; }

        [BindProperty]
        public string InputName { get; set; } = default!;
        [BindProperty]
        public decimal InputValue { get; set; } = default!;
        [BindProperty]
        public decimal InputPrice { get; set; }  = default!;
        [BindProperty]
        public string InputType { get; set; }  = default!;
        [BindProperty]
        public decimal SellAmount { get; set; }

        public void OnGet()
        {
            MyAssets = PortfolioService.Assets;

            TotalNetWorth = MyAssets.Sum(a => a.CalculateValue());
        }

        public IActionResult OnPost()
        {
            try
            {
                string upperName = InputName.ToUpper();

                if (InputType == "Cash")
                {
                    var existingCash = PortfolioService.Assets
                        .OfType<Cash>()
                        .FirstOrDefault(c => c.Name == InputName);

                    if (existingCash != null)
                    {
                        existingCash.Amount += InputValue;
                    }
                    else
                    {
                        var cash = new Cash(InputName, InputValue);
                        PortfolioService.Assets.Add(cash);

                    }
                    PortfolioService.Transactions.Add(new Transaction
                    {
                        AssetName = InputName,
                        Amount = InputValue,
                        Type = "Deposit",
                        Date = DateTime.Now
                    });
                }
                else if (InputType == "Crypto" || InputType == "Stock")
                {
                    decimal marketPrice = MarketsModel.GetLatestPrice(upperName);

                    if (marketPrice == 0)
                    {
                        MyAssets = PortfolioService.Assets;
                        TotalNetWorth = MyAssets.Sum(a => a.CalculateValue());

                        ModelState.AddModelError(string.Empty, $"???????? {upperName} ?? ? ???????!");
                        return Page();
                    }

                    decimal totalCost = InputValue * marketPrice;

                    // Find Trading Cash
                    var tradingCash = PortfolioService.Assets.OfType<Cash>()
                        .FirstOrDefault(c => c.Name == "Trading Cash");

                    if (tradingCash == null || tradingCash.Amount < totalCost)
                    {
                        MyAssets = PortfolioService.Assets;
                        TotalNetWorth = MyAssets.Sum(a => a.CalculateValue());

                        ModelState.AddModelError(string.Empty, $"Insufficient funds! Need ${totalCost:N2}, have ${tradingCash?.Amount ?? 0:N2}");
                        return Page();
                    }

                    // Deduct cash
                    tradingCash.Amount -= totalCost;

                    // Create the asset (use Stock for stocks, Crypto for crypto)
                    Asset asset;
                    if (InputType == "Stock")
                    {
                        var existingStock = PortfolioService.Assets
                            .OfType<Stock>()
                            .FirstOrDefault(s => s.Name == InputName);
                        if (existingStock != null)
                        {
                            // Update existing stock
                            decimal totalQuantity = existingStock.Quantity + InputValue;
                            decimal weightedPrice = ((existingStock.Price * existingStock.Quantity) + (marketPrice * InputValue)) / totalQuantity;
                            existingStock.Quantity = totalQuantity;
                            existingStock.Price = weightedPrice;
                            asset = existingStock;
                        }
                        else
                        {
                            // Create new stock

                            asset = new Stock(InputName, InputValue, marketPrice);
                            PortfolioService.Assets.Add(asset);
                        }
                    }
                    else // Crypto
                    {
                        var existingCryto = PortfolioService.Assets
                            .OfType<Crypto>()
                            .FirstOrDefault(s => s.Name == InputName);
                        if (existingCryto != null)
                        {
                            // Update existing stock
                            decimal totalQuantity = existingCryto.Quantity + InputValue;
                            decimal weightedPrice = ((existingCryto.Price * existingCryto.Quantity) + (marketPrice * InputValue)) / totalQuantity;
                            existingCryto.Quantity = totalQuantity;
                            existingCryto.Price = weightedPrice;
                            asset = existingCryto;
                        }
                        else
                        {
                            // Create new stock
                            asset = new Crypto(InputName, InputValue, marketPrice);
                            PortfolioService.Assets.Add(asset);
                        }
                    }


                    PortfolioService.Transactions.Add(new Transaction
                    {
                        AssetName = InputName,
                        Amount = InputValue,
                        PriceAtTime = marketPrice,
                        Type = InputType + " Buy",
                        Date = DateTime.Now
                    });
                }

                return RedirectToPage();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, $"Validation error: {ex.Message}");
                MyAssets = PortfolioService.Assets;
                TotalNetWorth = MyAssets.Sum(a => a.CalculateValue());
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                MyAssets = PortfolioService.Assets;
                TotalNetWorth = MyAssets.Sum(a => a.CalculateValue());
                return Page();
            }
        }

        public IActionResult OnPostDelete(string assetName)
        {
            var assetToRemove = PortfolioService.Assets.FirstOrDefault(a => a.Name == assetName);

            if (assetToRemove != null)
            {
                PortfolioService.Assets.Remove(assetToRemove);

                Transaction transaction = new Transaction
                {
                    AssetName = assetToRemove.Name,
                    Type = "Asset Removed",
                    Date = DateTime.Now
                };

                if (assetToRemove is Crypto cryptoAsset)
                {
                    transaction.Amount = cryptoAsset.Quantity;
                    transaction.PriceAtTime = cryptoAsset.Price;
                }
                else if (assetToRemove is Stock stockAsset)
                {
                    transaction.Amount = stockAsset.Quantity;
                    transaction.PriceAtTime = stockAsset.Price;
                }
                else if (assetToRemove is Cash cashAsset)
                {
                    transaction.Amount = cashAsset.Amount;
                    transaction.PriceAtTime = 1; // Cash price is always 1
                }
                PortfolioService.Transactions.Add(transaction);
            }
            return RedirectToPage();
        }

        public IActionResult OnPostSellPartial(string assetName, decimal sellAmount)
        {
            try
            {
                var asset = PortfolioService.Assets.FirstOrDefault(a => a.Name == assetName);
                if (asset == null) return RedirectToPage();

                // Handle Cash
                if (asset is Cash cash)
                {
                    if (sellAmount > cash.Amount)
                    {
                        ModelState.AddModelError(string.Empty, $"Insufficient cash! You have ${cash.Amount:N2}, trying to withdraw ${sellAmount:N2}");
                        MyAssets = PortfolioService.Assets;
                        TotalNetWorth = MyAssets.Sum(a => a.CalculateValue());
                        return Page();
                    }

                    cash.Amount -= sellAmount;

                    PortfolioService.Transactions.Add(new Transaction
                    {
                        AssetName = asset.Name,
                        Amount = sellAmount,
                        PriceAtTime = 1,
                        Type = "Cash Withdrawal",
                        Date = DateTime.Now
                    });

                    // Remove if balance is zero or very close to zero
                    if (cash.Amount < 0.01m)
                    {
                        PortfolioService.Assets.Remove(cash);
                    }

                    return RedirectToPage();
                }

                // Handle Crypto and Stock
                decimal currentPrice = MarketsModel.GetLatestPrice(asset.Name.ToUpper());

                if (currentPrice == 0)
                {
                    ModelState.AddModelError(string.Empty, $"Cannot get price for {asset.Name}");
                    MyAssets = PortfolioService.Assets;
                    TotalNetWorth = MyAssets.Sum(a => a.CalculateValue());
                    return Page();
                }

                decimal quantity = 0;
                decimal originalPrice = 0;
                decimal totalQuantity = 0;

                if (asset is Crypto crypto)
                {
                    totalQuantity = crypto.Quantity;
                    originalPrice = crypto.Price;

                    if (sellAmount > totalQuantity)
                    {
                        ModelState.AddModelError(string.Empty, $"Insufficient quantity! You have {totalQuantity:F2}, trying to sell {sellAmount:F2}");
                        MyAssets = PortfolioService.Assets;
                        TotalNetWorth = MyAssets.Sum(a => a.CalculateValue());
                        return Page();
                    }

                    quantity = sellAmount;
                    crypto.Quantity -= sellAmount;
                }
                else if (asset is Stock stock)
                {
                    totalQuantity = stock.Quantity;
                    originalPrice = stock.Price;

                    if (sellAmount > totalQuantity)
                    {
                        ModelState.AddModelError(string.Empty, $"Insufficient quantity! You have {totalQuantity:F2}, trying to sell {sellAmount:F2}");
                        MyAssets = PortfolioService.Assets;
                        TotalNetWorth = MyAssets.Sum(a => a.CalculateValue());
                        return Page();
                    }

                    quantity = sellAmount;
                    stock.Quantity -= sellAmount;
                }
                else
                {
                    return RedirectToPage();
                }

                decimal originalInvestment = originalPrice * quantity;
                decimal currentValue = currentPrice * quantity;
                decimal profitLoss = currentValue - originalInvestment;

                // Find Trading Cash
                var tradingCash = PortfolioService.Assets.OfType<Cash>()
                    .FirstOrDefault(c => c.Name == "Trading Cash");

                if (tradingCash != null)
                {
                    tradingCash.Amount += currentValue;
                }
                else
                {
                    PortfolioService.Assets.Add(new Cash("Trading Cash", currentValue));
                }

                PortfolioService.Transactions.Add(new Transaction
                {
                    AssetName = asset.Name,
                    Amount = quantity,
                    PriceAtTime = currentPrice,
                    Type = profitLoss >= 0 ? "Sell (Profit)" : "Sell (Loss)",
                    Date = DateTime.Now
                });

                // Remove asset if quantity is zero or very close to zero
                if (asset is Crypto c && c.Quantity < 0.0001m)
                {
                    PortfolioService.Assets.Remove(asset);
                }
                else if (asset is Stock s && s.Quantity < 0.0001m)
                {
                    PortfolioService.Assets.Remove(asset);
                }

                return RedirectToPage();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, $"Validation error: {ex.Message}");
                MyAssets = PortfolioService.Assets;
                TotalNetWorth = MyAssets.Sum(a => a.CalculateValue());
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                MyAssets = PortfolioService.Assets;
                TotalNetWorth = MyAssets.Sum(a => a.CalculateValue());
                return Page();
            }
        }
        public JsonResult OnGetValidateSymbol(string symbol, string type)
        {
            string[] cryptos = { "BTC", "ETH", "SOL", "BNB", "XRP", "DOGE", "ADA" };
            bool isCrypto = cryptos.Contains(symbol.ToUpper());

            if (type == "Crypto" && !isCrypto)
            {
                return new JsonResult(new { valid = false, message = $"{symbol} е акция, не криптовалута!" });
            }

            if (type == "Stock" && isCrypto)
            {
                return new JsonResult(new { valid = false, message = $"{symbol} е криптовалута, не акция!" });
            }

            return new JsonResult(new { valid = true });
        }
    }
}
