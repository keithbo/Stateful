namespace Stateful.ServiceFabric.Actors.Configuration
{
    public interface IActorStateActivatorConfiguration
    {
        IStateKey Key { get; set; }

        IActorStateActivator Build();
    }
}