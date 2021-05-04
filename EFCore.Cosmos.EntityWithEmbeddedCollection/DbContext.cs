using EFCore.Cosmos.EntityWithEmbeddedCollection.Model;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Cosmos.EntityWithEmbeddedCollection
{
    public class CosmosStoreDbContext : DbContext
    {
        private const string CONTAINER_NAME = "CosmosStoreDbContext";

        public CosmosStoreDbContext(DbContextOptions<CosmosStoreDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultContainer(CONTAINER_NAME);

            modelBuilder.Entity<Order>().HasKey(x => x.Id);
            modelBuilder.Entity<Order>().OwnsMany(x => x.Items).HasKey(x => x.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}