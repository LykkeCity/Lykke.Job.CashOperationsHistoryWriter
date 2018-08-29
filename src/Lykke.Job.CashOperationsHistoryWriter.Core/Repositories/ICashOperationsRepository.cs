using Lykke.MatchingEngine.Connector.Models.Events;
using System.Threading.Tasks;

namespace Lykke.Job.CashOperationsHistoryWriter.Core.Repositories
{
    public interface ICashOperationsRepository
    {
        Task RegisterAsync(CashInEvent cashinEvent);
        Task RegisterAsync(CashOutEvent cashoutEvent);
    }
}
