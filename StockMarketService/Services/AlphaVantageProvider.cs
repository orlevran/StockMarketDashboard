using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StockMarketService.Models;

namespace StockMarketService.Services
{
    public class AlphaVantageProvider : IStockDataProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AlphaVantageProvider(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["AlphaVantage:ApiKey"];
        }

        public async Task<decimal?> GetClosingPriceAsync(string stockSymbol, DateTime date)
        {
            string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={stockSymbol}&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("Time Series (Daily)", out JsonElement timeSeries))
            {
                Console.WriteLine("Time Series (Daily) not found in JSON.");
                return null;
            }

            // Try exact match first
            string exactDate = date.ToString("yyyy-MM-dd");
            if (timeSeries.TryGetProperty(exactDate, out JsonElement exactDay) &&
                exactDay.TryGetProperty("4. close", out JsonElement exactClose))
            {
                return decimal.Parse(exactClose.GetString());
            }


            // Fallback: find closest earlier date
            var allDates = timeSeries.EnumerateObject()
                .Where(p => DateTime.TryParse(p.Name, out _))
                .Select(p => new
                {
                    Date = DateTime.Parse(p.Name),
                    DayData = p.Value
                })
                .Where(p => p.Date <= date)
                .OrderByDescending(p => p.Date)
                .ToList();

            var fallback = allDates.FirstOrDefault();
            if (fallback != null &&
                fallback.DayData.TryGetProperty("4. close", out JsonElement fallbackClose))
            {
                Console.WriteLine($"Falling back to previous trading day: {fallback.Date:yyyy-MM-dd}");
                return decimal.Parse(fallbackClose.GetString());
            }

            Console.WriteLine("No valid price data found.");
            return null;
        }


        public async Task<StockAnalysisResult> AnalyzeAsync(string symbol, DateTime purchaseDate, DateTime sellDate, decimal purchasePrice)
        {
            var sellPrice = await GetClosingPriceAsync(symbol, sellDate);
            if (sellPrice == null)
                throw new Exception("Could not fetch stock price");

            var change = sellPrice.Value - purchasePrice;
            var percentChange = (change / purchasePrice) * 100;

            return new StockAnalysisResult
            {
                Symbol = symbol,
                PurchasePrice = purchasePrice,
                SalePrice = sellPrice.Value,
                Change = change,
                ChangePercent = percentChange
            };
        }
    }
}
