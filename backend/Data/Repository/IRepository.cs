using MediHub.Web.Models.Common;
using System.Linq.Expressions;

namespace MediHub.Web.Data.Repository
{
    public interface IRepository
    {
        Task AddAsync<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task AddAsync<TEntity>(TEntity entity) where TEntity : class, IEntity<Guid>;

        Task AddRangeAsync<TEntity, TKey>(IEnumerable<TEntity> entities) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity<Guid>;

        Task DeleteAsync<TEntity, TKey>(params object[] ids) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task DeleteAsync<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task DeleteAsync<TEntity>(params object[] ids) where TEntity : class, IEntity<Guid>;

        Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class, IEntity<Guid>;

        Task DeleteRangeAsync<TEntity, TKey>(IEnumerable<TEntity> entities) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task DeleteRangeAsync<TEntity, TKey>(IEnumerable<object> ids) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task DeleteRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity<Guid>;

        Task DeleteRangeAsync<TEntity>(IEnumerable<object> ids) where TEntity : class, IEntity<Guid>;

        Task<IEnumerable<TEntity>> FindAllAsync<TEntity, TKey>(Expression<Func<TEntity, bool>> where = null, Expression<Func<TEntity, TEntity>> selector = null, IEnumerable<string> includes = null) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task<IEnumerable<TEntity>> FindAllAsync<TEntity, TKey>(IQueryable<TEntity> query, Expression<Func<TEntity, TEntity>> selector = null) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(Expression<Func<TEntity, bool>> where = null, Expression<Func<TEntity, TEntity>> selector = null, IEnumerable<string> includes = null) where TEntity : class, IEntity<Guid>;

        Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(IQueryable<TEntity> query, Expression<Func<TEntity, TEntity>> selector = null) where TEntity : class, IEntity<Guid>;

        Task<TEntity> FindAsync<TEntity, TKey>(TKey id, IEnumerable<string> includes = null) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;
        Task<TEntity> FindAsync<TEntity, TKey>(params object[] ids) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task<TEntity> FindAsync<TEntity, TKey>(Expression<Func<TEntity, bool>> where = null, IEnumerable<string> includes = null) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task<bool> CheckContains<TEntity, TKey>(Expression<Func<TEntity, bool>> where = null, IEnumerable<string> includes = null) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task<bool> CheckContains<TEntity>(Expression<Func<TEntity, bool>> where = null)
            where TEntity : class, IEntity<Guid>;

        Task<TEntity> FindAsync<TEntity>(params object[] ids) where TEntity : class, IEntity<Guid>;

        Task<TEntity> FindAsync<TEntity>(Guid id, IEnumerable<string> includes = null) where TEntity : class, IEntity<Guid>;

        Task<TEntity> FindAsync<TEntity>(Expression<Func<TEntity, bool>> where = null, IEnumerable<string> includes = null) where TEntity : class, IEntity<Guid>;

        IQueryable<TEntity> GetQueryable<TEntity, TKey>() where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class, IEntity<Guid>;

        Task SaveChangeAsync();

        Task UpdateAsync<TEntity, TKey>(TEntity entity) where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class, IEntity<Guid>;
        Task FindAllAsync<T>(Guid id);
    }
}
