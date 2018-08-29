using System;
using System.Linq;
using Lykke.MatchingEngine.Connector.Models.Events;
using Lykke.MatchingEngine.Connector.Models.Events.Common;
using Microsoft.WindowsAzure.Storage.Table;
using FeeType = Lykke.Job.CashOperationsHistoryWriter.Core.Domain.FeeType;

namespace Lykke.Job.CashOperationsHistoryWriter.AzureRepositories
{
    public class CashInOutOperationEntity : TableEntity
    {
        public DateTime DateTime { get; set; }
        public string AssetId { get; set; }
        public string ClientId { get; set; }
        public double Amount { get; set; }
        public double FeeSize { get; set; }
        public string FeeTypeText { get; set; }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        public static class ByClientId
        {
            public static string GeneratePartitionKey(string clientId)
            {
                return clientId;
            }

            public static CashInOutOperationEntity FromMeModel(CashInEvent cashinEvent)
            {
                var result = CashInOutOperationEntity.FromMeModel(cashinEvent);
                result.PartitionKey = GeneratePartitionKey(cashinEvent.CashIn.WalletId);
                result.RowKey = GenerateRowKey(cashinEvent.Header.MessageId);
                return result;
            }

            public static CashInOutOperationEntity FromMeModel(CashOutEvent cashoutEvent)
            {
                var result = CashInOutOperationEntity.FromMeModel(cashoutEvent);
                result.PartitionKey = GeneratePartitionKey(cashoutEvent.CashOut.WalletId);
                result.RowKey = GenerateRowKey(cashoutEvent.Header.MessageId);
                return result;
            }
        }

        public static class ByDate
        {
            public static string GeneratePartitionKey(DateTime dateTime)
            {
                return dateTime.ToString("yyyy-MM-dd");
            }

            public static CashInOutOperationEntity FromMeModel(CashInEvent cashinEvent)
            {
                var result = CashInOutOperationEntity.FromMeModel(cashinEvent);
                result.PartitionKey = GeneratePartitionKey(cashinEvent.Header.Timestamp);
                result.RowKey = GenerateRowKey(cashinEvent.Header.MessageId);
                return result;
            }

            public static CashInOutOperationEntity FromMeModel(CashOutEvent cashoutEvent)
            {
                var result = CashInOutOperationEntity.FromMeModel(cashoutEvent);
                result.PartitionKey = GeneratePartitionKey(cashoutEvent.Header.Timestamp);
                result.RowKey = GenerateRowKey(cashoutEvent.Header.MessageId);
                return result;
            }
        }

        private static CashInOutOperationEntity FromMeModel(CashInEvent cashinEvent)
        {
            var result = new CashInOutOperationEntity
            {
                DateTime = cashinEvent.Header.Timestamp,
                AssetId = cashinEvent.CashIn.AssetId,
                Amount = double.Parse(cashinEvent.CashIn.Volume),
                ClientId = cashinEvent.CashIn.WalletId,
            };
            if (cashinEvent.CashIn.Fees.Any())
            {
                var fee = cashinEvent.CashIn.Fees.First();
                result.FeeSize = double.Parse(fee.Instruction.Size);
                result.FeeTypeText = fee.Instruction.SizeType == FeeInstructionSizeType.Absolute
                    ? FeeType.Absolute.ToString()
                    : FeeType.Relative.ToString();
            }
            return result;
        }

        private static CashInOutOperationEntity FromMeModel(CashOutEvent cashoutEvent)
        {
            var result = new CashInOutOperationEntity
            {
                DateTime = cashoutEvent.Header.Timestamp,
                AssetId = cashoutEvent.CashOut.AssetId,
                Amount = double.Parse(cashoutEvent.CashOut.Volume),
                ClientId = cashoutEvent.CashOut.WalletId,
            };
            if (cashoutEvent.CashOut.Fees.Any())
            {
                var fee = cashoutEvent.CashOut.Fees.First();
                result.FeeSize = double.Parse(fee.Instruction.Size);
                result.FeeTypeText = fee.Instruction.SizeType == FeeInstructionSizeType.Absolute
                    ? FeeType.Absolute.ToString()
                    : FeeType.Relative.ToString();
            }
            return result;
        }
    }
}
