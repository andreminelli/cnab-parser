using System.ComponentModel;

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

        [Description("Identifier to query data saved")]
        public string ImportId { get; set; } = string.Empty;

        [Description("Number of transactions read successfully and saved")]
        public int TransactionCount { get; set; }
    }
}
