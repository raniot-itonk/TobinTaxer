using System;

namespace TobinTaxer.Models
{
    public class StockTaxObject
    {
        public Guid ReservationId { get; set; }
        public Guid Buyer { get; set; }
        public Guid Seller { get; set; }
        public string StockName { get; set; }
        public double Price { get; set; }
        public int Amount { get; set; }
    }
}