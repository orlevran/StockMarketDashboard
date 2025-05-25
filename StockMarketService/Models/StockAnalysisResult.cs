using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarketService.Models
{
    public class StockAnalysisResult
    {
        public string Symbol { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
    }
}
