using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TobinTaxer.DB;
using TobinTaxer.Models;

namespace TobinTaxer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly PublicShareOwnerContext _context;
        private readonly ILogger<StocksController> _logger;

        public StocksController(PublicShareOwnerContext context, ILogger<StocksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get single Stock information
        //[Authorize("BankingService.UserActions")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Stock>> GetStock(long id)
        {
            
            var stock = await _context.Stocks.Include(s => s.ShareHolders).FirstOrDefaultAsync(s2 => s2.Id == id);

            if (stock == null)
            {
                return NotFound();
            }
            _logger.LogInformation("Got {Stock}", stock);
            return stock;
        }

        // Get all Stock or get all stock where owner is equal to userIdGuid
        //[Authorize("BankingService.UserActions")]
        [HttpGet]
        public async Task<ActionResult<List<Stock>>> GetStocks([FromQuery] string userIdGuid = null)
        {
            List<Stock> stocks;
            if (userIdGuid == null)
            {
                stocks = await _context.Stocks.ToListAsync();
                _logger.LogInformation("Got list of all stocks");
            }
            else
            {
                //TODO validate that Id is same as in Header

                stocks = await _context.Stocks
                    .Include(stock => stock.ShareHolders)
                    .Where(s => s.ShareHolders.Any(q => q.Id == Guid.Parse(userIdGuid))).ToListAsync();
                _logger.LogInformation("Got list of all stocks with owner {UserId}", userIdGuid);
            }

            if (!stocks.Any())
            {
                return NotFound();
            }
            
            return stocks;
        }

        // Add Stock
        //[Authorize("BankingService.UserActions")]
        [HttpPost]
        public async Task<ActionResult<Stock>> PostStock(StockObject stockObject)
        {
            var stock = new Stock
            {
                LastTradedValue = 0,
                Name = stockObject.Name,
                ShareHolders = stockObject.Shares
            };
            await _context.Stocks.AddAsync(stock);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added Stock {Stock}", stock);
            return stock;
        }

        // Issue more Shares to existing stock
        //[Authorize("BankingService.UserActions")]
        [HttpPut("{id}/issue")]
        public async Task<ActionResult> IssueShares([FromRoute] long id,[FromBody] IssueObject issueObject)
        {
            var stock = await _context.Stocks.Where(x => x.Id == id)
                .Include(s => s.ShareHolders)
                .Where(s => s.ShareHolders.Any(q => q.Id == issueObject.Owner)).FirstOrDefaultAsync();
            stock = await AddShareholder(id, issueObject, stock);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<Stock> AddShareholder(long id, IssueObject issueObject, Stock stock)
        {
            if (stock == null)
            {
                stock = await _context.Stocks.Include(x => x.ShareHolders).Where(x => x.Id == id).FirstOrDefaultAsync();
                stock.ShareHolders.Add(new Shareholder { Id = issueObject.Owner, Amount = issueObject.Amount });
            }
            else
            {
                var shareHolder = stock.ShareHolders.FirstOrDefault(sh => sh.Id == issueObject.Owner);
                if (shareHolder != null) shareHolder.Amount += issueObject.Amount;
            }

            return stock;
        }

        // Change ownership of existing shares
        //[Authorize("BankingService.UserActions")]
        [HttpPut("{id}/ownership")]
        public async Task<ActionResult> ChangeOwnership([FromRoute] long id, [FromBody] OwnershipObject ownershipObject)
        {
            var stock = await _context.Stocks.Where(x => x.Id == id)
                .Include(s => s.ShareHolders)
                .Where(s => s.ShareHolders.Any(q => q.Id == ownershipObject.Seller)).FirstOrDefaultAsync();
            if (stock == null)
            {
                _logger.LogError("Failed to find the Seller");
                return NotFound("Failed to find the Seller");
            }

            var actionResult = SetSellerAmount(ownershipObject, stock);
            if (actionResult != null) return actionResult;

            SetBuyerAmount(ownershipObject, stock);

            _logger.LogInformation("Changed ownership of {Amount} shares from {Seller} to {Buyer}", ownershipObject.Amount, ownershipObject.Seller, ownershipObject.Buyer);

            await _context.SaveChangesAsync();

            return Ok();
        }

        // Change ownership of existing shares
        //[Authorize("BankingService.UserActions")]
        [HttpPut("{id}/LastTradedValue/{value}")]
        public async Task<ActionResult> UpdateLastTradedValue([FromRoute] long id, [FromRoute] double value)
        {
            var stock = await _context.Stocks.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (stock == null)
            {
                _logger.LogError("Failed to find the Stock");
                return NotFound("Failed to find the Stock");
            }

            var oldLastTradedValue = stock.LastTradedValue;
            stock.LastTradedValue = value;

            _logger.LogInformation("Updated the last traded value of stock {Stock} from {oldValue} to {NewValue}", stock.Name, oldLastTradedValue, stock.LastTradedValue);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private static void SetBuyerAmount(OwnershipObject ownershipObject, Stock stock)
        {
            var shareHolderBuyer = stock.ShareHolders.FirstOrDefault(sh => sh.Id == ownershipObject.Buyer);
            if (shareHolderBuyer == null)
            {
                shareHolderBuyer = new Shareholder { Id = ownershipObject.Buyer, Amount = ownershipObject.Amount };
                stock.ShareHolders.Add(shareHolderBuyer);
            }
            else
            {
                shareHolderBuyer.Amount += ownershipObject.Amount;
            }
        }

        private ActionResult SetSellerAmount(OwnershipObject ownershipObject, Stock stock)
        {
            var shareHolderSeller = stock.ShareHolders.FirstOrDefault(sh => sh.Id == ownershipObject.Seller);
            if (shareHolderSeller == null)
            {
                _logger.LogError("Failed to find the Seller");
                {
                    return NotFound("Failed to find the Seller");
                }
            }

            shareHolderSeller.Amount -= ownershipObject.Amount;
            if (shareHolderSeller.Amount < 0)
            {
                _logger.LogError("Seller cannot go below 0 shares");
                return BadRequest("Seller cannot go below 0 shares");
            }

            return null;
        }
    }
}
