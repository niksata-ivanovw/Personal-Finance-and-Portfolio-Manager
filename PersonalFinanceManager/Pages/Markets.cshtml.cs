using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace test.Pages;

public class MarketsModel : PageModel
{
    //static lists -> data in server memory
    private static readonly Dictionary<string, decimal> __basePrices = new Dictionary<string, decimal>
    {
    { "SPY", 590.50m },
    { "VT", 145.08m },
    { "NVDA", 184.84m},
    { "AAPL", 230.15m },
    { "TSLA", 449.36m },
    { "AMZN", 444.11m },
    { "MSTR", 159.96m },
    { "MU", 397.58m },
    { "NFLX", 83.54m },
    { "BNB", 901.21m },
    { "SOL", 128.18m },
    { "XRP", 1.92m },
    { "DOGE", 0.13m },
    { "ADA", 0.36m },
    { "ETH", 3250.00m },
    { "BTC", 102450.00m },
    
    };

    private static Dictionary<string, List<decimal>> __allPriceHistories = new Dictionary<string, List<decimal>>();
    private static Dictionary<string, List<string>> __allTimeLabels = new Dictionary<string, List<string>>();
    private static readonly Random _rng = new Random();
    private static readonly object _lock = new object(); //prevent concurrency

    public void OnGet()
    {

    }

    public JsonResult OnGetAvailableStocks()
    {
        return new JsonResult(__basePrices.Keys.ToList());
    }
    public bool IsCrypto(string symbol)
    {
        string[] cryptos = { "BTC", "ETH", "SOL", "BNB", "XRP", "DOGE", "ADA" };
        return cryptos.Contains(symbol);
    }

    public JsonResult OnGetStockPrice(string symbol, bool isRefresh = true)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(symbol) || !__basePrices.ContainsKey(symbol))
            {
                return new JsonResult(new { current = "0.00", error = "Invalid Symbol" });
            }

            if (!__allPriceHistories.ContainsKey(symbol))
            {
                __allPriceHistories[symbol] = new List<decimal>();
            }

            if (!__allTimeLabels.ContainsKey(symbol))
            {
                __allTimeLabels[symbol] = new List<string>();
            }

            decimal initialPrice = __basePrices[symbol];

            if (isRefresh || __allPriceHistories.Count == 0)
            {
                decimal lastPrice = __allPriceHistories[symbol].Count > 0
                                ? __allPriceHistories[symbol].Last()
                                : initialPrice;

                decimal currentPrice = CalculateCrntPrice(lastPrice);
                __allPriceHistories[symbol].Add(currentPrice);


                if (__allPriceHistories[symbol].Count % 12 == 1 || __allPriceHistories[symbol].Count == 1)
                {
                    __allTimeLabels[symbol].Add(DateTime.Now.ToString("HH:mm"));
                }
                else
                {
                    __allTimeLabels[symbol].Add(""); //empty label for better readability
                }
            }

            decimal latestPrice = __allPriceHistories[symbol].Last();
            decimal totalChange = ((latestPrice - initialPrice) / initialPrice * 100);

            // return data as Json
            return new JsonResult(new
            {
                current = latestPrice.ToString("N2"),
                history = __allPriceHistories[symbol],
                labels = __allTimeLabels[symbol],
                change = totalChange.ToString("F2"),
                minPrice = __allPriceHistories[symbol].Min().ToString("N2"),
                maxPrice = __allPriceHistories[symbol].Max().ToString("N2"),
                avgPrice = __allPriceHistories[symbol].Average().ToString("N2")
            });
        }
    }

    public static decimal CalculateCrntPrice(decimal lastPrice) //random walk price algorithm
    {
        double volatility = 0.02;
        double randomShift = _rng.NextDouble() * 2 - 1;

        decimal changePercent = (decimal)(randomShift * volatility);
        decimal currentPrice = lastPrice * (1 + changePercent);

        if (_rng.Next(1, 101) > 95)
        {
            decimal shock = (decimal)(_rng.NextDouble() * 0.10 - 0.05);
            currentPrice *= (1 + shock);
        }

        return currentPrice;
    }
    public static void UpdateAllPrices()
    {
        lock (_lock)
        {
            foreach (var symbol in __basePrices.Keys)
            {
                if (!__allPriceHistories.ContainsKey(symbol))
                {
                    __allPriceHistories[symbol] = new List<decimal>();
                }

                decimal lastPrice = __allPriceHistories[symbol].Count > 0
                                ? __allPriceHistories[symbol].Last()
                                : __basePrices[symbol];

                decimal currentPrice = CalculateCrntPrice(lastPrice);

                __allPriceHistories[symbol].Add(currentPrice);

                if (!__allTimeLabels.ContainsKey(symbol))
                    __allTimeLabels[symbol] = new List<string>();

                if (__allPriceHistories[symbol].Count % 12 == 1 || __allPriceHistories[symbol].Count == 1)
                    __allTimeLabels[symbol].Add(DateTime.Now.ToString("HH:mm"));
                else
                    __allTimeLabels[symbol].Add("");
            }
        }
    }
    public static decimal GetLatestPrice(string symbol)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(symbol) || !__basePrices.ContainsKey(symbol))
                return 0m;

            if (!__allPriceHistories.ContainsKey(symbol) || __allPriceHistories[symbol].Count == 0)
            {
                return __basePrices[symbol];
            }

            return __allPriceHistories[symbol].Last();
        }
    }
    public static List<string> GetAllSymbols()
    {
        return __basePrices.Keys.ToList();
    }

    public static SymbolData? GetSymbolData(string symbol)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(symbol) || !__basePrices.ContainsKey(symbol))
                return null;

            if (!__allPriceHistories.ContainsKey(symbol) || __allPriceHistories[symbol].Count == 0)
            {
                // Initialize if not exists
                __allPriceHistories[symbol] = new List<decimal> { __basePrices[symbol] };
                __allTimeLabels[symbol] = new List<string> { DateTime.Now.ToString("HH:mm") };
            }

            decimal initialPrice = __basePrices[symbol];
            decimal latestPrice = __allPriceHistories[symbol].Last();
            decimal changePercent = ((latestPrice - initialPrice) / initialPrice * 100);

            return new SymbolData
            {
                CurrentPrice = latestPrice,
                InitialPrice = initialPrice,
                ChangePercent = changePercent,
                History = __allPriceHistories[symbol].ToList(),
                Labels = __allTimeLabels[symbol].ToList()
            };
        }
    }
}