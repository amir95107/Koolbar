﻿using Microsoft.EntityFrameworkCore;

namespace Koolbar.Repositories;
public interface IBaseRepository<TEntity, TKey>
    where TEntity : class
{
    DbSet<TEntity> Entities { get; }
    Task<TEntity> FindAsync(TKey id);
    Task<bool> ExistsAsync(TKey id);
    Task<int> CountAsync();
    Task<List<TEntity>> GetAllAsync(int skip, int take);
    Task AddAsync(TEntity entity);
    void Modify(TEntity entity);
    Task Remove(TKey id);
    void Remove(TEntity entity);

    Task SaveChangesAsync();
    void SaveChanges();
}

