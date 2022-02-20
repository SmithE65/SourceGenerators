
using SG4.Boilerplate.Data.Models;
using SG4.Boilerplate.Data.Repositories.Base;
using System.Linq.Expressions;
namespace SG4.Boilerplate.Data.Repositories2;

#nullable enable

public interface IAddressRepository : IRepository<Address>, IIntegerKey<Address> { }

internal class AddressRepository : EfCoreRepositoryBase<Address>, IAddressRepository
{
    public AddressRepository(ApplicationDbContext context) : base(context) { }
    public Address? Find(int id) => Table.FirstOrDefault(x => x.AddressId == id);
    public Address? Find(Expression<Func<Address, bool>> expression) => Table.FirstOrDefault(expression);
    public Address[] GetAll() => Table.ToArray();

}
