using System;

namespace TobinTaxer.Models
{
    public class OwnershipObject
    {
        public Guid Seller { get; set; }
        public Guid Buyer { get; set; }
        public int Amount { get; set; }
    }
}