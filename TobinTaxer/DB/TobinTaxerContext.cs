using System;
using Microsoft.EntityFrameworkCore;

namespace TobinTaxer.DB
{
    public class TobinTaxerContext : DbContext
    {
        public TobinTaxerContext(DbContextOptions<TobinTaxerContext> options)
            : base(options)
        {
        }
        public DbSet<TaxHistory> TaxHistories { get; set; }
    }

    public class TaxHistory
    {
        public long Id { get; set; }
        public Guid Buyer { get; set; }
        public Guid Seller { get; set; }
        public string StockName { get; set; }
        public double Price { get; set; }
        public int Amount { get; set; }
        public double TaxRate { get; set; }
        public double Tax { get; set; }
    }
}
