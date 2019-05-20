using TobinTaxer.Models;

namespace TobinTaxer.Clients
{
    public interface IRabbitMqClient
    {
        void SendMessage(HistoryMessage historyMessage);
    }
}