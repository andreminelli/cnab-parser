namespace CnabParser.Application.Services;

public class ImportDataResponse(IReadOnlyList<StoreResponse> data)
{
    public IReadOnlyList<StoreResponse> Data { get; } = data;
}
