using System.Threading.Tasks;
using Lykke.Common.Log;
using Lykke.Job.CashOperationsHistoryWriter.Core.Repositories;
using Lykke.MatchingEngine.Connector.Models.Events;

namespace Lykke.Job.CashOperationsHistoryWriter.AzureRepositories
{
    public class CashOperationsRepository : ICashOperationsRepository
    {
        private readonly BatchSaver<CashInOutOperationEntity> _batchSaver;

        public CashOperationsRepository(ILogFactory logFactory, string connectionString)
        {
            _batchSaver = new BatchSaver<CashInOutOperationEntity>(
                connectionString,
                "OperationsCash",
                logFactory);
        }

        public async Task RegisterAsync(CashInEvent cashinEvent)
        {
            var byClient = CashInOutOperationEntity.ByClientId.FromMeModel(cashinEvent);
            var byDate = CashInOutOperationEntity.ByDate.FromMeModel(cashinEvent);

            await _batchSaver.AddAsync(byClient, byDate);
        }

        public async Task RegisterAsync(CashOutEvent cashoutEvent)
        {
            var byClient = CashInOutOperationEntity.ByClientId.FromMeModel(cashoutEvent);
            var byDate = CashInOutOperationEntity.ByDate.FromMeModel(cashoutEvent);

            await _batchSaver.AddAsync(byClient, byDate);
        }

        public void Start()
        {
            _batchSaver.Start();
        }

        public void Dispose()
        {
            _batchSaver.Dispose();
        }

        public void Stop()
        {
            _batchSaver.Stop();
        }
    }
}
