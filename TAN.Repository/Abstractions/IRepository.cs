using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.Repository.Abstractions
{
    public interface IRepository<TEntity> where TEntity : class
    {
        void Add(TEntity model);
        void AddRange(List<TEntity> model);
        void Update(TEntity model);
        IEnumerable<TEntity> GetAll();
        TEntity GetById(object Id);
        void Modify(TEntity model);
        void Delete(TEntity model);
        void DeleteById(object Id);
        void RemoveRange(List<TEntity> model);
        Task<IEnumerable<TEntity>> ExecWithStoreProcedure(string query, params object[] parameters);
        Task ExecuteWithStoreProcedure(string query, params object[] parameters);


    }
}
