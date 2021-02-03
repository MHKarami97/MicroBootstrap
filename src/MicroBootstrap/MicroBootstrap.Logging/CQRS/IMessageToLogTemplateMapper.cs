namespace MicroBootstrap.Logging.CQRS
{
    public interface IMessageToLogTemplateMapper
    {
        HandlerLogTemplate Map<TMessage>(TMessage message) where TMessage : class;
    }
}