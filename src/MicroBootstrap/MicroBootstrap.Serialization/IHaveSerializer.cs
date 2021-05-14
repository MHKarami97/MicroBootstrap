namespace MicroBootstrap.Serialization
{
    public interface IHaveSerializer
    {
         ISerializer Serializer { get; }
    }
}