namespace CnabParser.Application.Services;

public class ImportDataResponse
{
    public ImportDataResponse()
    {
    }

    public ImportDataResponse(IReadOnlyList<StoreResponse> data)
    {
        Data = data;
    }

    public IReadOnlyList<StoreResponse> Data { get; set; } = [];
}
