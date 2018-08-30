using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.CashOperationsHistoryWriter.Core.Repositories;
using Lykke.Job.CashOperationsHistoryWriter.Core.Services;
using Lykke.MatchingEngine.Connector.Models.Events;
using Lykke.MatchingEngine.Connector.Models.Events.Common;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using System;
using System.Threading.Tasks;

namespace Lykke.Job.CashOperationsHistoryWriter.RabbitSubscribers
{
    public class RabbitSubscriber : IStartStop
    {
        private readonly ILogFactory _logFactory;
        private readonly ICashOperationsRepository _cashOperationsRepository;
        private readonly string _connectionString;
        private readonly string _exchangeName;

        private RabbitMqSubscriber<CashInEvent> _cashinSubscriber;
        private RabbitMqSubscriber<CashOutEvent> _cashoutSubscriber;

        public RabbitSubscriber(
            ILogFactory logFactory,
            ICashOperationsRepository cashOperationsRepository,
            string connectionString,
            string exchangeName)
        {
            _logFactory = logFactory;
            _cashOperationsRepository = cashOperationsRepository;
            _connectionString = connectionString;
            _exchangeName = exchangeName;
        }

        public void Start()
        {
            _cashOperationsRepository.Start();

            var cashinSettings = RabbitMqSubscriptionSettings
                .CreateForSubscriber(_connectionString, _exchangeName, "cashinhistorywriter")
                .MakeDurable()
                .UseRoutingKey(((int)MessageType.CashIn).ToString());

            _cashinSubscriber = new RabbitMqSubscriber<CashInEvent>(
                    _logFactory,
                    cashinSettings,
                    new ResilientErrorHandlingStrategy(
                        _logFactory,
                        cashinSettings,
                        TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, cashinSettings)))
                .SetMessageDeserializer(new ProtobufMessageDeserializer<CashInEvent>())
                .Subscribe(ProcessCashinAsync)
                .CreateDefaultBinding()
                .SetConsole(new LogToConsole())
                .Start();

            var cashoutSettings = RabbitMqSubscriptionSettings
                .CreateForSubscriber(_connectionString, _exchangeName, "cashouthistorywriter")
                .MakeDurable()
                .UseRoutingKey(((int)MessageType.CashOut).ToString());

            _cashoutSubscriber = new RabbitMqSubscriber<CashOutEvent>(
                    _logFactory,
                    cashoutSettings,
                    new ResilientErrorHandlingStrategy(
                        _logFactory,
                        cashoutSettings,
                        TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, cashoutSettings)))
                .SetMessageDeserializer(new ProtobufMessageDeserializer<CashOutEvent>())
                .Subscribe(ProcessCashoutAsync)
                .CreateDefaultBinding()
                .SetConsole(new LogToConsole())
                .Start();
        }

        private async Task ProcessCashinAsync(CashInEvent arg)
        {
            await _cashOperationsRepository.RegisterAsync(arg);
        }

        private async Task ProcessCashoutAsync(CashOutEvent arg)
        {
            await _cashOperationsRepository.RegisterAsync(arg);
        }

        public void Dispose()
        {
            Stop();

            Parallel.ForEach(new IDisposable[] { _cashinSubscriber, _cashoutSubscriber }, i => i?.Dispose());
        }

        public void Stop()
        {
            Parallel.ForEach(new IStopable[] {_cashinSubscriber, _cashoutSubscriber}, i => i?.Stop());

            _cashOperationsRepository.Stop();
        }
    }
}
