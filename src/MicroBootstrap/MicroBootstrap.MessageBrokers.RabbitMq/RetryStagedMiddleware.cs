using System.Threading;
using System.Threading.Tasks;
using RawRabbit.Common;
using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;

namespace MicroBootstrap.MessageBrokers.RabbitMQ
{
    internal class RetryStagedMiddleware : StagedMiddleware
    {
        public override string StageMarker { get; } = RawRabbit.Pipe.StageMarker.MessageDeserialized;

        public override async Task InvokeAsync(IPipeContext context,
            CancellationToken token = new CancellationToken())
        {
            var retry = context.GetRetryInformation();
            if (context.GetMessageContext() is CorrelationContext message)
            {
                message.Retries = retry.NumberOfRetries;
            }

            await Next.InvokeAsync(context, token);
        }
    }
}