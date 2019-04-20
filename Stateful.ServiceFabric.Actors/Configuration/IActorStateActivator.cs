namespace Stateful.ServiceFabric.Actors.Configuration
{
    using Microsoft.ServiceFabric.Actors.Runtime;

    public interface IActorStateActivator
    {
        IState Resolve(IActorStateManager stateManager, IStateKey key);
    }
}