using System;
using System.Collections.Generic;

namespace EFCore.Cosmos.EntityWithEmbeddedCollection.Model
{
    public record Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CustomerName { get; set; } = string.Empty;
        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();
    }
}