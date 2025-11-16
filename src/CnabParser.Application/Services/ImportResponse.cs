
namespace CnabParser.Application.Services
{
    public class ImportResponse
    {
        public ImportResponse(string importId, int transactionCount)
        {
            ImportId = importId;
            TransactionCount = transactionCount;
        }

        public string ImportId { get; }
        public int TransactionCount { get; }
    }
}
