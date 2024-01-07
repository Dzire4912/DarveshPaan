using Microsoft.EntityFrameworkCore;
using TAN.Repository.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace TAN.Repository.Implementations
{
    [ExcludeFromCodeCoverage]
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected DbContext db { get; set; }
        

        public void Add(TEntity model)
        {
            db.Set<TEntity>().Add(model);
        }

        public void AddRange(List<TEntity> model)
        {
            db.Set<TEntity>().AddRange(model);
        }

        public void Delete(TEntity model)
        {
            db.Set<TEntity>().Remove(model);
        }

        public void DeleteById(object Id)
        {
            TEntity entity = db.Set<TEntity>().Find(Id);
            this.Delete(entity);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return db.Set<TEntity>().AsNoTracking().ToList();
        }

        public TEntity GetById(object Id)
        {
            return db.Set<TEntity>().Find(Id);
        }


        public void Modify(TEntity model)
        {
            db.Entry<TEntity>(model).State = EntityState.Modified;
        }

        public void Update(TEntity model)
        {
            db.Set<TEntity>().Update(model);
        }
        public void RemoveRange(List<TEntity> model)
        {
            db.Set<TEntity>().RemoveRange(model);
        }
        public async Task<IEnumerable<TEntity>> ExecWithStoreProcedure(string query, params object[] parameters)
        {
            db.Database.SetCommandTimeout(TimeSpan.FromSeconds(800));
            return await db.Set<TEntity>().FromSqlRaw(query, parameters).ToListAsync();
        }

        // Fire and forget
        public async Task ExecuteWithStoreProcedure(string query, params object[] parameters)
        {
            db.Database.SetCommandTimeout(TimeSpan.FromSeconds(800));
            db.Database.ExecuteSqlRaw(query, parameters);
        }


    }
}
