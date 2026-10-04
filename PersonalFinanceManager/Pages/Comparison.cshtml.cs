using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace test.Pages
{
    public class ComparisonModel : PageModel
    {
        [BindProperty]
        public string Symbol1 { get; set; } = default!;

        [BindProperty]
        public string Symbol2 { get; set; } = default!;

        [BindProperty]
        public string ChartType { get; set; } = "absolute";

        public List<string> AvailableSymbols { get; set; } = new List<string>();
        public ComparisonData? ComparisonResult { get; set; }

        public void OnGet()
        {
            LoadAvailableSymbols();
        }

        public IActionResult OnPost()
        {
            LoadAvailableSymbols();

            if (string.IsNullOrEmpty(Symbol1) || string.IsNullOrEmpty(Symbol2))
            {
                ModelState.AddModelError(string.Empty, "???? ???????? ??? ??????? ?? ?????????");
                return Page();
            }

            Symbol1 = Symbol1.ToUpper();
            Symbol2 = Symbol2.ToUpper();

            ComparisonResult = GenerateComparison(Symbol1, Symbol2, ChartType);

            if (ComparisonResult == null)
            {
                ModelState.AddModelError(string.Empty, "?????? ??? ????????? ?? ???????");
            }

            return Page();
        }

        private void LoadAvailableSymbols()
        {
            AvailableSymbols = MarketsModel.GetAllSymbols();
        }

        private ComparisonData? GenerateComparison(string symbol1, string symbol2, string chartType)
        {
            var data1 = MarketsModel.GetSymbolData(symbol1);
            var data2 = MarketsModel.GetSymbolData(symbol2);

            if (data1 == null || data2 == null)
                return null;

            var comparison = new ComparisonData
            {
                Symbol1 = symbol1,
                Symbol2 = symbol2,
                Symbol1Current = data1.CurrentPrice,
                Symbol2Current = data2.CurrentPrice,
                Symbol1Change = data1.ChangePercent,
                Symbol2Change = data2.ChangePercent,
                Symbol1Initial = data1.InitialPrice,
                Symbol2Initial = data2.InitialPrice,
                Labels = data1.Labels,
                ChartType = chartType
            };

            if (chartType == "percent")
            {
                // Normalized to percentage change
                comparison.Symbol1Data = data1.History
                    .Select(price => ((price - data1.InitialPrice) / data1.InitialPrice * 100))
                    .ToList();
                comparison.Symbol2Data = data2.History
                    .Select(price => ((price - data2.InitialPrice) / data2.InitialPrice * 100))
                    .ToList();
            }
            else
            {
                // Absolute prices
                comparison.Symbol1Data = data1.History;
                comparison.Symbol2Data = data2.History;
            }

            return comparison;
        }
        public JsonResult OnGetRefreshComparison(string symbol1, string symbol2, string chartType)
        {
            symbol1 = symbol1.ToUpper();
            symbol2 = symbol2.ToUpper();

            var comparison = GenerateComparison(symbol1, symbol2, chartType);

            return new JsonResult(comparison);
        }
    }

    public class ComparisonData
    {
        public string Symbol1 { get; set; } = default!;
        public string Symbol2 { get; set; } = default!;
        public decimal Symbol1Current { get; set; }
        public decimal Symbol2Current { get; set; }
        public decimal Symbol1Change { get; set; }
        public decimal Symbol2Change { get; set; }
        public decimal Symbol1Initial { get; set; }
        public decimal Symbol2Initial { get; set; }
        public List<decimal> Symbol1Data { get; set; } = new List<decimal>();
        public List<decimal> Symbol2Data { get; set; } = new List<decimal>();
        public List<string> Labels { get; set; } = new List<string>();
        public string ChartType { get; set; } = "absolute";
    }

    public class SymbolData
    {
        public decimal CurrentPrice { get; set; }
        public decimal InitialPrice { get; set; }
        public decimal ChangePercent { get; set; }
        public List<decimal> History { get; set; } = new List<decimal>();
        public List<string> Labels { get; set; } = new List<string>();
    }
}