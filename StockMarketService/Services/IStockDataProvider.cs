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
        Task<Tuple<decimal, decimal>?> GetValuesByDates(string stockSymbol, DateTime purchase_date, DateTime sell_date);
    }
}
