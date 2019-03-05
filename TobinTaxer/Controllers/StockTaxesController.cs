using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TobinTaxer.DB;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Polly;
using TobinTaxer.Models;
using TobinTaxer.OptionModels;

namespace TobinTaxer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockTaxesController : ControllerBase
    {
        private readonly TobinTaxerContext _context;
        private readonly ILogger<StockTaxesController> _logger;
        private readonly Services _services;
        private readonly TaxInfo _taxInfo;

        public StockTaxesController(TobinTaxerContext context, ILogger<StockTaxesController> logger, IOptionsMonitor<Services> servicesOptions, IOptionsMonitor<TaxInfo> taxOptions)
        {
            _context = context;
            _logger = logger;
            _services = servicesOptions.CurrentValue;
            _taxInfo = taxOptions.CurrentValue;
        }

        // Post Stock Tax
        //[Authorize("BankingService.UserActions")]
        [HttpPost]
        public async Task<ActionResult<TaxHistory>> PostStockTax(StockTaxObject stockTaxObject)
        {
            // Call Bank service
            var tax = stockTaxObject.Price * stockTaxObject.Amount * _taxInfo.TaxRate;

            var url = _services.BankService.BaseAddress + "api/transfer";
            var stateAccount = Guid.Parse("7bedb953-4e7e-45f9-91de-ffc0175be744");
            var transferObject = new TransferObject { Amount = tax, FromAccountId = stockTaxObject.Buyer, ReservationId = stockTaxObject.ReservationId, ToAccountId = stateAccount };
            try
            {
                var response = await Policy.Handle<FlurlHttpException>()
                    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(3)
                    }).ExecuteAsync(() => url.WithTimeout(10).PutJsonAsync(transferObject));

                if (!response.IsSuccessStatusCode) return BadRequest("Failed to Tax trade");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            

            // Store in own database
            var taxHistory = new TaxHistory
            {
                Amount = stockTaxObject.Amount,
                Buyer = stockTaxObject.Buyer,
                Price = stockTaxObject.Price,
                Seller = stockTaxObject.Seller,
                StockName = stockTaxObject.StockName,
                TaxRate = _taxInfo.TaxRate,
                Tax = tax 
            };
            await _context.TaxHistories.AddAsync(taxHistory);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Exacted {Tax in taxes from from {User}", tax, stockTaxObject.Buyer);
            _logger.LogInformation("Logged TaxInfo to database {TaxHistory}", taxHistory);
            return taxHistory;
        }
    }
}