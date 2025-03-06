namespace Application.Service;

public interface IGenericRepository<T> : IDisposable where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    IEnumerable<T> GetAll();
    T GetById(object id);
    Task<T> GetByIdAsync(object id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task DeleteByIdAsync(object id);
}
