using Autofac;
using Lykke.Job.CashOperationsHistoryWriter.AzureRepositories;
using Lykke.Job.CashOperationsHistoryWriter.Core.Repositories;
using Lykke.Job.CashOperationsHistoryWriter.Core.Services;
using Lykke.Job.CashOperationsHistoryWriter.Services;
using Lykke.Job.CashOperationsHistoryWriter.Settings.JobSettings;
using Lykke.Job.CashOperationsHistoryWriter.RabbitSubscribers;

namespace Lykke.Job.CashOperationsHistoryWriter.Modules
{
    public class JobModule : Module
    {
        private readonly CashOperationsHistoryWriterSettings _settings;

        public JobModule(CashOperationsHistoryWriterSettings settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<BatchSaver<CashInOutOperationEntity>>()
                .As<IBatchSaver<CashInOutOperationEntity>>()
                .As<IStartStop>()
                .WithParameter("connectionString", _settings.Db.DataConnString)
                .WithParameter("tableName", "OperationsCash");

            builder.RegisterType<CashOperationsRepository>()
                .As<ICashOperationsRepository>()
                .SingleInstance();

            builder.RegisterType<RabbitSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _settings.Rabbit.ConnectionString)
                .WithParameter("exchangeName", _settings.Rabbit.ExchangeName);
        }
    }
}
