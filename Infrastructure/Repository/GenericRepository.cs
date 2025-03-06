using Application.Service;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class GenericRepository<T> : IGenericRepository<T>, IDisposable where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _table;
    private bool _disposed = false;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _table = _context.Set<T>();
    }

    public async Task AddAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _table.AddAsync(entity);
    }

    public void Delete(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _table.Remove(entity);
    }

    public async Task DeleteByIdAsync(object id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            Delete(entity);
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _table.AsNoTracking().ToListAsync();
    }

    public IEnumerable<T> GetAll()
    {
        return _table.AsNoTracking().ToList();
    }

    public T GetById(object id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return _table.Find(id);
    }

    public async Task<T> GetByIdAsync(object id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return await _table.FindAsync(id);
    }

    public void Update(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _table.Update(entity);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
