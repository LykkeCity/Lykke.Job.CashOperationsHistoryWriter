using System.Threading.Tasks;

namespace Lykke.Job.CashOperationsHistoryWriter.Core.Repositories
{
    public interface IBatchSaver<T>
    {
        Task AddAsync(params T[] items);
    }
}
