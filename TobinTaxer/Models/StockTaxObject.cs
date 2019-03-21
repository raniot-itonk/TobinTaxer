using System;

namespace TobinTaxer.Models
{
    public class StockTaxObject
    {
        public Guid ReservationId { get; set; }
        public Guid Buyer { get; set; }
        public Guid Seller { get; set; }
        public long StockId { get; set; }
        public double Price { get; set; }
        public int Amount { get; set; }
    }
}