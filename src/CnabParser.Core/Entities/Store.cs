namespace CnabParser.Core.Entities;

public class Store
{
    public int Id { get; protected set; }

    public required string Name { get; set; }
    public required string OwnerName { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = [];

    public override bool Equals(object? obj)
    {
        if (obj is Store storeObj)
        {
            return Equals(storeObj);
        }

        return false;
    }

    public bool Equals(Store obj)
        => Name.Equals(obj.Name, StringComparison.Ordinal) &&
           OwnerName.Equals(obj.OwnerName, StringComparison.Ordinal);

    public override int GetHashCode()
        => HashCode.Combine(Name, OwnerName);
}
