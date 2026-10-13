namespace EcommerceBackend.Repositories
{
    // Generic repository interface for CRUD operations
    //the porpose of this interface is to provide a common set of methods for performing CRUD operations on entities of type T,
    //where T is a class. The interface defines methods for retrieving entities by their unique identifier (GetByIdAsync),
    //retrieving all entities (GetAllAsync), adding new entities (AddSync), updating existing entities (Update), removing entities (Remove),
    //and saving changes to the underlying data store (SaveChangesAsync).
    //By using this generic repository interface, developers can create concrete implementations for
    //specific entity types, promoting code reusability and maintainability in the application.
    public interface IGenericRepository<T , TKey>  where T : class
    {
        // Retrieves an entity of type T by its unique identifier asynchronously.
        Task<T?> GetByIdAsync(TKey id);
        // Retrieves all entities of type T from the data store asynchronously.
        Task<IEnumerable<T>> GetAllAsync();
        // Adds a new entity of type T to the data store asynchronously.
        Task AddAsync(T entity);
        // Updates an existing entity of type T in the data store asynchronously.
        void Update(T entity);
        // Removes an entity of type T from the data store asynchronously.
        void Remove (T entity);
        // Saves changes made to the data store asynchronously and returns a boolean indicating success or failure 
        Task<bool> SaveChangesAsync ();
    }
}
