using System.Threading.Tasks;

namespace Lykke.Job.CashOperationsHistoryWriter.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
