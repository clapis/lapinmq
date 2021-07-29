using LapinMQ.StateMachines;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace LapinMQ
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            EnsureDbIndex(host);
                
            host.Run();
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        
        private static void EnsureDbIndex(IHost host)
        {
            using var scope = host.Services.CreateScope();

            var collection = scope.ServiceProvider.GetRequiredService<IMongoCollection<OrderState>>();
            
            collection.Indexes.CreateOne(new CreateIndexModel<OrderState>(
                Builders<OrderState>.IndexKeys.Ascending(c => c.OrderNumber),
                new CreateIndexOptions { Unique = true }));
        }

    }
}