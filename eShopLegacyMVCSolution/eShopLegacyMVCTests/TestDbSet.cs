using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace eShopLegacyMVCTests
{
    /// <summary>
    /// In-memory <see cref="DbSet{TEntity}"/> test double backed by a list. Supports
    /// LINQ querying (including EF6 Include, which is treated as a no-op) so services
    /// that query the context can be unit tested without a database.
    /// </summary>
    public class TestDbSet<TEntity> : DbSet<TEntity>, IQueryable<TEntity>, IEnumerable<TEntity>
        where TEntity : class
    {
        private readonly List<TEntity> _data;
        private readonly IQueryable _query;

        public TestDbSet(IEnumerable<TEntity> data = null)
        {
            _data = data == null ? new List<TEntity>() : new List<TEntity>(data);
            _query = _data.AsQueryable();
        }

        public IReadOnlyList<TEntity> Items => _data;

        public override TEntity Add(TEntity entity)
        {
            _data.Add(entity);
            return entity;
        }

        public override TEntity Remove(TEntity entity)
        {
            _data.Remove(entity);
            return entity;
        }

        public override TEntity Attach(TEntity entity)
        {
            _data.Add(entity);
            return entity;
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

        Type IQueryable.ElementType => _query.ElementType;

        Expression IQueryable.Expression => _query.Expression;

        IQueryProvider IQueryable.Provider => _query.Provider;
    }
}
