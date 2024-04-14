using FluentMigrator.Runner;
using HomeworkApp.Bll.Extensions;
using HomeworkApp.Bll.Services.Interfaces;
using HomeworkApp.Dal.Extensions;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using HomeworkApp.Utils.Extensions;
using HomeworkApp.Utils.Providers.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;

namespace HomeworkApp.IntegrationTests.Fixtures
{
    public class TestFixture
    {
        public readonly Mock<IDateTimeOffsetProvider> DateTimeOffsetProviderFaker = new();
        
        public IUserRepository UserRepository { get; }
        
        public ITaskRepository TaskRepository { get; }
        
        public ITaskLogRepository TaskLogRepository { get; }
        
        public ITakenTaskRepository TakenTaskRepository { get; }
        
        public ITaskCommentRepository TaskCommentRepository { get; }
        
        public IUserScheduleRepository UserScheduleRepository { get; }
        
        public IUserRateLimitRepository UserRateLimitRepository { get; }
        
        public IDistributedCache DistributedCache { get; }
        
        public ITaskService TaskService { get; }
        

        public TestFixture()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDalInfrastructure(config)
                        .AddDalRepositories()
                        .AddUtils()
                        .AddBllServices();

                    services.Replace(
                        new ServiceDescriptor(typeof(IDateTimeOffsetProvider), DateTimeOffsetProviderFaker.Object));
                    
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = 
                            config.GetRequiredSection(nameof(DalOptions)).Get<DalOptions>()!.RedisConnectionString;
                    });
                })
                .Build();
            
            ClearDatabase(host);
            host.MigrateUp();

            var scope = host.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            UserRepository = serviceProvider.GetRequiredService<IUserRepository>();
            TaskRepository = serviceProvider.GetRequiredService<ITaskRepository>();
            TaskLogRepository = serviceProvider.GetRequiredService<ITaskLogRepository>();
            TakenTaskRepository = serviceProvider.GetRequiredService<ITakenTaskRepository>();
            UserScheduleRepository = serviceProvider.GetRequiredService<IUserScheduleRepository>();
            TaskCommentRepository = serviceProvider.GetRequiredService<ITaskCommentRepository>();
            DistributedCache = serviceProvider.GetRequiredService<IDistributedCache>();
            UserRateLimitRepository = serviceProvider.GetRequiredService<IUserRateLimitRepository>();
            TaskService = serviceProvider.GetRequiredService<ITaskService>();
            
            FluentAssertionOptions.UseDefaultPrecision();
        }

        private static void ClearDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateDown(0);
        }
    }
}