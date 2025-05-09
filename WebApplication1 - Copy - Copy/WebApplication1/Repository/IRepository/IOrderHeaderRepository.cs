using WebApplication1.Models;

namespace WebApplication1.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);


    }
}
