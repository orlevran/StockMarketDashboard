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

        public async Task<Tuple<decimal, decimal>?> GetValuesByDates(string stockSymbol, DateTime purchase_date, DateTime sell_date)
        {
            // returns the sell price
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

            decimal purchasePrice = 0;
            decimal sellPrice = 0;

            // Try exact match first
            string exactDate = purchase_date.ToString("yyyy-MM-dd");
            if(timeSeries.TryGetProperty(exactDate, out JsonElement exactPurchaseDay) &&
                exactPurchaseDay.TryGetProperty("4. close", out JsonElement exactPurchaseClose))
            {
                purchasePrice = decimal.Parse(exactPurchaseClose.GetString());
            }

            exactDate = sell_date.ToString("yyyy-MM-dd");
            if (timeSeries.TryGetProperty(exactDate, out JsonElement exactSellDay) &&
                exactSellDay.TryGetProperty("4. close", out JsonElement exactSellClose))
            {
                sellPrice = decimal.Parse(exactSellClose.GetString());
                //return decimal.Parse(exactClose.GetString());
            }

            Tuple<decimal, decimal>? result = new Tuple<decimal, decimal>(purchasePrice, sellPrice);
            return result;
            /*

            // Fallback: find closest earlier date
            var allDates = timeSeries.EnumerateObject()
                .Where(p => DateTime.TryParse(p.Name, out _))
                .Select(p => new
                {
                    Date = DateTime.Parse(p.Name),
                    DayData = p.Value
                })
                .Where(p => p.Date <= sell_date)
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
            */
        }
    }
}
