using Microsoft.EntityFrameworkCore.Storage;
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
    Task<int> CountAsync();
    Task<decimal> SumAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, decimal>> selector);
    Task<IEnumerable<TEntity>> GetLastSevenDaysTransactionsAsync();
    Task<IEnumerable<TEntity>> GetRecentUsersAsync(int count);
    Task<IEnumerable<TEntity>> GetRecentTransactionsAsync(int count);
    Task<IDbContextTransaction> BeginTransactionAsync();

}