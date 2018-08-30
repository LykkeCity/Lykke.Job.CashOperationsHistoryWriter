using Lykke.MatchingEngine.Connector.Models.Events;
using System.Threading.Tasks;
using Lykke.Job.CashOperationsHistoryWriter.Core.Services;

namespace Lykke.Job.CashOperationsHistoryWriter.Core.Repositories
{
    public interface ICashOperationsRepository : IStartStop
    {
        Task RegisterAsync(CashInEvent cashinEvent);
        Task RegisterAsync(CashOutEvent cashoutEvent);
    }
}
