namespace SG4.Boilerplate.Data.Repositories.Base;

public interface IIntegerKey<T> where T : class
{
    T? Find(int id);
}

public interface IStringKey<T> where T : class
{
    T? Find(string id);
}