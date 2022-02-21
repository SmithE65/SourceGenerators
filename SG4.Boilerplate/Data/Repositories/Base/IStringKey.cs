namespace SG4.Boilerplate.Data.Repositories.Base;

public interface IStringKey<T> where T : class
{
    T? Find(string id);
}