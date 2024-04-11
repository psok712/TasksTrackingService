using FluentMigrator.Runner;
using HomeworkApp.Dal.Extensions;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Utils.Extensions;
using HomeworkApp.Utils.Providers.Interfaces;
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
                        .AddUtils();

                    services.Replace(
                        new ServiceDescriptor(typeof(IDateTimeOffsetProvider), DateTimeOffsetProviderFaker.Object));
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