using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMarketService.Models;

namespace StockMarketService.Services
{
    public class StockAnalyzer
    {
        public async Task<StockAnalysisResult> AnalyzeAsync(IStockDataProvider provider, string symbol, DateTime purchaseDate, DateTime sellDate)
        {
            // Fetch the yield data
            var result = await provider.GetValuesByDates(symbol, purchaseDate, sellDate);
            if (result == null)
                throw new Exception("Could not fetch stock price");

            // Calculate change and percent change
            if (result.Item2 == 0)
                throw new Exception("Couldn't find sell price. Please try again later or try using different provider");
            if (result.Item1 == 0)
                throw new Exception("Couldn't find Purchase price. Please try again later or try using different provider");
            var yield = result.Item2 / result.Item1;
            var change = result.Item2 - result.Item1;
            var changePercent = Math.Round(yield * 100, 5);

            return new StockAnalysisResult
            {
                Symbol = symbol,
                PurchasePrice = result.Item1,
                SalePrice = result.Item2,
                Change = change,
                ChangePercent = changePercent
            };
        }
    }
}
