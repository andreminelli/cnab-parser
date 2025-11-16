namespace CnabParser.Application.Services
{
    public class ImportResponse
    {
        public ImportResponse()
        {
        }

        public ImportResponse(string importId, int transactionCount)
        {
            ImportId = importId;
            TransactionCount = transactionCount;
        }

        public string ImportId { get; set; } = string.Empty;

        public int TransactionCount { get; set; }
    }
}
