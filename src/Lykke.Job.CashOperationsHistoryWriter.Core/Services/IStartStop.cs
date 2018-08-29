using Autofac;
using Common;

namespace Lykke.Job.CashOperationsHistoryWriter.Core.Services
{
    public interface IStartStop : IStartable, IStopable
    {
    }
}
