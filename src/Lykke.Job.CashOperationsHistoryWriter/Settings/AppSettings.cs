using Lykke.Job.CashOperationsHistoryWriter.Settings.JobSettings;
using Lykke.Job.CashOperationsHistoryWriter.Settings.SlackNotifications;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CashOperationsHistoryWriter.Settings
{
    public class AppSettings
    {
        public CashOperationsHistoryWriterSettings CashOperationsHistoryWriterJob { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }

        [Optional]
        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }
    }
}
