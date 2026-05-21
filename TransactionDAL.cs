using System.Collections.Generic;

namespace PersonalFinanceApp.DAL
{
    public interface IDataAccess<T>
    {
        bool Insert(T item);
        bool Update(T item);
        bool Delete(int id);
        List<T> GetAll();
    }
}
