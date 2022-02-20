using System.Linq.Expressions;

namespace SG4.Boilerplate.Data.Repositories.Base;

public interface IRepository<T> where T : class
{
    T? Find(Expression<Func<T, bool>> expression);
    T[] GetAll();
    //T[] Query(IRepositoryQuery<T> query);
}
