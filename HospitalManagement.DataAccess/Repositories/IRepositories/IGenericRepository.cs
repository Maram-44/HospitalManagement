using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.DataAccess.Repositories.IRepository
{
    public interface IGenericRepository<T> where T : class
    {
        T FindById(int id);

        T SelectOne(Expression<Func<T, bool>> match);

        IEnumerable<T> FindAll();

        IEnumerable<T> FindAll(params string[] agers);

        Task<T> FindByIdAsync(int id);

        Task<IEnumerable<T>> FindAllAsync();

        Task<IEnumerable<T>> FindAllAsync(params string[] agers);

        void AddOne(T myItem);

        void UpdateOne(T myItem);

        void DeleteOne(T myItem);

        void AddList(IEnumerable<T> myList);

        void UpdateList(IEnumerable<T> myList);

        void DeleteList(IEnumerable<T> myList);
    }
}
