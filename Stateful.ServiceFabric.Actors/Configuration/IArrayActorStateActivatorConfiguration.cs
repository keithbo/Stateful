namespace Stateful.ServiceFabric.Actors.Configuration
{
    public interface IArrayActorStateActivatorConfiguration : IActorStateActivatorConfiguration
    {
        long Length { get; set; }
    }
}