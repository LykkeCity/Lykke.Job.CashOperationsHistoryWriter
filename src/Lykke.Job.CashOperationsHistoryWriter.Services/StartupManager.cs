using Autofac;
using Lykke.Job.CashOperationsHistoryWriter.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.CashOperationsHistoryWriter.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly List<IStartable> _startables = new List<IStartable>();

        public StartupManager(IEnumerable<IStartStop> startables)
        {
            _startables.AddRange(startables);
        }

        public Task StartAsync()
        {
            Parallel.ForEach(_startables, i => i.Start());

            return Task.CompletedTask;
        }
    }
}
