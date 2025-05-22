using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMarketService.Models;

namespace StockMarketService.Services
{
    public interface IStockDataProvider
    {
        Task<decimal?> GetClosingPriceAsync(string stockSymbol, DateTime date);
        Task<StockAnalysisResult> AnalyzeAsync(string symbol, DateTime purchaseDate, DateTime sellDate, decimal purchasePrice);
    }
}
