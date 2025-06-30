using VanQuangTin_2280603267_Lab04.Models;

namespace VanQuangTin_2280603267_Lab04.Repositories
{
    public interface IProductRepository
    {
        IQueryable<Product> GetAll();
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<List<Product>> SearchByNameAsync(string keyword);
    }
}
