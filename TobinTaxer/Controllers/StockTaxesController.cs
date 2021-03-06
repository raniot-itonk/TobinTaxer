﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TobinTaxer.DB;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Polly;
using Prometheus;
using TobinTaxer.Clients;
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
        private readonly IRabbitMqClient _rabbitMqClient;
        private readonly Services _services;
        private readonly TaxInfo _taxInfo;

        private static readonly Counter TaxPaid = Metrics
            .CreateCounter("TaxPaid", "Total amount of tax paid");


        public StockTaxesController(TobinTaxerContext context, ILogger<StockTaxesController> logger, IOptionsMonitor<Services> servicesOptions, IOptionsMonitor<TaxInfo> taxOptions, IRabbitMqClient rabbitMqClient)
        {
            _context = context;
            _logger = logger;
            _rabbitMqClient = rabbitMqClient;
            _services = servicesOptions.CurrentValue;
            _taxInfo = taxOptions.CurrentValue;
        }

        // Post Stock Tax
        //[Authorize("BankingService.UserActions")]
        [HttpPost]
        public async Task<ActionResult<ValidationResult>> PostStockTax(StockTaxObject stockTaxObject)
        {
            try
            {
                // Call Bank service
                var tax = stockTaxObject.Price * stockTaxObject.Amount * _taxInfo.TaxRate;

                var url = _services.BankService.BaseAddress + "api/transfer";
                var stateAccount = Guid.Parse("7bedb953-4e7e-45f9-91de-ffc0175be744");
                var transferObject = new TransferObject { Amount = tax, FromAccountId = stockTaxObject.Buyer, ReservationId = stockTaxObject.ReservationId, ToAccountId = stateAccount };

                var validationResult = await Policy.Handle<FlurlHttpException>()
                    .WaitAndRetryAsync(new[]
                    {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                    }).ExecuteAsync(() => url.WithTimeout(10).PutJsonAsync(transferObject).ReceiveJson<ValidationResult>());
                if(!validationResult.Valid) _logger.LogWarning("Failed to Tax Trade, with error {ErrorMessage}", validationResult.ErrorMessage);

                // Store in own database
                var taxHistory = new TaxHistory
                {
                    Amount = stockTaxObject.Amount,
                    Buyer = stockTaxObject.Buyer,
                    Price = stockTaxObject.Price,
                    Seller = stockTaxObject.Seller,
                    StockId = stockTaxObject.StockId,
                    TaxRate = _taxInfo.TaxRate,
                    Tax = tax
                };
                TaxPaid.Inc(tax);
                await _context.TaxHistories.AddAsync(taxHistory);
                await _context.SaveChangesAsync();

                _rabbitMqClient.SendMessage(new HistoryMessage { Event = "PaidTax", EventMessage = $"Paid ${tax} tax for buying shares", User = stockTaxObject.Buyer, Timestamp = DateTime.UtcNow });

                _logger.LogInformation("Exacted {Tax} in taxes from from {User}", tax, stockTaxObject.Buyer);
                _logger.LogInformation("Logged TaxInfo to database {TaxHistory}", taxHistory);
                return validationResult;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to do stuff");
                Console.WriteLine(e);
                throw;
            }

        }
    }
}