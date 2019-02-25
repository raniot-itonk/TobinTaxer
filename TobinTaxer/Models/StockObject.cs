using System.Collections.ObjectModel;
using TobinTaxer.DB;

namespace TobinTaxer.Models
{
    public class StockObject
    {
        public string Name { get; set; }
        public Collection<Shareholder> Shares { get; set; }
    }
}