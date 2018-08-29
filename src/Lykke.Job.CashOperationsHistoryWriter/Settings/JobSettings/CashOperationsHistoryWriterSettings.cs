namespace Lykke.Job.CashOperationsHistoryWriter.Settings.JobSettings
{
    public class CashOperationsHistoryWriterSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings Rabbit { get; set; }
    }
}
