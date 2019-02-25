using System;

namespace TobinTaxer.Models
{
    public class IssueObject
    {
        public int Amount { get; set; }
        public Guid Owner { get; set; }
    }
}