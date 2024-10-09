using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using BE_EXE201.Entities;

namespace BE_EXE201.Repositories;

public class GenericRepository<TEntity, TKey> : IRepository<TEntity, TKey>
where TEntity : class
{
    private readonly AppDbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
    }

    public IQueryable<TEntity> GetAll()
        => _dbSet;

    public IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> predicate)
        => _dbSet.Where(predicate);

    public async Task<TEntity?> GetByIdAsync(TKey id)
        => await _dbSet.FindAsync(id);

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        var entityEntry = await _dbContext.Set<TEntity>().AddAsync(entity);
        return entityEntry.Entity;
    }

    public TEntity Update(TEntity entity)
    {
        var entityEntry = _dbContext.Set<TEntity>().Update(entity);
        return entityEntry.Entity;
    }

    public TEntity Remove(TKey id)
    {
        var entity = GetByIdAsync(id).Result;
        var entityEntry = _dbContext.Set<TEntity>().Remove(entity!);
        return entityEntry.Entity;
    }

    public Task<int> Commit() => _dbContext.SaveChangesAsync();

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
    public async Task<decimal> SumAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, decimal>> selector)
    {
        return await _dbSet.Where(predicate).SumAsync(selector);
    }
    public async Task<IEnumerable<TEntity>> GetLastSevenDaysTransactionsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var sevenDaysAgo = DateTime.Now.Date.AddDays(-6);
        return await _dbSet.Where(predicate).Where(e => EF.Property<DateTime>(e, "CreatedDate") >= sevenDaysAgo).ToListAsync();
    }
    public async Task<IEnumerable<TEntity>> GetRecentUsersAsync(int count)
    {
        return await _dbSet.OrderByDescending(e => EF.Property<DateTime>(e, "CreateDate"))
                           .Take(count)
                           .ToListAsync();
    }
}