using System.Threading.Tasks;
using MongoDB.Driver;

namespace MicroBootstrap.Mongo
{
    public interface IMongoSessionFactory
    {
        Task<IClientSessionHandle> CreateAsync();
    }
}