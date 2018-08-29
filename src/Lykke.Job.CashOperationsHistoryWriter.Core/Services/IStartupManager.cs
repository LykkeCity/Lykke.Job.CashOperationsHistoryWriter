using System.Threading.Tasks;

namespace Lykke.Job.CashOperationsHistoryWriter.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}