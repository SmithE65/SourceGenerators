using Microsoft.EntityFrameworkCore;

namespace SG4.Boilerplate.Data.Repositories.Base;

public interface IRepository<T> where T : class
{
    T? Find(int id);
    T[] GetAll();
    //T[] Query(IRepositoryQuery<T> query);
}

//public interface IRepositoryQuery<T>
//{

//}

internal abstract class EfCoreRepositoryBase<T> : IRepository<T> where T : class
{
    protected DbContext DbContext { get; }
    protected DbSet<T> Table { get; }

    public EfCoreRepositoryBase(DbContext context)
    {
        DbContext = context;
        Table = context.Set<T>();
    }

    public abstract T? Find(int id);
    public abstract T[] GetAll();
}
