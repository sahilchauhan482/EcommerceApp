using Microsoft.EntityFrameworkCore;
using Project_ecommerce_1.Data;
using Project_ecommerce_1.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Project_ecommerce_1.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        public readonly ApplicationDbContext _Context;
        internal DbSet<T> dbset;
        public Repository(ApplicationDbContext context)
        { 
            _Context = context;
            dbset=context.Set<T>();
                
        }


        public void Add(T entity)
        {
            dbset.Add(entity);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> filter = null, string IncludeProperties = null)
        {
            IQueryable<T> query = dbset;
            if (filter != null)
                query = query.Where(filter);
            if (IncludeProperties != null)
            {
                foreach (var incprop in IncludeProperties.Split(new[] {','},StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(incprop);
                }
            }
            return query.FirstOrDefault();
        }

        public T Get(int id)
        {
            return dbset.Find(id);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderby = null, string IncludeProperties = null, int? take = null)
        {
            IQueryable<T> query = dbset;
            if (filter != null)
                query=query.Where(filter);
            if(IncludeProperties !=null)
            {
                foreach (var incprop in IncludeProperties.Split(new [] {','},StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(incprop);
                }
            }
            if (orderby != null)
                return orderby(query).ToList();
            if (take.HasValue)
                query = query.Take(take.Value);
            return query.ToList();
        }

        public void Remove(T entity)
        {
            dbset.Remove(entity);
        }

        public void Remove(int id)
        {
            dbset.Remove(Get(id));
        }

        public void RemoveRange(IEnumerable<T> values)
        {
            dbset.RemoveRange(values);
        }

        public void Update(T entity)
        {
            _Context.ChangeTracker.Clear();
            dbset.Update(entity);
        }

       
    }

}
