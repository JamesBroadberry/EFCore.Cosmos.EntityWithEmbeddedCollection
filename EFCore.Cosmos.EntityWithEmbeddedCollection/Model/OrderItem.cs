using System;

namespace EFCore.Cosmos.EntityWithEmbeddedCollection.Model
{
    public record OrderItem
    {
        public string ItemName { get; set; } = string.Empty;
        public decimal ItemPrice { get; set; } = 0;
    }
}