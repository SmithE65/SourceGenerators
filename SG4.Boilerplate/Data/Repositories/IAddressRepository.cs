using SG4.Boilerplate.Data.Models;

namespace SG4.Boilerplate.Data.Repositories;

public partial interface IAddressRepository
{
    Address[] GetFive();
}

internal partial class AddressRepository
{
    public Address[] GetFive() => Table.OrderByDescending(x => x.ModifiedDate).Take(5).ToArray();
}
