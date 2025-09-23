namespace EVCS.Repositories.HuyCG.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Synchronous methods
        List<T> GetAll();
        T GetById(int id);
        T GetById(string code);
        T GetById(Guid code);
        void Create(T entity);
        void Update(T entity);
        bool Remove(T entity);
        int Save();

        // Asynchronous methods
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<T> GetByIdAsync(string code);
        Task<T> GetByIdAsync(Guid code);
        Task<int> CreateAsync(T entity);
        Task<int> UpdateAsync(T entity);
        Task<bool> RemoveAsync(T entity);
        Task<int> SaveAsync();

        // Prepare methods for batch operations (without immediate save)
        void PrepareCreate(T entity);
        void PrepareUpdate(T entity);
        void PrepareRemove(T entity);
    }
}