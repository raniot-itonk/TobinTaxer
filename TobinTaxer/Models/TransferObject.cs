using System;

namespace TobinTaxer.Models
{
    public class TransferObject
    {
        public Guid FromAccountId { get; set; }
        public Guid ReservationId { get; set; }
        public Guid ToAccountId { get; set; }
        public double Amount { get; set; }
    }
}