using System;
using System.Threading.Tasks;
using EFCore.Cosmos.EntityWithEmbeddedCollection.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Cosmos.EntityWithEmbeddedCollection
{
    class Program
    {
        public static async Task Main()
        {
            // Emulator connection string - not secret (same for all emulators)
            const string connectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            const string databaseName = "ExampleScenarioForEntityWithEmbeddedCollection";
            
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<CosmosStoreDbContext>((_, builder) =>
                builder.UseCosmos(connectionString, databaseName));

            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            var dbContext = serviceProvider.GetRequiredService<CosmosStoreDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            dbContext.RemoveRange(dbContext.Set<Order>());

            var bobOrder = await AddOrder(dbContext, "Bob");
            await AddOrderItem(dbContext, bobOrder.Id, new OrderItem()
            {
                ItemName = "Foo",
                ItemPrice = 10
            });
            await AddOrderItem(dbContext, bobOrder.Id, new OrderItem()
            {
                ItemName = "Bar",
                ItemPrice = 20
            });

            var janeOrder = await AddOrder(dbContext, "Jane");
            await AddOrderItem(dbContext, janeOrder.Id, new OrderItem()
            {
                ItemName = "Foobar",
                ItemPrice = 30
            });
            
            Console.WriteLine("Done");
        }

        private static async Task<Order> AddOrder(CosmosStoreDbContext context, string customerName)
        {
            var entity = await context.Set<Order>().AddAsync(new Order()
            {
                CustomerName = customerName
            });
            
            await context.SaveChangesAsync();
            
            entity.State = EntityState.Detached;
            return entity.Entity;
        }

        private static async Task AddOrderItem(CosmosStoreDbContext context, Guid orderId, OrderItem item)
        {
            var order = await context.Set<Order>().FindAsync(orderId);

            order.Items.Add(item);
            
            await context.SaveChangesAsync();
        }
    }
}