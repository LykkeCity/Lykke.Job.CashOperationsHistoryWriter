using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.CashOperationsHistoryWriter.Core.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Job.CashOperationsHistoryWriter.Core.Repositories;

namespace Lykke.Job.CashOperationsHistoryWriter.AzureRepositories
{
    public class BatchSaver<T> : TimerPeriod, IStartStop, IBatchSaver<T>
        where T : TableEntity
    {
        private const int _tableServiceBatchMaximumOperations = 100;
        private const int _maxNumberOfTasks = 50; //200
        private const int _warningQueueCount = 2000;

        private readonly CloudTable _table;
        private readonly ILog _log; 
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private List<T> _queue = new List<T>();

        public BatchSaver(
            string connectionString,
            string tableName,
            ILogFactory logFactory)
            : base(TimeSpan.FromSeconds(5), logFactory)
        {
            var cloudAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = cloudAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(tableName);
            _log = logFactory.CreateLog(this);
        }

        public async Task AddAsync(params T[] items)
        {
            await _lock.WaitAsync();
            try
            {
                _queue.AddRange(items);

                if (_queue.Count >= _warningQueueCount)
                    _log.Warning($"Queue has {_queue.Count} items");
            }
            finally
            {
                _lock.Release();
            }
        }

        public override async Task Execute()
        {
            List<T> batch;
            await _lock.WaitAsync();
            try
            {
                if (_queue.Count == 0)
                    return;

                batch = _queue;
                _queue = new List<T>(batch.Count);
            }
            finally
            {
                _lock.Release();
            }

            int taskCount = 0;
            var batchTasks = new List<Task<IList<TableResult>>>();

            var partitionsDict = new Dictionary<string, List<T>>();
            foreach (var item in batch)
            {
                if (partitionsDict.ContainsKey(item.PartitionKey))
                    partitionsDict[item.PartitionKey].Add(item);
                else
                    partitionsDict.Add(item.PartitionKey, new List<T> { item });
            }

            foreach (var partitionItems in partitionsDict)
            {
                for (var i = 0; i < partitionItems.Value.Count; i += _tableServiceBatchMaximumOperations)
                {
                    var batchItems = partitionItems.Value.GetRange(i, Math.Min(_tableServiceBatchMaximumOperations, partitionItems.Value.Count - i));

                    var batchOp = new TableBatchOperation();
                    foreach (var item in batchItems)
                    {
                        batchOp.InsertOrMerge(item);
                    }

                    var task = _table.ExecuteBatchAsync(batchOp);
                    batchTasks.Add(task);
                    ++taskCount;

                    if (taskCount >= _maxNumberOfTasks)
                    {
                        await Task.WhenAll(batchTasks);
                        batchTasks.Clear();
                        taskCount = 0;
                    }
                }
            }

            if (batchTasks.Count > 0)
                await Task.WhenAll(batchTasks);
        }
    }
}
