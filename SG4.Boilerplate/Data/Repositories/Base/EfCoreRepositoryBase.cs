using Microsoft.EntityFrameworkCore;

namespace SG4.Boilerplate.Data.Repositories.Base;

internal abstract class EfCoreRepositoryBase<T> where T : class
{
    protected DbContext DbContext { get; }
    protected DbSet<T> Table { get; }

    public EfCoreRepositoryBase(DbContext context)
    {
        DbContext = context;
        Table = context.Set<T>();
    }
}
