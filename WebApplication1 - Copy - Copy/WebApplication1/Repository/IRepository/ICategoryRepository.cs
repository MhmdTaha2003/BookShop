using WebApplication1.Models;

namespace WebApplication1.Repository.IRepository
{
    public interface ICategoryRepository  : IRepository<Category>
    {
        void Update(Category obj);
}
    
}
