using Lykke.Job.CashOperationsHistoryWriter.Core.Repositories;
using Lykke.MatchingEngine.Connector.Models.Events;
using System.Threading.Tasks;

namespace Lykke.Job.CashOperationsHistoryWriter.AzureRepositories
{
    public class CashOperationsRepository : ICashOperationsRepository
    {
        private readonly IBatchSaver<CashInOutOperationEntity> _batchSaver;

        public CashOperationsRepository(IBatchSaver<CashInOutOperationEntity> batchSaver)
        {
            _batchSaver = batchSaver;
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
    }
}
