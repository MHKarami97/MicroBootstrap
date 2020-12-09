using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MicroBootstrap.Queries.Dispatchers;
namespace MicroBootstrap.Queries.Dispatchers
{
    public class InMemoryQueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceScopeFactory _serviceFactory;
        public InMemoryQueryDispatcher(IServiceScopeFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query)
        {
            // https://www.blog.jamesmichaelhickey.com/NET-Core-Dependency-Injection/
            using var scope = _serviceFactory.CreateScope();
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            dynamic handler = scope.ServiceProvider.GetRequiredService(handlerType);
            return await handler.HandleAsync((dynamic)query);
        }

        public async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query) where TQuery : class, IQuery<TResult>
        {
            using var scope = _serviceFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            return await handler.HandleAsync(query);
        }
    }
}