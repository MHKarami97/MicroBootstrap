using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MicroBootstrap.Common;

namespace MicroBootstrap.Queries.Dispatchers
{
    internal class InMemoryQueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceScopeFactory _serviceFactory;

        public InMemoryQueryDispatcher(IServiceScopeFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query)
        {
            using var scope = _serviceFactory.CreateScope();
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            var handler = scope.ServiceProvider.GetRequiredService(handlerType);
            if (handler is null)
                throw new System.Exception($"can't find corresponding handler for {handlerType.Name} handler type");

            MethodInfo method = handlerType.GetMethod("HandleAsync");
            var res = await method.InvokeAsync(handler, query);
            return (TResult)res;
        }

        public async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query) where TQuery : class, IQuery<TResult>
        {
            // https://www.blog.jamesmichaelhickey.com/NET-Core-Dependency-Injection/
            using var scope = _serviceFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            if (handler is null)
                throw new System.Exception($"can't find corresponding handler for {typeof(IQueryHandler<TQuery, TResult>).Name} handler type");
            return await handler.HandleAsync(query);
        }
    }
}