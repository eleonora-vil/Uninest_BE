using System.Linq.Expressions;

namespace BE_EXE201.Repositories;

public interface IRepository<TEntity, in TKey>
{
    IQueryable<TEntity> GetAll();
    IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<TEntity> AddAsync(TEntity entity);
    TEntity Update(TEntity entity);
    TEntity Remove(TKey id);
    Task<int> Commit();
}