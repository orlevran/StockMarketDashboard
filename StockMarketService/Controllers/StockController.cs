﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StockMarketService.Services;

namespace StockMarketService.Controllers
{
    [ApiController]
    [Route("api/stocks")]
    public class StockController : ControllerBase
    {
        private readonly IStockDataProvider _provider;
        private readonly StockAnalyzer _analyzer = new StockAnalyzer();

        public StockController(IStockDataProvider provider)
        {
            _provider = provider;
        }

        [HttpGet("analyze")]
        public async Task<IActionResult> AnalyzeStock(string symbol, DateTime purchaseDate, DateTime sellDate)
        {
            try
            {
                // NOTE: AlphaVantageProvider implements AnalyzeAsync, but IStockDataProvider does not.
                if (_provider is not AlphaVantageProvider realProvider)
                    return StatusCode(500, "Invalid provider instance.");

                var result = await _analyzer.AnalyzeAsync(_provider, symbol, purchaseDate, sellDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
