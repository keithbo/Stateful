namespace Stateful.ServiceFabric.Actors.Configuration
{
    using Microsoft.ServiceFabric.Actors.Runtime;

    public interface IActorStateActivator
    {
        IStateKey Key { get; }

        IState Resolve(IActorStateManager stateManager);
    }
}