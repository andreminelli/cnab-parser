namespace CnabParser.Core.Entities;

public class Store
{
    public int Id { get; protected set; }

    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
}
