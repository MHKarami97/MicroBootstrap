namespace MicroBootstrap.RabbitMq
{
    public interface ICorrelationContextAccessor
    {
        object CorrelationContext { get; set; }
    }
}