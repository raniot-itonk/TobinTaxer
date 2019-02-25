using System.Collections.ObjectModel;

namespace TobinTaxer.DB
{
    public class Stock
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double LastTradedValue { get; set; }
        public Collection<Shareholder> ShareHolders { get; set; }
    }
}